# Backend entity list de xuat

Nguon mapping: frontend React Native/Expo/TypeScript strict la source of truth. Cac field chinh duoc doc tu:

- `src/types/transaction.ts`
- `src/store/transactionStore.ts`
- `src/store/authStore.ts`
- `src/store/settingsStore.ts`
- `src/services/encryptedStorageService.ts`
- `src/types/notification.ts`
- `src/utils/notificationParser.ts`
- `src/services/backupService.ts`
- `src/services/webhookService.ts`
- `src/services/notificationService.ts`
- `src/services/nativeNotificationBridge.ts`

Backend mac dinh: C# ASP.NET Core, EF Core code-first, SQLite.

## Quy uoc chung

- Money: dung `decimal` trong C#, cau hinh SQLite precision logic o EF Core, khuyen nghi `decimal(18,2)` ve mat model.
- Time: dung `DateTimeOffset` de khong mat timezone khi sync mobile/backend.
- Enum/union frontend: dung C# enum va EF Core value converter luu text lowercase de match API contract.
- Id tu frontend hien la `string`, nen cac entity dong bo truc tiep voi mobile giu `string Id` thay vi ep sang `Guid`.
- Secret/API key/webhook secret: khong luu plaintext; cot de xuat la ban ma hoa (`...Encrypted`).

## Enums/value objects nen co

| Ten | Gia tri | Ly do |
| --- | --- | --- |
| `TransactionType` | `income`, `expense` | Map tu `Transaction.type`. |
| `TransactionSource` | `notification`, `manual`, `api` | Map tu `Transaction.source`. |
| `TransactionCategoryId` | `food`, `shopping`, `transport`, `entertainment`, `bills`, `health`, `education`, `salary`, `transfer`, `investment`, `gift`, `other` | Map tu `TransactionCategory`. |
| `BudgetPeriod` | `daily`, `weekly`, `monthly` | Map tu `Budget.period`. |
| `NotificationPermissionStatus` | `authorized`, `denied`, `unknown` | Map tu settings. |
| `FilterPeriod` | `day`, `week`, `month`, `year`, `custom` | Map tu `DateFilter.period`. |
| `WebhookEventName` | `transaction.created`, `transaction.updated`, `transaction.deleted`, `budget.exceeded`, `budget.warning`, `daily.summary`, `weekly.summary`, `monthly.summary`, `notification.received` | Map tu `WebhookEvent`. |
| `ImportFileType` | `json`, `xlsx`, `xls` | Map tu backup/restore. |
| `ImportRowStatus` | `imported`, `updated`, `skipped`, `failed` | Can cho audit import. |

## 1. UserAccount

Table: `Users`

| Column | C# type | Nullable | Constraint/index | Ly do mapping |
| --- | --- | --- | --- | --- |
| `Id` | `string` | No | PK, max 64 | Map tu `StoredAuthAccount.id`, frontend la string UUID. |
| `FullName` | `string` | No | max 160 | Map tu `fullName`. |
| `Email` | `string` | No | unique, max 320 | Map tu `email`, login theo email. |
| `PasswordHash` | `string` | No | max 256 | Map tu `passwordHash`. |
| `PasswordSalt` | `string` | No | max 128 | Map tu `passwordSalt`. |
| `DataKeySalt` | `string` | No | max 128 | Map tu `dataKeySalt`. |
| `EncryptedDataKey` | `string` | No | text | Map tu `encryptedDataKey`, khong luu data key plaintext. |
| `PasswordAlgorithm` | `string` | No | max 32 | Map tu `security.passwordAlgorithm`, default `bcrypt`. |
| `BcryptRounds` | `int` | No | index khong can | Map tu `security.bcryptRounds`. |
| `DataKeyAlgorithm` | `string` | No | max 32 | Map tu `security.dataKeyAlgorithm`, default `AES-256-GCM`. |
| `KeyDerivation` | `string` | No | max 32 | Map tu `security.keyDerivation`, default `PBKDF2-SHA256`. |
| `KeyDerivationIterations` | `int` | No | index khong can | Map tu `security.keyDerivationIterations`. |
| `CreatedAt` | `DateTimeOffset` | No | index optional | Map tu `createdAt`. |
| `UpdatedAt` | `DateTimeOffset` | No | index optional | Map tu `updatedAt`. |

