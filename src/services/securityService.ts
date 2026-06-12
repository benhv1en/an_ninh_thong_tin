import * as forge from 'node-forge';

const ACTIVE_DATA_KEY_BYTES = 32;
const GCM_TAG_LENGTH = 16;
const GCM_IV_LENGTH = 12;

export const PASSWORD_KDF_ITERATIONS = 100000;

const SERVER_PUBLIC_KEY_PEM = `-----BEGIN PUBLIC KEY-----
MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAmRflvzAhSG4JxjC1B7He
JMcqfbtYKJ1QFO/HiODfgVFbRA05BI1rlPhaq8zc/OQITf28LFATTJ4T8GrlbL48
dy/Oa1dWtBgubtuTpFmk2vio64102TQsZgtH5NaN9IZ0YDp8FGDnZbeI/MkeM888
6rFC5lJ8mRu7iPdBR0pfZDd9pTo9SDiRm2WaCP0Lok+IrAjjEygPwllOY4aDTgnA
JBAXF/vqNV/BO8hbgB7zNSbmRuq/WbThRpdka8RMiIakudnMQMvIZGmXbI+7iuzb
JsLzZBQQZzw+NCfzhdptysuvOLUe6EFH2qnWg7DEwyNTM8nLktee0mEgnuscGU99
MwIDAQAB
-----END PUBLIC KEY-----`;

let activeDataKeyBase64: string | null = null;

const bytesToBinaryString = (bytes: Uint8Array): string => {
  let output = '';
  const chunkSize = 8192;

  for (let i = 0; i < bytes.length; i += chunkSize) {
    const chunk = bytes.subarray(i, i + chunkSize);
    output += String.fromCharCode(...Array.from(chunk));
  }

  return output;
};

const binaryStringToBytes = (value: string): Uint8Array => {
  const bytes = new Uint8Array(value.length);

  for (let i = 0; i < value.length; i += 1) {
    bytes[i] = value.charCodeAt(i);
  }

  return bytes;
};

const getWebCryptoRandomBytes = (byteCount: number): Uint8Array | null => {
  if (!globalThis.crypto?.getRandomValues) {
    return null;
  }

  const bytes = new Uint8Array(byteCount);
  globalThis.crypto.getRandomValues(bytes);
  return bytes;
};

export const randomBytes = (byteCount: number): Uint8Array => {
  return getWebCryptoRandomBytes(byteCount) ?? binaryStringToBytes(forge.random.getBytesSync(byteCount));
};

export const bytesToBase64 = (bytes: Uint8Array): string => {
  return forge.util.encode64(bytesToBinaryString(bytes));
};

export const base64ToBytes = (value: string): Uint8Array => {
  return binaryStringToBytes(forge.util.decode64(value));
};

const utf8ToBytes = (value: string): Uint8Array => {
  return binaryStringToBytes(forge.util.encodeUtf8(value));
};

const bytesToUtf8 = (bytes: Uint8Array): string => {
  return forge.util.decodeUtf8(bytesToBinaryString(bytes));
};

const utf8ToBinaryString = (value: string): string => {
  return bytesToBinaryString(utf8ToBytes(value));
};

export const randomBytesBase64 = (byteCount: number): string => {
  return bytesToBase64(randomBytes(byteCount));
};

export const randomUuid = (): string => {
  const bytes = randomBytes(16);
  bytes[6] = (bytes[6] & 0x0f) | 0x40;
  bytes[8] = (bytes[8] & 0x3f) | 0x80;

  const hex = Array.from(bytes, (byte) => byte.toString(16).padStart(2, '0'));
  return [
    hex.slice(0, 4).join(''),
    hex.slice(4, 6).join(''),
    hex.slice(6, 8).join(''),
    hex.slice(8, 10).join(''),
    hex.slice(10, 16).join(''),
  ].join('-');
};

export const generateDataKey = (): string => {
  return randomBytesBase64(ACTIVE_DATA_KEY_BYTES);
};

export const setActiveDataKey = (keyBase64: string): void => {
  if (base64ToBytes(keyBase64).length !== ACTIVE_DATA_KEY_BYTES) {
    throw new Error('Khóa dữ liệu phải là AES-256 key 32 byte.');
  }

  activeDataKeyBase64 = keyBase64;
};

