import * as Crypto from 'expo-crypto';
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

export const randomBytesBase64 = (byteCount: number): string => {
  return bytesToBase64(Crypto.getRandomBytes(byteCount));
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
  const key = await Crypto.AESEncryptionKey.import(keyBase64, 'base64');
  const sealedData = await Crypto.aesEncryptAsync(utf8ToBytes(plaintext), key, {
    nonce: { length: GCM_IV_LENGTH },
    tagLength: GCM_TAG_LENGTH,
    additionalData: additionalData ? utf8ToBytes(additionalData) : undefined,
  });

  const combined = await sealedData.combined('base64');
  return combined as string;
};

export const decryptStringAes256Gcm = async (
  encryptedBase64: string,
  keyBase64: string,
  additionalData?: string
): Promise<string> => {
  const key = await Crypto.AESEncryptionKey.import(keyBase64, 'base64');
  const sealedData = Crypto.AESSealedData.fromCombined(encryptedBase64, {
    ivLength: GCM_IV_LENGTH,
    tagLength: GCM_TAG_LENGTH,
  });

  const decrypted = await Crypto.aesDecryptAsync(sealedData, key, {
    additionalData: additionalData ? utf8ToBytes(additionalData) : undefined,
  });

  return bytesToUtf8(decrypted as Uint8Array);
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