Indexes/unique:

- `UX_Users_Email` unique on `Email`.

Relationships:

- `UserAccount` 1-1 `UserSettings`.
- `UserAccount` 1-n `Transactions`.
- `UserAccount` 1-n `WebhookEndpoints`.
- `UserAccount` 1-n `BankNotifications`.
- `UserAccount` 1-n `CategoryBudgets`.

## 2. UserSession

Table: `UserSessions`

| Column | C# type | Nullable | Constraint/index | Ly do mapping |
| --- | --- | --- | --- | --- |
| `Id` | `string` | No | PK, max 64 | Backend session id. |
| `UserId` | `string` | No | FK `Users.Id`, index | Map tu `AuthSession.userId`. |
| `Email` | `string` | No | max 320 | Map tu `AuthSession.email` de audit session. |
| `AuthenticatedAt` | `DateTimeOffset` | No | index | Map tu `authenticatedAt`. |
| `SecureChannelAlgorithm` | `string` | No | max 64 | Map tu `secureChannel.algorithm`. |
| `SessionKeyAlgorithm` | `string` | No | max 32 | Map tu `secureChannel.sessionKeyAlgorithm`. |
| `ServerKeyFingerprint` | `string` | No | max 128 | Map tu `secureChannel.serverKeyFingerprint`. |
| `EncryptedSessionKey` | `string` | No | text | Map tu `secureChannel.encryptedSessionKey`. |
| `EstablishedAt` | `DateTimeOffset` | No | index | Map tu `secureChannel.establishedAt`. |
| `RevokedAt` | `DateTimeOffset?` | Yes | index optional | Backend can thu hoi session. |

Indexes/unique:

- `IX_UserSessions_UserId_EstablishedAt`.
- `IX_UserSessions_RevokedAt`.

Relationships:

- n-1 `UserAccount`.

## 3. TransactionCategory

Table: `TransactionCategories`

| Column | C# type | Nullable | Constraint/index | Ly do mapping |
| --- | --- | --- | --- | --- |
| `Id` | `string` | No | PK, max 32 | Map truc tiep tu `TransactionCategory`. |
| `Label` | `string` | No | max 80 | Map tu `CategoryInfo.label`. |
| `LabelVi` | `string` | No | max 80 | Map tu `CategoryInfo.labelVi`. |
| `Icon` | `string` | No | max 80 | Map tu Material icon name. |
| `Color` | `string` | No | max 16 | Map tu hex color. |
| `GradientStart` | `string` | No | max 16 | Map tu `gradient[0]`. |
| `GradientEnd` | `string` | No | max 16 | Map tu `gradient[1]`. |
| `SortOrder` | `int` | No | index optional | Giu thu tu seed nhu frontend. |

Indexes/unique:

- PK `Id`.

Relationships:

- 1-n `Transactions`.
- 1-n `CategoryBudgets`.

Ghi chu:

- Bang nay nen seed 12 category frontend hien co.

## 4. BankApp

Table: `BankApps`

| Column | C# type | Nullable | Constraint/index | Ly do mapping |
| --- | --- | --- | --- | --- |
| `Code` | `string` | No | PK, max 16 | Map tu `BankCode`. |
| `Name` | `string` | No | max 200 | Map tu `BankInfo.name`. |
| `ShortName` | `string` | No | max 80 | Map tu `BankInfo.shortName`. |
| `PackageName` | `string` | No | unique, max 160 | Map tu `BankInfo.packageName` va settings `selectedBankApps`. |
| `Color` | `string` | No | max 16 | Map tu `BankInfo.color`. |
| `Logo` | `string?` | Yes | max 256 | Map tu optional `logo`. |

