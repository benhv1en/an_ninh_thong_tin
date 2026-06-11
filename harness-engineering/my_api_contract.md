# REST API contract tối thiểu cho CashTrack

Ngày lập: 12/06/2026 01:44:08 +0700

## Nguyên tắc contract

- Frontend React Native/Expo/TypeScript strict mode là source of truth.
- API trả JSON, trừ endpoint export file.
- Date/time qua API dùng ISO 8601 UTC string, ví dụ `2026-06-11T18:44:08Z`.
- Frontend hiện tại dùng `amount: number` theo VND. Backend nên map sang `long AmountMinorUnits`; khi trả API vẫn trả `amount` để khớp frontend.
- Backend không trả plaintext secret, password, token, Gemini API key hoặc webhook secret.
- Các endpoint dưới đây là tối thiểu để app đồng bộ transaction, lấy metadata, parse notification, test/send webhook và import/export backup.

## Enum dùng chung

| Enum | Giá trị hợp lệ |
|---|---|
| `TransactionType` | `income`, `expense` |
| `TransactionSource` | `notification`, `manual`, `api` |
| `TransactionCategory` | `food`, `shopping`, `transport`, `entertainment`, `bills`, `health`, `education`, `salary`, `transfer`, `investment`, `gift`, `other` |
| `WebhookEvent` | `transaction.created`, `transaction.updated`, `transaction.deleted`, `budget.exceeded`, `budget.warning`, `daily.summary`, `weekly.summary`, `monthly.summary`, `notification.received` |
| `BackupFormat` | `json`, `xlsx` |

## DTO dùng chung

### TransactionDto

| Field | Type | Nullable | Ghi chú |
|---|---|---:|---|
| `id` | `string` | No | Id giao dịch. |
| `amount` | `number` | No | Số tiền VND theo frontend; backend lưu `long AmountMinorUnits`. |
| `type` | `TransactionType` | No | `income` hoặc `expense`. |
| `category` | `TransactionCategory` | No | Category id từ frontend. |
| `description` | `string` | No | Mô tả hiển thị. |
| `merchant` | `string` | Yes | Merchant/nguồn tiền nếu có. |
| `bankAccount` | `string` | Yes | Frontend đang dùng để gán bank code/account từ notification. |
| `source` | `TransactionSource` | No | Mặc định `manual` khi tạo thủ công. |
| `rawNotification` | `string` | Yes | Nội dung notification gốc, dùng để dedupe. |
| `createdAt` | `string` | No | ISO UTC. |
| `updatedAt` | `string` | No | ISO UTC. |

### CategoryDto

| Field | Type | Nullable | Ghi chú |
|---|---|---:|---|
| `id` | `TransactionCategory` | No | Category id. |
| `label` | `string` | No | Tên tiếng Anh. |
| `labelVi` | `string` | No | Tên tiếng Việt. |
| `icon` | `string` | No | Material icon name frontend đang dùng. |
| `color` | `string` | No | Hex color. |
| `gradient` | `string[]` | No | 2 màu gradient. |

### BankInfoDto

| Field | Type | Nullable | Ghi chú |
|---|---|---:|---|
| `code` | `string` | No | Ví dụ `VCB`, `MB`, `TCB`, `MOMO`. |
| `name` | `string` | No | Tên đầy đủ. |
| `shortName` | `string` | No | Tên ngắn. |
| `packageName` | `string` | No | Android package name dùng để detect app. |
| `color` | `string` | No | Hex color. |
| `logo` | `string` | Yes | Optional. |

### BankNotificationDto

| Field | Type | Nullable | Ghi chú |
|---|---|---:|---|
| `app` | `string` | No | Android package name, ví dụ `com.mbmobile`. |
| `title` | `string` | No | Notification title. |
| `text` | `string` | No | Notification body. |
| `time` | `number|string` | No | Epoch milliseconds theo frontend hiện tại hoặc ISO UTC. |
| `extra` | `object` | Yes | Metadata native nếu có. |
| `processed` | `boolean` | Yes | Trạng thái local, backend có thể bỏ qua. |

### ParsedTransactionDto

| Field | Type | Nullable | Ghi chú |
|---|---|---:|---|
| `amount` | `number` | No | Số tiền VND. |
| `type` | `TransactionType` | No | Kết quả parse. |
| `merchant` | `string` | Yes | Merchant parse được. |
| `description` | `string` | Yes | Mô tả rút gọn, tối đa 200 ký tự theo frontend. |
| `bankCode` | `string` | Yes | Bank code detect từ package name. |
| `accountNumber` | `string` | Yes | Nếu parser backend trích xuất được. |
| `time` | `string` | No | ISO UTC. |
| `rawText` | `string` | No | `title + text`. |

### WebhookConfigDto

