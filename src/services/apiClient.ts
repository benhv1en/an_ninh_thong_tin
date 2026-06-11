import Constants from 'expo-constants';
import { Platform } from 'react-native';
import type {
  ApiProblemDetails,
  BankInfoDto,
  CategoryDto,
  CreateTransactionRequest,
  HealthResponse,
  ParseNotificationRequest,
  ParseNotificationResponse,
  TransactionDto,
  TransactionListResponse,
  TransactionQuery,
  UpdateTransactionRequest,
} from '../types/api';

declare const process:
  | {
      env?: {
        EXPO_PUBLIC_API_BASE_URL?: string;
      };
    }
  | undefined;

const API_PORT = '5055';
const API_BASE_URL_ENV = 'EXPO_PUBLIC_API_BASE_URL';

export class ApiClientError extends Error {
  readonly status: number;
  readonly problem?: ApiProblemDetails;

  constructor(message: string, status: number, problem?: ApiProblemDetails) {
    super(message);
    this.name = 'ApiClientError';
    this.status = status;
    this.problem = problem;
  }
}

const normalizeBaseUrl = (baseUrl: string): string => baseUrl.replace(/\/+$/, '');

export const resolveApiBaseUrl = (): string => {
  const envBaseUrl = process?.env?.EXPO_PUBLIC_API_BASE_URL?.trim();
  if (envBaseUrl) {
    return normalizeBaseUrl(envBaseUrl);
  }

  if (Constants.isDevice) {
    throw new Error(
      `Set ${API_BASE_URL_ENV}=http://<LAN-IP>:${API_PORT} when running on a physical device.`
    );
  }

  if (Platform.OS === 'android') {
    return `http://10.0.2.2:${API_PORT}`;
  }

  return `http://localhost:${API_PORT}`;
};

const toQueryValue = (value: unknown): string | undefined => {
  if (value === undefined || value === null || value === '') {
    return undefined;
  }

  if (value instanceof Date) {
    return value.toISOString();
  }

  if (value === 'all') {
    return undefined;
  }

  return String(value);
};

const buildQueryString = (query?: object): string => {
  if (!query) {
    return '';
  }

  const params = Object.entries(query)
    .map(([key, value]) => {
      const normalized = toQueryValue(value);
      return normalized ? `${encodeURIComponent(key)}=${encodeURIComponent(normalized)}` : undefined;
    })
    .filter((item): item is string => Boolean(item));

  return params.length > 0 ? `?${params.join('&')}` : '';
};

const normalizeRequestBody = (body: unknown): unknown => {
  if (!body || typeof body !== 'object') {
    return body;
  }

  return Object.fromEntries(
    Object.entries(body as Record<string, unknown>).map(([key, value]) => [
      key,
      value instanceof Date ? value.toISOString() : value,
    ])
  );
};

class CashTrackApiClient {
  private readonly baseUrl?: string;

  constructor(baseUrl?: string) {
    this.baseUrl = baseUrl ? normalizeBaseUrl(baseUrl) : undefined;
  }

  health(): Promise<HealthResponse> {
    return this.request<HealthResponse>('/health');
  }

  listTransactions(query?: TransactionQuery): Promise<TransactionListResponse> {
    return this.request<TransactionListResponse>('/api/v1/transactions', { query });
  }

  getTransaction(id: string): Promise<TransactionDto> {
    return this.request<TransactionDto>(`/api/v1/transactions/${encodeURIComponent(id)}`);
  }

  createTransaction(request: CreateTransactionRequest): Promise<TransactionDto> {
    return this.request<TransactionDto>('/api/v1/transactions', {
      method: 'POST',
      body: request,
    });
  }

  updateTransaction(id: string, request: UpdateTransactionRequest): Promise<TransactionDto> {
    return this.request<TransactionDto>(`/api/v1/transactions/${encodeURIComponent(id)}`, {
      method: 'PATCH',
      body: request,
    });
  }

  async deleteTransaction(id: string): Promise<void> {
    await this.request<void>(`/api/v1/transactions/${encodeURIComponent(id)}`, {
      method: 'DELETE',
    });
  }

  listCategories(): Promise<CategoryDto[]> {
    return this.request<CategoryDto[]>('/api/v1/categories');
  }

  listBanks(): Promise<BankInfoDto[]> {
    return this.request<BankInfoDto[]>('/api/v1/banks');
  }

  parseNotification(request: ParseNotificationRequest): Promise<ParseNotificationResponse> {
    return this.request<ParseNotificationResponse>('/api/v1/notifications/parse', {
      method: 'POST',
      body: request,
    });
  }

  private async request<T>(
    path: string,
    options: {
      method?: 'GET' | 'POST' | 'PATCH' | 'DELETE';
      query?: object;
      body?: unknown;
    } = {}
  ): Promise<T> {
    const method = options.method ?? 'GET';
    const url = `${this.baseUrl ?? resolveApiBaseUrl()}${path}${buildQueryString(options.query)}`;
    const hasBody = options.body !== undefined;

    const response = await fetch(url, {
      method,
      headers: {
        Accept: 'application/json',
        ...(hasBody ? { 'Content-Type': 'application/json' } : {}),
      },
      body: hasBody ? JSON.stringify(normalizeRequestBody(options.body)) : undefined,
    });

    if (response.status === 204) {
      return undefined as T;
    }

    const contentType = response.headers.get('content-type') ?? '';
    const payload = contentType.includes('application/json') ? await response.json() : undefined;

    if (!response.ok) {
      const problem = payload as ApiProblemDetails | undefined;
      const message = problem?.detail || problem?.title || `Request failed with status ${response.status}`;
      throw new ApiClientError(message, response.status, problem);
    }

    return payload as T;
  }
}

export const createApiClient = (baseUrl?: string): CashTrackApiClient => new CashTrackApiClient(baseUrl);

export const apiClient = createApiClient();