Indexes/unique:

- `UX_BankApps_PackageName` unique.

Relationships:

- 1-n `UserSelectedBankApps` qua `PackageName`.
- `Transactions.BankAccount` co the chua bank code hien tai, nen relation toi `BankApps.Code` la optional.

Ghi chu:

- Bang nay nen seed danh sach bank trong `src/types/notification.ts`.

## 5. UserSettings

Table: `UserSettings`

| Column | C# type | Nullable | Constraint/index | Ly do mapping |
| --- | --- | --- | --- | --- |
| `UserId` | `string` | No | PK, FK `Users.Id` | Settings la 1-1 theo user. |
| `Language` | `string` | No | max 8 | Map tu `language`, default `vi`. |
| `Currency` | `string` | No | max 8 | Map tu `currency`, default `VND`. |
| `NotificationEnabled` | `bool` | No | index optional | Map tu `notificationEnabled`. |
| `NotificationPermission` | `string` | No | max 16 | Map tu `notificationPermission`. |
| `MonthlyBudget` | `decimal` | No | none | Map tu `monthlyBudget`. |
| `IsCustomBudget` | `bool` | No | none | Map tu `isCustomBudget`. |
| `BudgetAlertThreshold` | `int` | No | none | Map tu `budgetAlertThreshold`. |
| `ShowCents` | `bool` | No | none | Map tu `showCents`. |
| `CompactNumbers` | `bool` | No | none | Map tu `compactNumbers`. |
| `GeminiApiKeyEncrypted` | `string?` | Yes | text | Map tu `geminiApiKey`, nhung backend phai ma hoa. |
| `UseAICategorization` | `bool` | No | none | Map tu `useAICategorizaton` typo frontend. |
| `UseAIReports` | `bool` | No | none | Map tu `useAIReports`. |
| `WebhooksEnabled` | `bool` | No | none | Map tu `webhooksEnabled`. |
| `CurrentFilterPeriod` | `string` | No | max 16 | Map tu `currentFilter.period`. |
| `FilterStartDate` | `DateTimeOffset?` | Yes | none | Map tu optional `currentFilter.startDate`. |
| `FilterEndDate` | `DateTimeOffset?` | Yes | none | Map tu optional `currentFilter.endDate`. |
| `SelectedMonth` | `int?` | Yes | none | Map tu optional `currentFilter.selectedMonth` 0-11. |
| `SelectedYear` | `int?` | Yes | none | Map tu optional `currentFilter.selectedYear`. |
| `UpdatedAt` | `DateTimeOffset` | No | index optional | Backend audit. |

Indexes/unique:

- PK `UserId`.

Relationships:

- 1-1 `UserAccount`.

## 6. UserSelectedBankApp

Table: `UserSelectedBankApps`

| Column | C# type | Nullable | Constraint/index | Ly do mapping |
| --- | --- | --- | --- | --- |
| `UserId` | `string` | No | composite PK, FK `Users.Id` | Owner settings. |
| `PackageName` | `string` | No | composite PK, FK alternate key `BankApps.PackageName` | Map tu `selectedBankApps: string[]`. |
| `CreatedAt` | `DateTimeOffset` | No | none | Audit. |

Indexes/unique:

- Composite PK/unique: `UserId`, `PackageName`.

Relationships:

- n-1 `UserAccount`.
- n-1 `BankApp`.

## 7. CategoryBudget

Table: `CategoryBudgets`

| Column | C# type | Nullable | Constraint/index | Ly do mapping |
| --- | --- | --- | --- | --- |
| `Id` | `string` | No | PK, max 64 | Backend/client id. |
| `UserId` | `string` | No | FK `Users.Id`, index | Owner. |
| `CategoryId` | `string` | No | FK `TransactionCategories.Id`, index | Map tu `Budget.category`. |
| `Limit` | `decimal` | No | none | Map tu `Budget.limit`. |
| `Period` | `string` | No | max 16 | Map tu `Budget.period`. |
| `CreatedAt` | `DateTimeOffset` | No | none | Audit. |
| `UpdatedAt` | `DateTimeOffset` | No | none | Audit. |

