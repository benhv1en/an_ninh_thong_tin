export interface StoredAuthAccount {
  id: string;
  fullName: string;
  email: string;
  passwordHash: string;
  passwordSalt: string;
  dataKeySalt: string;
  encryptedDataKey: string;
  createdAt: string;
  updatedAt: string;
  security: {
    passwordAlgorithm: 'bcrypt';
    bcryptRounds: number;
    dataKeyAlgorithm: 'AES-256-GCM';
    keyDerivation: 'PBKDF2-SHA256';
    keyDerivationIterations: number;
  };
}

export interface SecureChannelSession {
  algorithm: 'RSA-OAEP-SHA256';
  sessionKeyAlgorithm: 'AES-256';
  serverKeyFingerprint: string;
  encryptedSessionKey: string;
  establishedAt: string;
}

export interface AuthSession {
  userId: string;
  email: string;
  fullName: string;
  authenticatedAt: string;
  secureChannel: SecureChannelSession;
}

export interface RegisterInput {
  fullName: string;
  email: string;
  password: string;
}

export interface LoginInput {
  email: string;
  password: string;
}
