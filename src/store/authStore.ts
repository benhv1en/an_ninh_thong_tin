import { create } from 'zustand';
import { createJSONStorage, persist } from 'zustand/middleware';
import AsyncStorage from '@react-native-async-storage/async-storage';
import { authService } from '../services/authService';
import { AuthSession, LoginInput, RegisterInput, StoredAuthAccount } from '../types/auth';

interface AuthState {
  account: StoredAuthAccount | null;
  session: AuthSession | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  hasHydrated: boolean;
  error: string | null;

  register: (input: RegisterInput) => Promise<boolean>;
  login: (input: LoginInput) => Promise<boolean>;
  logout: () => Promise<void>;
  clearAuthError: () => void;
  setHasHydrated: (value: boolean) => void;
}

const hydrateTransactionsAfterUnlock = async () => {
  const { useTransactionStore } = await import('./transactionStore');
  await useTransactionStore.persist.rehydrate();
};

const lockTransactionsInMemory = async () => {
  const { useTransactionStore } = await import('./transactionStore');
  useTransactionStore.getState().lockInMemoryData();
};

const getErrorMessage = (error: unknown): string => {
  if (error instanceof Error) {
    return error.message;
  }

  return 'Không thể xử lý xác thực. Vui lòng thử lại.';
};

export const useAuthStore = create<AuthState>()(
  persist(
    (set, get) => ({
      account: null,
      session: null,
      isAuthenticated: false,
      isLoading: false,
      hasHydrated: false,
      error: null,

      register: async (input) => {
        if (get().account) {
          set({ error: 'Thiết bị này đã có tài khoản. Vui lòng đăng nhập.' });
          return false;
        }

        set({ isLoading: true, error: null });

        try {
          const { account, session } = await authService.register(input);
          set({
            account,
            session,
            isAuthenticated: true,
            isLoading: false,
            error: null,
          });
          await hydrateTransactionsAfterUnlock();
          return true;
        } catch (error) {
          set({
            isLoading: false,
            error: getErrorMessage(error),
          });
          return false;
        }
      },

      login: async (input) => {
        const account = get().account;

        if (!account) {
          set({ error: 'Chưa có tài khoản trên thiết bị này.' });
          return false;
        }

        set({ isLoading: true, error: null });

        try {
          const session = await authService.login(account, input);
          set({
            session,
            isAuthenticated: true,
            isLoading: false,
            error: null,
          });
          await hydrateTransactionsAfterUnlock();
          return true;
        } catch (error) {
          set({
            session: null,
            isAuthenticated: false,
            isLoading: false,
            error: getErrorMessage(error),
          });
          return false;
        }
      },

      logout: async () => {
        authService.logout();
        await lockTransactionsInMemory();
        set({
          session: null,
          isAuthenticated: false,
          isLoading: false,
          error: null,
        });
      },

      clearAuthError: () => set({ error: null }),
      setHasHydrated: (value) => set({ hasHydrated: value }),
    }),
    {
      name: 'cashtrack-auth',
      storage: createJSONStorage(() => AsyncStorage),
      partialize: (state) => ({
        account: state.account,
      }),
      onRehydrateStorage: () => (state) => {
        state?.setHasHydrated(true);
      },
    }
  )
);