Indexes/unique:

- `UX_CategoryBudgets_UserId_CategoryId` unique. Frontend replace budget theo `category`, khong theo period.

Relationships:

- n-1 `UserAccount`.
- n-1 `TransactionCategory`.

## 8. Transaction

Table: `Transactions`

| Column | C# type | Nullable | Constraint/index | Ly do mapping |
| --- | --- | --- | --- | --- |
| `Id` | `string` | No | PK, max 64 | Map tu `Transaction.id`, frontend id la string. |
| `UserId` | `string` | No | FK `Users.Id`, index | Backend multi-user. |
| `Amount` | `decimal` | No | index optional | Map tu `amount: number`; dung decimal cho tien. |
| `Type` | `string` | No | max 16, index | Map tu `income`/`expense`. |
| `CategoryId` | `string` | No | FK `TransactionCategories.Id`, index | Map tu `category`. |
| `Description` | `string` | No | max 500 | Map tu `description`; backup xlsx cot `Description`. |
| `Merchant` | `string?` | Yes | max 200, index optional | Map tu optional `merchant`; search/filter co dung merchant. |
| `BankAccount` | `string?` | Yes | max 64, index optional | Map tu `bankAccount`; parser hien gan bank code vao field nay. |
| `Source` | `string` | No | max 16, index | Map tu `notification`/`manual`/`api`. |
| `RawNotification` | `string?` | Yes | text | Map tu optional `rawNotification`. |
| `RawNotificationHash` | `string?` | Yes | max 64, unique filtered | Backend duplicate check tu raw notification ma khong index text lon. |
| `CreatedAt` | `DateTimeOffset` | No | index | Map tu `createdAt`; xlsx cot `Created_At`. |
| `UpdatedAt` | `DateTimeOffset` | No | index optional | Map tu `updatedAt`; xlsx cot `Updated_At`. |
| `ImportBatchId` | `string?` | Yes | FK `ImportBatches.Id` | Trace giao dich duoc import tu JSON/XLSX. |
| `IsDeleted` | `bool` | No | index optional | Backend sync tombstone cho event delete. |
| `DeletedAt` | `DateTimeOffset?` | Yes | index optional | Thoi diem xoa neu can sync deletion. |

Indexes/unique:

- `IX_Transactions_UserId_CreatedAt` for list, recent, stats theo ngay/thang.
- `IX_Transactions_UserId_CategoryId_CreatedAt` for category breakdown.
- `IX_Transactions_UserId_Type_CreatedAt` for income/expense filter.
- `IX_Transactions_UserId_Source`.
- `UX_Transactions_UserId_RawNotificationHash` unique filtered `RawNotificationHash IS NOT NULL` to match duplicate prevention in notification flow.

Relationships:

- n-1 `UserAccount`.
- n-1 `TransactionCategory`.
- n-1 optional `ImportBatch`.
- 1-n `WebhookDeliveryLogs` optional through event payload transaction id.

## 9. BankNotification

Table: `BankNotifications`

