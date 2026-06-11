import type { TransactionCategory } from './transaction';

export type IsoDateString = string;

export type ApiTransactionType = 'income' | 'expense';
export type ApiTransactionSource = 'notification' | 'manual' | 'api';

export interface HealthResponse {
  status: string;
}

export interface ApiProblemDetails {
  type?: string;
  title?: string;
  status?: number;
  detail?: string;
  instance?: string;
  errors?: Record<string, string[]>;
  [key: string]: unknown;
}

export interface TransactionDto {
  id: string;
  amount: number;
  type: ApiTransactionType;
  category: TransactionCategory;
  description: string;
  merchant?: string | null;
  bankAccount?: string | null;
  source: ApiTransactionSource;
  rawNotification?: string | null;
  createdAt: IsoDateString;
  updatedAt: IsoDateString;
}

export interface TransactionListResponse {
  items: TransactionDto[];
  nextCursor?: string | null;
}

export interface TransactionQuery {
  type?: ApiTransactionType | 'all';
  category?: TransactionCategory;
  startDate?: Date | IsoDateString;
  endDate?: Date | IsoDateString;
  minAmount?: number;
  maxAmount?: number;
  search?: string;
  limit?: number;
  cursor?: string;
}

export interface CreateTransactionRequest {
  amount: number;
  type: ApiTransactionType;
  category: TransactionCategory;
  description?: string;
  merchant?: string;
  bankAccount?: string;
  source?: ApiTransactionSource;
  rawNotification?: string;
  externalId?: string;
  createdAt?: Date | IsoDateString;
}

export interface UpdateTransactionRequest {
  amount?: number;
  type?: ApiTransactionType;
  category?: TransactionCategory;
  description?: string;
  merchant?: string | null;
  bankAccount?: string | null;
}

export interface CategoryDto {
  id: TransactionCategory;
  label: string;
  labelVi: string;
  icon: string;
  color: string;
  gradient: string[];
}

export interface BankInfoDto {
  code: string;
  name: string;
  shortName: string;
  packageName: string;
  color: string;
  logo?: string | null;
}

export interface BankNotificationDto {
  app: string;
  title: string;
  text: string;
  time: number | IsoDateString;
  extra?: Record<string, unknown>;
  processed?: boolean;
}

export interface ParsedTransactionDto {
  amount: number;
  type: ApiTransactionType;
  merchant?: string | null;
  description?: string | null;
  bankCode?: string | null;
  accountNumber?: string | null;
  time: IsoDateString;
  rawText: string;
}

export interface ParseNotificationRequest {
  notification: BankNotificationDto;
  selectedBankApps?: string[];
  useAi?: boolean;
}

export interface ParseNotificationResponse {
  isBankingNotification: boolean;
  isAdvertisement: boolean;
  parsed?: ParsedTransactionDto | null;
  suggestedCategory?: TransactionCategory | null;
  duplicateKey?: string | null;
  reason?: string | null;
}

export type WebhookEvent =
  | 'transaction.created'
  | 'transaction.updated'
  | 'transaction.deleted'
  | 'budget.exceeded'
  | 'budget.warning'
  | 'daily.summary'
  | 'weekly.summary'
  | 'monthly.summary'
  | 'notification.received';

export interface WebhookConfigDto {
  id: string;
  name: string;
  url: string;
  enabled: boolean;
  events: WebhookEvent[];
  headers?: Record<string, string> | null;
  createdAt: IsoDateString;
  lastTriggeredAt?: IsoDateString | null;
  failCount: number;
  hasSecret: boolean;
}

export interface WebhookDeliveryResultDto {
  webhookId: string;
  success: boolean;
  statusCode?: number | null;
  error?: string | null;
  timestamp: IsoDateString;
}

export interface ImportResultDto {
  success: boolean;
  total: number;
  imported: number;
  updated: number;
  skipped: number;
  errors: Array<{
    row: number;
    field: string;
    message: string;
  }>;
}
