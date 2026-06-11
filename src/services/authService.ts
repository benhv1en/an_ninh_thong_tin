import * as bcrypt from 'bcryptjs';
import * as Crypto from 'expo-crypto';
import {
  clearActiveDataKey,
  decryptStringAes256Gcm,
  derivePasswordKey,
  encryptStringAes256Gcm,
  establishSecureChannel,
  generateDataKey,
  randomBytesBase64,
  setActiveDataKey,
  PASSWORD_KDF_ITERATIONS,
} from './securityService';
import { AuthSession, LoginInput, RegisterInput, StoredAuthAccount } from '../types/auth';

const BCRYPT_ROUNDS = 12;

let bcryptRandomFallbackConfigured = false;

const configureBcryptRandomFallback = () => {
  if (bcryptRandomFallbackConfigured) {
    return;
  }

  bcrypt.setRandomFallback((length) => Array.from(Crypto.getRandomBytes(length)));
  bcryptRandomFallbackConfigured = true;
};

const normalizeEmail = (email: string): string => email.trim().toLowerCase();

const buildDataKeyAad = (email: string): string => `cashtrack:data-key:${email}`;

const createSession = (account: StoredAuthAccount): AuthSession => ({
  userId: account.id,
  email: account.email,
  fullName: account.fullName,
  authenticatedAt: new Date().toISOString(),
  secureChannel: establishSecureChannel(),
});

export const authService = {
  register: async ({ fullName, email, password }: RegisterInput) => {
    configureBcryptRandomFallback();

    const normalizedEmail = normalizeEmail(email);
    const passwordSalt = await bcrypt.genSalt(BCRYPT_ROUNDS);
    const passwordHash = await bcrypt.hash(password, passwordSalt);
    const dataKey = generateDataKey();
    const dataKeySalt = randomBytesBase64(16);
    const wrappingKey = derivePasswordKey(password, dataKeySalt);
    const encryptedDataKey = await encryptStringAes256Gcm(
      dataKey,
      wrappingKey,
      buildDataKeyAad(normalizedEmail)
    );
    const now = new Date().toISOString();

    const account: StoredAuthAccount = {
      id: Crypto.randomUUID(),
      fullName: fullName.trim(),
      email: normalizedEmail,
      passwordHash,
      passwordSalt: bcrypt.getSalt(passwordHash),
      dataKeySalt,
      encryptedDataKey,
      createdAt: now,
      updatedAt: now,
      security: {
        passwordAlgorithm: 'bcrypt',
        bcryptRounds: BCRYPT_ROUNDS,
        dataKeyAlgorithm: 'AES-256-GCM',
        keyDerivation: 'PBKDF2-SHA256',
        keyDerivationIterations: PASSWORD_KDF_ITERATIONS,
      },
    };

    setActiveDataKey(dataKey);

    return {
      account,
      session: createSession(account),
    };
  },

  login: async (account: StoredAuthAccount, { email, password }: LoginInput) => {
    configureBcryptRandomFallback();

    if (normalizeEmail(email) !== account.email) {
      throw new Error('Email hoặc mật khẩu không đúng.');
    }

    const isValid = await bcrypt.compare(password, account.passwordHash);

    if (!isValid) {
      throw new Error('Email hoặc mật khẩu không đúng.');
    }

    const wrappingKey = derivePasswordKey(password, account.dataKeySalt);
    const dataKey = await decryptStringAes256Gcm(
      account.encryptedDataKey,
      wrappingKey,
      buildDataKeyAad(account.email)
    );

    setActiveDataKey(dataKey);

    return createSession(account);
  },

  logout: () => {
    clearActiveDataKey();
  },
};