| Column | C# type | Nullable | Constraint/index | Ly do mapping |
| --- | --- | --- | --- | --- |
| `Id` | `string` | No | PK, max 128 | Can nhan id native theo `app_time_amount` hoac server generate. |
| `UserId` | `string` | No | FK `Users.Id`, index | Owner. |
| `AppPackage` | `string` | No | max 160, index | Map tu `BankNotification.app`. |
| `BankCode` | `string?` | Yes | max 16, FK optional `BankApps.Code` | Parser detect bank tu package. |
| `Title` | `string` | No | max 300 | Map tu `title`. |
| `Text` | `string` | No | text | Map tu `text`. |
| `NotificationTime` | `DateTimeOffset` | No | index | Map tu `time: number`. |
| `ExtraJson` | `string?` | Yes | text | Map tu optional `extra`. |
| `Processed` | `bool` | No | index | Map tu optional `processed` trong pending file. |
| `IsBankingNotification` | `bool` | No | index optional | Ket qua `isBankingNotification`. |
| `IsAdvertisement` | `bool` | No | index optional | Ket qua `isAdvertisementNotification`/AI detection. |
| `RawTextHash` | `string` | No | max 64, unique per user | Chua hash cua `title + text`, phuc vu dedupe. |
| `ParsedAmount` | `decimal?` | Yes | none | Map tu `ParsedTransaction.amount` neu parse thanh cong. |
| `ParsedType` | `string?` | Yes | max 16 | Map tu `ParsedTransaction.type`. |
| `ParsedMerchant` | `string?` | Yes | max 200 | Map tu `ParsedTransaction.merchant`. |
| `ParsedDescription` | `string?` | Yes | max 500 | Map tu `ParsedTransaction.description`. |
| `ParsedTransactionId` | `string?` | Yes | FK `Transactions.Id`, unique optional | Link notification tao ra transaction nao. |
| `CreatedAt` | `DateTimeOffset` | No | index optional | Backend ingestion time. |

Indexes/unique:

- `UX_BankNotifications_UserId_RawTextHash` unique.
- `IX_BankNotifications_UserId_Processed`.
- `IX_BankNotifications_UserId_AppPackage_NotificationTime`.
- `IX_BankNotifications_ParsedTransactionId`.

Relationships:

- n-1 `UserAccount`.
- n-1 optional `BankApp`.
- 0/1-1 optional `Transaction`.

## 10. WebhookEndpoint

Table: `WebhookEndpoints`

| Column | C# type | Nullable | Constraint/index | Ly do mapping |
| --- | --- | --- | --- | --- |
| `Id` | `string` | No | PK, max 64 | Map tu `WebhookConfig.id`. |
| `UserId` | `string` | No | FK `Users.Id`, index | Owner. |
| `Name` | `string` | No | max 160 | Map tu `name`. |
| `Url` | `string` | No | max 2048 | Map tu `url`. |
| `Enabled` | `bool` | No | index | Map tu `enabled`. |
| `SecretEncrypted` | `string?` | Yes | text | Map tu optional `secret`, phai ma hoa. |
| `CreatedAt` | `DateTimeOffset` | No | index optional | Map tu `createdAt`. |
| `LastTriggeredAt` | `DateTimeOffset?` | Yes | index optional | Map tu `lastTriggeredAt`. |
| `FailCount` | `int` | No | index optional | Map tu `failCount`, backend skip neu >= 5. |

Indexes/unique:

- `IX_WebhookEndpoints_UserId_Enabled`.
- `IX_WebhookEndpoints_UserId_FailCount`.

Relationships:

- n-1 `UserAccount`.
- 1-n `WebhookEventSubscriptions`.
- 1-n `WebhookHeaders`.
- 1-n `WebhookDeliveryLogs`.

## 11. WebhookEventSubscription

Table: `WebhookEventSubscriptions`

| Column | C# type | Nullable | Constraint/index | Ly do mapping |
| --- | --- | --- | --- | --- |
| `Id` | `string` | No | PK, max 64 | Backend id. |
| `WebhookEndpointId` | `string` | No | FK `WebhookEndpoints.Id`, index | Parent webhook. |
| `Event` | `string` | No | max 64, index | Map tu `WebhookConfig.events[]`. |

Indexes/unique:

- `UX_WebhookEventSubscriptions_WebhookEndpointId_Event` unique.

Relationships:

- n-1 `WebhookEndpoint`.

## 12. WebhookHeader

Table: `WebhookHeaders`