| Field | Type | Nullable | Ghi chú |
|---|---|---:|---|
| `id` | `string` | No | Webhook id. |
| `name` | `string` | No | Tên hiển thị. |
| `url` | `string` | No | URL đích. |
| `enabled` | `boolean` | No | Có gửi webhook hay không. |
| `events` | `WebhookEvent[]` | No | Danh sách event. |
| `headers` | `Record<string,string>` | Yes | Custom headers. |
| `createdAt` | `string` | No | ISO UTC. |
| `lastTriggeredAt` | `string` | Yes | ISO UTC. |
| `failCount` | `number` | No | Số lần fail liên tiếp. |
| `hasSecret` | `boolean` | No | Thay cho plaintext `secret`. |

### WebhookDeliveryResultDto

| Field | Type | Nullable | Ghi chú |
|---|---|---:|---|
| `webhookId` | `string` | No | Id webhook đã gửi. |
| `success` | `boolean` | No | Kết quả response/exception. |
| `statusCode` | `number` | Yes | HTTP status của endpoint đích. |
| `error` | `string` | Yes | Lỗi network/HTTP nếu có. |
| `timestamp` | `string` | No | ISO UTC. |

## Bảng endpoint tối thiểu

| Endpoint | Method | Request DTO | Response DTO | Status code | Validation rule |
|---|---:|---|---|---|---|
| `/api/v1/transactions` | `GET` | Query: `type?`, `category?`, `startDate?`, `endDate?`, `minAmount?`, `maxAmount?`, `search?`, `limit?`, `cursor?` | `{ items: TransactionDto[], nextCursor?: string }` | `200`, `400`, `401` | Enum phải hợp lệ; date là ISO UTC; amount >= 0; `limit` trong khoảng 1-100; sort mới nhất trước theo `createdAt`. |
| `/api/v1/transactions/{id}` | `GET` | Path: `id` | `TransactionDto` | `200`, `401`, `404` | `id` bắt buộc, không rỗng; chỉ đọc giao dịch thuộc user hiện tại. |
| `/api/v1/transactions` | `POST` | `CreateTransactionRequest`: `amount`, `type`, `category`, `description?`, `merchant?`, `bankAccount?`, `source?`, `rawNotification?`, `externalId?`, `createdAt?` | `TransactionDto` | `201`, `400`, `401`, `409` | `amount > 0`; `type/category/source` hợp lệ; `description <= 200`; `merchant <= 100`; `source` mặc định `manual`; dedupe bằng `externalId` hoặc hash của `rawNotification`. |
| `/api/v1/transactions/{id}` | `PATCH` | `UpdateTransactionRequest`: partial của `amount`, `type`, `category`, `description`, `merchant`, `bankAccount` | `TransactionDto` | `200`, `400`, `401`, `404`, `409` | Có ít nhất 1 field; field nào gửi lên thì validate như create; cập nhật `updatedAt` server-side. |
| `/api/v1/transactions/{id}` | `DELETE` | Không body | Không body | `204`, `401`, `404` | Chỉ xóa giao dịch thuộc user hiện tại; nên soft delete ở DB để phục vụ sync sau này. |
| `/api/v1/categories` | `GET` | Không body | `CategoryDto[]` | `200` | Trả đúng danh sách category frontend đang có; không đổi id/icon/color nếu chưa update frontend. |
| `/api/v1/banks` | `GET` | Không body | `BankInfoDto[]` | `200` | Trả danh sách bank/app package dùng cho notification parser và settings selected banks. |
| `/api/v1/notifications/parse` | `POST` | `ParseNotificationRequest`: `notification: BankNotificationDto`, `selectedBankApps?: string[]`, `useAi?: boolean` | `ParseNotificationResponse`: `isBankingNotification`, `isAdvertisement`, `parsed?: ParsedTransactionDto`, `suggestedCategory?: TransactionCategory`, `duplicateKey?: string`, `reason?: string` | `200`, `400`, `401`, `422` | `app/title/text/time` bắt buộc; nếu `selectedBankApps` có giá trị thì `notification.app` phải nằm trong list; không tự tạo transaction trong endpoint parse; nếu không parse được trả `parsed = null` và `reason`. |
| `/api/v1/webhooks` | `GET` | Không body | `WebhookConfigDto[]` | `200`, `401` | Chỉ trả webhook của user hiện tại; không trả plaintext secret. |
| `/api/v1/webhooks` | `POST` | `CreateWebhookRequest`: `name`, `url`, `enabled?`, `secret?`, `events`, `headers?` | `WebhookConfigDto` | `201`, `400`, `401` | `name` bắt buộc; `url` phải là absolute `http`/`https`; `events.length > 0`; mọi event phải hợp lệ; header name/value không rỗng. |
| `/api/v1/webhooks/{id}` | `PATCH` | `UpdateWebhookRequest`: partial của `name`, `url`, `enabled`, `secret`, `events`, `headers` | `WebhookConfigDto` | `200`, `400`, `401`, `404` | Validate như create với field được gửi; nếu `secret = null` thì xóa secret; không trả secret sau update. |
| `/api/v1/webhooks/{id}` | `DELETE` | Không body | Không body | `204`, `401`, `404` | Chỉ xóa webhook thuộc user hiện tại. |
| `/api/v1/webhooks/{id}/test` | `POST` | `TestWebhookRequest?`: `payload?` | `WebhookDeliveryResultDto` | `200`, `400`, `401`, `404`, `502` | Webhook phải tồn tại; nếu không có `payload`, gửi event test `transaction.created` với `{ test: true, message: "This is a test webhook from CashTrack" }`; timeout nên giới hạn 10-15 giây. |
| `/api/v1/webhooks/send` | `POST` | `SendWebhookRequest`: `event`, `data`, `webhookIds?` | `WebhookDeliveryResultDto[]` | `200`, `400`, `401`, `502` | `event` phải hợp lệ; chỉ gửi webhook `enabled = true`, có subscribe event và `failCount < 5`; nếu có `webhookIds`, chỉ gửi các id thuộc user hiện tại. |
| `/api/v1/backup/export` | `GET` | Query: `format=json|xlsx` | File stream JSON/XLSX | `200`, `400`, `401` | `format` bắt buộc hoặc mặc định `json`; export transaction của user hiện tại; JSON nên là `TransactionDto[]`; XLSX dùng sheet `Transactions`. |
| `/api/v1/backup/import` | `POST` | `multipart/form-data`: `file`; hoặc JSON `{ transactions: TransactionDto[] }` | `ImportResultDto`: `success`, `total`, `imported`, `updated`, `skipped`, `errors[]` | `200`, `400`, `401`, `413`, `422` | Chỉ nhận `.json`, `.xlsx`, `.xls`; giới hạn dung lượng file; validate từng dòng; dedupe theo `id`, `externalId`, hoặc hash `rawNotification`; dòng lỗi không làm fail toàn bộ batch nếu có thể. |

