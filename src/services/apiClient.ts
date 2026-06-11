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

export type ApiRequestStatus = 'idle' | 'loading' | 'success' | 'error';

export interface ApiRequestState<T> {
  status: ApiRequestStatus;
  loading: boolean;
  data: T | null;
  error: ApiClientError | null;
}

export interface ApiCallOptions<T> {
  signal?: AbortSignal;
  onStateChange?: (state: ApiRequestState<T>) => void;
}

const normalizeBaseUrl = (baseUrl: string): string => baseUrl.replace(/\/+$/, '');

const createState = <T>(
  status: ApiRequestStatus,
  data: T | null,
  error: ApiClientError | null
): ApiRequestState<T> => ({
  status,
  loading: status === 'loading',
  data,
  error,
});

const toApiClientError = (error: unknown): ApiClientError => {
  if (error instanceof ApiClientError) {
    return error;
  }

  if (error instanceof Error) {
    return new ApiClientError(error.message, 0);
  }

  return new ApiClientError('Network request failed', 0);
};

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

  health(options?: ApiCallOptions<HealthResponse>): Promise<HealthResponse> {
    return this.request<HealthResponse>('/health', {}, options);
  }

  listTransactions(
    query?: TransactionQuery,
    options?: ApiCallOptions<TransactionListResponse>
  ): Promise<TransactionListResponse> {
    return this.request<TransactionListResponse>('/api/v1/transactions', { query }, options);
  }

  getTransaction(id: string, options?: ApiCallOptions<TransactionDto>): Promise<TransactionDto> {
    return this.request<TransactionDto>(
      `/api/v1/transactions/${encodeURIComponent(id)}`,
      {},
      options
    );
  }

  createTransaction(
    request: CreateTransactionRequest,
    options?: ApiCallOptions<TransactionDto>
  ): Promise<TransactionDto> {
    return this.request<TransactionDto>(
      '/api/v1/transactions',
      {
        method: 'POST',
        body: request,
      },
      options
    );
  }

  updateTransaction(
    id: string,
    request: UpdateTransactionRequest,
    options?: ApiCallOptions<TransactionDto>
  ): Promise<TransactionDto> {
    return this.request<TransactionDto>(
      `/api/v1/transactions/${encodeURIComponent(id)}`,
      {
        method: 'PATCH',
        body: request,
      },
      options
    );
  }

  async deleteTransaction(id: string, options?: ApiCallOptions<void>): Promise<void> {
    await this.request<void>(
      `/api/v1/transactions/${encodeURIComponent(id)}`,
      {
        method: 'DELETE',
      },
      options
    );
  }

  listCategories(options?: ApiCallOptions<CategoryDto[]>): Promise<CategoryDto[]> {
    return this.request<CategoryDto[]>('/api/v1/categories', {}, options);
  }

  listBanks(options?: ApiCallOptions<BankInfoDto[]>): Promise<BankInfoDto[]> {
    return this.request<BankInfoDto[]>('/api/v1/banks', {}, options);
  }

  parseNotification(
    request: ParseNotificationRequest,
    options?: ApiCallOptions<ParseNotificationResponse>
  ): Promise<ParseNotificationResponse> {
    return this.request<ParseNotificationResponse>(
      '/api/v1/notifications/parse',
      {
        method: 'POST',
        body: request,
      },
      options
    );
  }

  private async request<T>(
    path: string,
    options: {
      method?: 'GET' | 'POST' | 'PATCH' | 'DELETE';
      query?: object;
      body?: unknown;
    } = {},
    callOptions?: ApiCallOptions<T>
  ): Promise<T> {
    const method = options.method ?? 'GET';
    const url = `${this.baseUrl ?? resolveApiBaseUrl()}${path}${buildQueryString(options.query)}`;
    const hasBody = options.body !== undefined;

    callOptions?.onStateChange?.(createState<T>('loading', null, null));

    try {
      const response = await fetch(url, {
        method,
        headers: {
          Accept: 'application/json',
          ...(hasBody ? { 'Content-Type': 'application/json' } : {}),
        },
        body: hasBody ? JSON.stringify(normalizeRequestBody(options.body)) : undefined,
        signal: callOptions?.signal,
      });

      if (response.status === 204) {
        const emptyResult = undefined as T;
        callOptions?.onStateChange?.(createState<T>('success', emptyResult, null));
        return emptyResult;
      }

      const contentType = response.headers.get('content-type') ?? '';
      const payload = contentType.includes('application/json') ? await response.json() : undefined;

      if (!response.ok) {
        const problem = payload as ApiProblemDetails | undefined;
        const message = problem?.detail || problem?.title || `Request failed with status ${response.status}`;
        throw new ApiClientError(message, response.status, problem);
      }

      const result = payload as T;
      callOptions?.onStateChange?.(createState<T>('success', result, null));
      return result;
    } catch (error) {
      const apiError = toApiClientError(error);
      callOptions?.onStateChange?.(createState<T>('error', null, apiError));
      throw apiError;
    }
  }
}

export const createApiClient = (baseUrl?: string): CashTrackApiClient => new CashTrackApiClient(baseUrl);

export const apiClient = createApiClient();