| Column | C# type | Nullable | Constraint/index | Ly do mapping |
| --- | --- | --- | --- | --- |
| `Id` | `string` | No | PK, max 64 | Backend id. |
| `WebhookEndpointId` | `string` | No | FK `WebhookEndpoints.Id`, index | Parent webhook. |
| `Name` | `string` | No | max 128 | Map tu `WebhookConfig.headers` key. |
| `ValueEncrypted` | `string` | No | text | Map tu header value; ma hoa vi co the chua secret. |

Indexes/unique:

- `UX_WebhookHeaders_WebhookEndpointId_Name` unique.

Relationships:

- n-1 `WebhookEndpoint`.

## 13. WebhookDeliveryLog

Table: `WebhookDeliveryLogs`

| Column | C# type | Nullable | Constraint/index | Ly do mapping |
| --- | --- | --- | --- | --- |
| `Id` | `string` | No | PK, max 64 | Backend id. |
| `WebhookEndpointId` | `string` | No | FK `WebhookEndpoints.Id`, index | Map tu `WebhookDeliveryResult.webhookId`. |
| `Event` | `string` | No | max 64, index | Map tu `WebhookPayload.event`. |
| `TransactionId` | `string?` | Yes | FK optional `Transactions.Id`, index | Link nhanh cho transaction events. |
| `PayloadJson` | `string` | No | text | Map tu `WebhookPayload.data`. |
| `Success` | `bool` | No | index | Map tu `success`. |
| `StatusCode` | `int?` | Yes | index optional | Map tu optional `statusCode`. |
| `Error` | `string?` | Yes | max 1000 | Map tu optional `error`. |
| `Timestamp` | `DateTimeOffset` | No | index | Map tu `timestamp`. |

Indexes/unique:

- `IX_WebhookDeliveryLogs_WebhookEndpointId_Timestamp`.
- `IX_WebhookDeliveryLogs_Success_Timestamp`.
- `IX_WebhookDeliveryLogs_TransactionId`.

Relationships:

- n-1 `WebhookEndpoint`.
- n-1 optional `Transaction`.

## 14. ImportBatch

Table: `ImportBatches`

| Column | C# type | Nullable | Constraint/index | Ly do mapping |
| --- | --- | --- | --- | --- |
| `Id` | `string` | No | PK, max 64 | Backend import id. |
| `UserId` | `string` | No | FK `Users.Id`, index | Owner. |
| `FileName` | `string` | No | max 255 | Map tu picked file name. |
| `FileType` | `string` | No | max 16 | Map tu `json`, `xlsx`, `xls`. |
| `StartedAt` | `DateTimeOffset` | No | index | Import start time. |
| `CompletedAt` | `DateTimeOffset?` | Yes | index optional | Import complete time. |
| `Status` | `string` | No | max 32 | `running`, `completed`, `failed`, etc. |
| `TotalRows` | `int` | No | none | So dong doc tu JSON/XLSX. |
| `ImportedCount` | `int` | No | none | Tuong ung add moi. |
| `UpdatedCount` | `int` | No | none | Tuong ung update existing id. |
| `SkippedCount` | `int` | No | none | Dong bi bo qua/duplicate. |
| `ErrorMessage` | `string?` | Yes | max 2000 | Loi import neu co. |

Indexes/unique:

- `IX_ImportBatches_UserId_StartedAt`.
- `IX_ImportBatches_Status`.

Relationships:

- n-1 `UserAccount`.
- 1-n `ImportRows`.
- 1-n `Transactions` optional qua `ImportBatchId`.

## 15. ImportRow

Table: `ImportRows`

