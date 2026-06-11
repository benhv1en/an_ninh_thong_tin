import AsyncStorage from '@react-native-async-storage/async-storage';
import { StateStorage } from 'zustand/middleware';
import {
  decryptStringAes256Gcm,
  encryptStringAes256Gcm,
  getActiveDataKey,
  getKeyFingerprint,
} from './securityService';

interface EncryptedStorageEnvelope {
  __cashtrackEncrypted: true;
  version: 1;
  algorithm: 'AES-256-GCM';
  keyFingerprint: string;
  payload: string;
  updatedAt: string;
}

const isEncryptedEnvelope = (value: unknown): value is EncryptedStorageEnvelope => {
  return (
    typeof value === 'object' &&
    value !== null &&
    (value as EncryptedStorageEnvelope).__cashtrackEncrypted === true &&
    (value as EncryptedStorageEnvelope).version === 1 &&
    (value as EncryptedStorageEnvelope).algorithm === 'AES-256-GCM' &&
    typeof (value as EncryptedStorageEnvelope).payload === 'string'
  );
};

const buildAdditionalData = (name: string): string => {
  return `cashtrack:encrypted-storage:${name}`;
};

export const encryptedTransactionStorage: StateStorage = {
  getItem: async (name) => {
    const stored = await AsyncStorage.getItem(name);

    if (!stored) {
      return null;
    }

    const activeKey = getActiveDataKey();

    if (!activeKey) {
      return null;
    }

    try {
      const parsed = JSON.parse(stored);

      if (isEncryptedEnvelope(parsed)) {
        return decryptStringAes256Gcm(parsed.payload, activeKey, buildAdditionalData(name));
      }
    } catch {
      // Plain JSON from older app versions is migrated below after unlock.
    }

    await encryptedTransactionStorage.setItem(name, stored);
    return stored;
  },

  setItem: async (name, value) => {
    const activeKey = getActiveDataKey();

    if (!activeKey) {
      return;
    }

    const envelope: EncryptedStorageEnvelope = {
      __cashtrackEncrypted: true,
      version: 1,
      algorithm: 'AES-256-GCM',
      keyFingerprint: getKeyFingerprint(activeKey),
      payload: await encryptStringAes256Gcm(value, activeKey, buildAdditionalData(name)),
      updatedAt: new Date().toISOString(),
    };

    await AsyncStorage.setItem(name, JSON.stringify(envelope));
  },

  removeItem: async (name) => {
    await AsyncStorage.removeItem(name);
  },
};