## DTO request/response chính

### CreateTransactionRequest

```json
{
  "amount": 50000,
  "type": "expense",
  "category": "food",
  "description": "Mua hang",
  "merchant": "Circle K",
  "bankAccount": "MB",
  "source": "manual",
  "rawNotification": null,
  "externalId": null,
  "createdAt": "2026-06-11T18:44:08Z"
}
```

### UpdateTransactionRequest

```json
{
  "category": "shopping",
  "description": "Cap nhat mo ta",
  "merchant": "Shopee"
}
```

### ParseNotificationRequest

```json
{
  "notification": {
    "app": "com.mbmobile",
    "title": "MB Bank",
    "text": "GD: -50,000 VND tai Circle K luc 14:30. SD: 5,000,000 VND.",
    "time": 1781203448000,
    "extra": {}
  },
  "selectedBankApps": ["com.mbmobile"],
  "useAi": false
}
```

### ParseNotificationResponse

```json
{
  "isBankingNotification": true,
  "isAdvertisement": false,
  "parsed": {
    "amount": 50000,
    "type": "expense",
    "merchant": "Circle K",
    "description": "GD: -50,000 VND tai Circle K luc 14:30. SD: 5,000,000 VND.",
    "bankCode": "MB",
    "accountNumber": null,
    "time": "2026-06-11T18:44:08Z",
    "rawText": "MB Bank GD: -50,000 VND tai Circle K luc 14:30. SD: 5,000,000 VND."
  },
  "suggestedCategory": "food",
  "duplicateKey": "sha256:...",
  "reason": null
}
```

### CreateWebhookRequest

```json
{
  "name": "Discord",
  "url": "https://example.com/webhook",
  "enabled": true,
  "secret": "optional-secret",
  "events": ["transaction.created", "transaction.updated"],
  "headers": {
    "X-Custom-Header": "cashtrack"
  }
}
```

### SendWebhookRequest

```json
{
  "event": "transaction.created",
  "data": {
    "transaction": {
      "id": "abc",
      "amount": 50000,
      "type": "expense",
      "category": "food",
      "description": "Mua hang",
      "merchant": "Circle K",
      "createdAt": "2026-06-11T18:44:08Z"
    }
  },
  "webhookIds": ["webhook-1"]
}
```

### ImportResultDto

```json
{
  "success": true,
  "total": 10,
  "imported": 7,
  "updated": 2,
  "skipped": 1,
  "errors": [
    {
      "row": 5,
      "field": "amount",
      "message": "amount must be greater than 0"
    }
  ]
}
```

## Thứ tự ưu tiên implement

1. `GET/POST/PATCH/DELETE /api/v1/transactions`.
2. `GET /api/v1/categories` và `GET /api/v1/banks`.
3. `POST /api/v1/notifications/parse`.
4. `POST /api/v1/webhooks/{id}/test` và `POST /api/v1/webhooks/send`.
5. `GET /api/v1/backup/export` và `POST /api/v1/backup/import`.
6. Webhook CRUD nếu muốn đưa config webhook từ AsyncStorage lên backend.