| Column | C# type | Nullable | Constraint/index | Ly do mapping |
| --- | --- | --- | --- | --- |
| `Id` | `string` | No | PK, max 64 | Backend row id. |
| `ImportBatchId` | `string` | No | FK `ImportBatches.Id`, index | Parent import. |
| `RowNumber` | `int` | No | unique per batch | XLSX row / JSON item index. |
| `ExternalTransactionId` | `string?` | Yes | max 64, index optional | Map tu xlsx cot `ID` hoac JSON `id`. |
| `RawJson` | `string` | No | text | Snapshot dong import goc. |
| `Status` | `string` | No | max 32 | `imported`, `updated`, `skipped`, `failed`. |
| `ErrorMessage` | `string?` | Yes | max 2000 | Loi parse/validation. |
| `TransactionId` | `string?` | Yes | FK optional `Transactions.Id`, index | Link den transaction tao/cap nhat. |

Indexes/unique:

- `UX_ImportRows_ImportBatchId_RowNumber` unique.
- `IX_ImportRows_ExternalTransactionId`.
- `IX_ImportRows_TransactionId`.

Relationships:

- n-1 `ImportBatch`.
- n-1 optional `Transaction`.

## 16. ClientStorageEnvelope

Table: `ClientStorageEnvelopes`

| Column | C# type | Nullable | Constraint/index | Ly do mapping |
| --- | --- | --- | --- | --- |
| `Id` | `string` | No | PK, max 64 | Backend id. |
| `UserId` | `string` | No | FK `Users.Id`, index | Owner. |
| `StorageKey` | `string` | No | max 128 | Map tu AsyncStorage keys: `cashtrack-transactions`, `cashtrack-auth`, `cashtrack-settings`, `cashtrack-webhooks`. |
| `Version` | `int` | No | none | Map tu envelope `version`. |
| `Algorithm` | `string` | No | max 32 | Map tu `AES-256-GCM`. |
| `KeyFingerprint` | `string?` | Yes | max 128 | Map tu `keyFingerprint`. |
| `Payload` | `string` | No | text | Map tu encrypted `payload`; khong giai ma o backend neu chi sync blob. |
| `UpdatedAt` | `DateTimeOffset` | No | index | Map tu envelope `updatedAt`. |

Indexes/unique:

- `UX_ClientStorageEnvelopes_UserId_StorageKey` unique.

Relationships:

- n-1 `UserAccount`.

Ghi chu:

- Bang nay chi nen dung neu backend can sync/migrate blob AsyncStorage cu. Luong nghiep vu chinh nen dung cac bang normalized o tren.

## DTO/computed khong nen tao table rieng

| Type frontend | Ly do khong persist thanh entity rieng |
| --- | --- |
| `TransactionStats` | Co the query aggregate tu `Transactions`. |
| `getTodayStats`, `getWeekStats`, `getMonthStats` result | Computed theo date range. |
| `getDailyTotals` result | Aggregate theo ngay tu `Transactions`. |
| `getCategoryTotals` result | Aggregate theo `CategoryId`. |
| `ParsedTransaction` | Ket qua parse tam thoi; neu can audit thi da map vao `BankNotifications` + `Transactions`. |
| `WebhookPayload` | Payload delivery runtime; neu can audit thi luu `PayloadJson` trong `WebhookDeliveryLogs`. |
| `WebhookDeliveryResult` | Da map vao `WebhookDeliveryLogs`. |
| `DateFilter` | User preference nen nam trong `UserSettings`, khong can table rieng. |

## Tom tat quan he chinh

- `UserAccount` 1-1 `UserSettings`.
- `UserAccount` 1-n `Transactions`.
- `TransactionCategory` 1-n `Transactions`.
- `UserAccount` n-n `BankApps` qua `UserSelectedBankApps`.
- `UserAccount` 1-n `CategoryBudgets`, moi category toi da 1 budget theo frontend hien tai.
- `UserAccount` 1-n `BankNotifications`.
- `BankNotification` 0/1-1 `Transaction` khi notification tao duoc transaction.
- `UserAccount` 1-n `WebhookEndpoints`.
- `WebhookEndpoint` 1-n `WebhookEventSubscriptions`, `WebhookHeaders`, `WebhookDeliveryLogs`.
- `ImportBatch` 1-n `ImportRows`, va co the 1-n `Transactions` duoc import.