export const clearActiveDataKey = (): void => {
  activeDataKeyBase64 = null;
};

export const getActiveDataKey = (): string | null => activeDataKeyBase64;

export const isDataKeyReady = (): boolean => Boolean(activeDataKeyBase64);

export const getKeyFingerprint = (keyMaterial: string): string => {
  const digest = forge.md.sha256.create();
  digest.update(keyMaterial, 'utf8');
  return digest.digest().toHex();
};

export const derivePasswordKey = (password: string, saltBase64: string): string => {
  const salt = forge.util.decode64(saltBase64);
  const derived = forge.pkcs5.pbkdf2(
    password,
    salt,
    PASSWORD_KDF_ITERATIONS,
    ACTIVE_DATA_KEY_BYTES,
    forge.md.sha256.create()
  );

  return forge.util.encode64(derived);
};

export const encryptStringAes256Gcm = async (
  plaintext: string,
  keyBase64: string,
  additionalData?: string
): Promise<string> => {
  const key = bytesToBinaryString(base64ToBytes(keyBase64));
  const iv = bytesToBinaryString(randomBytes(GCM_IV_LENGTH));
  const cipher = forge.cipher.createCipher('AES-GCM', key);

  cipher.start({
    iv,
    additionalData: additionalData ? utf8ToBinaryString(additionalData) : undefined,
    tagLength: GCM_TAG_LENGTH * 8,
  });
  cipher.update(forge.util.createBuffer(utf8ToBinaryString(plaintext)));

  if (!cipher.finish()) {
    throw new Error('Không thể mã hóa dữ liệu.');
  }

  const combined = iv + cipher.output.getBytes() + cipher.mode.tag.getBytes();
  return forge.util.encode64(combined);
};

export const decryptStringAes256Gcm = async (
  encryptedBase64: string,
  keyBase64: string,
  additionalData?: string
): Promise<string> => {
  const combined = forge.util.decode64(encryptedBase64);

  if (combined.length < GCM_IV_LENGTH + GCM_TAG_LENGTH) {
    throw new Error('Dữ liệu mã hóa không hợp lệ.');
  }

  const key = bytesToBinaryString(base64ToBytes(keyBase64));
  const iv = combined.slice(0, GCM_IV_LENGTH);
  const tag = combined.slice(combined.length - GCM_TAG_LENGTH);
  const ciphertext = combined.slice(GCM_IV_LENGTH, combined.length - GCM_TAG_LENGTH);
  const decipher = forge.cipher.createDecipher('AES-GCM', key);

  decipher.start({
    iv,
    additionalData: additionalData ? utf8ToBinaryString(additionalData) : undefined,
    tagLength: GCM_TAG_LENGTH * 8,
    tag: forge.util.createBuffer(tag),
  });
  decipher.update(forge.util.createBuffer(ciphertext));

  if (!decipher.finish()) {
    throw new Error('Không thể giải mã dữ liệu.');
  }

  return bytesToUtf8(binaryStringToBytes(decipher.output.getBytes()));
};

export const rsaEncryptForServer = (secretBase64: string): string => {
  const publicKey = forge.pki.publicKeyFromPem(SERVER_PUBLIC_KEY_PEM);
  const encrypted = publicKey.encrypt(forge.util.decode64(secretBase64), 'RSA-OAEP', {
    md: forge.md.sha256.create(),
    mgf1: {
      md: forge.md.sha256.create(),
    },
  });

  return forge.util.encode64(encrypted);
};

export const establishSecureChannel = () => {
  const sessionKey = randomBytesBase64(ACTIVE_DATA_KEY_BYTES);

  return {
    algorithm: 'RSA-OAEP-SHA256' as const,
    sessionKeyAlgorithm: 'AES-256' as const,
    serverKeyFingerprint: getKeyFingerprint(SERVER_PUBLIC_KEY_PEM),
    encryptedSessionKey: rsaEncryptForServer(sessionKey),
    establishedAt: new Date().toISOString(),
  };
};
