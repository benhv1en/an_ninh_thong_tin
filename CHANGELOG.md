# 📋 Changelog

## 12/06/2026 08:28:29 +0700

### Thay đổi
- Thêm solution `CashTrack.slnx` để chạy `dotnet test` từ root repository.
- Thêm test project `backend/CashTrack.Api.Tests` dùng xUnit, `WebApplicationFactory<Program>` và SQLite in-memory cho API smoke tests.
- Thêm test tối thiểu cho `/health`, `GET /api/v1/transactions`, `POST /api/v1/transactions` và validation fail case với body `{}`.
- Cập nhật `backend/CashTrack.Api/Program.cs` với `public partial class Program;` để test host truy cập được entrypoint minimal API.
- Thêm REST Client examples tại `backend/CashTrack.Api/CashTrack.Api.http` cho health, list transaction, create transaction và validation fail case.
- Cập nhật `build.sh` để chạy `dotnet test` và ghi rõ cách dùng `.http` thủ công.

### Lý do
- Có kiểm thử backend API tối thiểu chạy qua HTTP pipeline thật, tách database test khỏi SQLite dev bằng SQLite in-memory.
- Có file `.http` để developer kiểm tra thủ công cùng contract frontend-backend hiện tại.

### Kiểm tra
- `dotnet test`: pass, 4/4 tests.
- Chạy backend thật bằng `dotnet run --project backend/CashTrack.Api/CashTrack.Api.csproj --no-build --urls http://127.0.0.1:5055`: pass.
- Request tương đương `.http` bằng `curl`: `/health` trả `200`, GET transactions trả `200`, POST transaction trả `201`, validation body `{}` trả `400 application/problem+json`.

## 12/06/2026 08:16:07 +0700

### Thay đổi
- Chẩn đoán lại lỗi nối frontend-backend theo thứ tự yêu cầu: backend runtime, URL base, emulator/device network, CORS, HTTP/HTTPS, route/method, status code, request JSON, response JSON casing, validation và SQLite/migration.
- Xác định root cause trực tiếp: backend không chạy trên `http://127.0.0.1:5055` tại thời điểm kiểm tra đầu tiên.
- Khởi động backend để xác nhận API contract hiện khớp khi backend chạy đúng.
- Không sửa source runtime frontend/backend vì route, method, JSON casing, validation và SQLite migration đều ổn.
- Cập nhật `harness-engineering/project_definition.md` và `build.sh` theo quy ước sau prompt.

### Lý do
- Ghi lại kết quả chẩn đoán mới nhất và cách chạy đúng cho local/emulator/device.
- Làm rõ rủi ro còn lại nằm ở vận hành: backend chưa chạy, CORS LAN cho Expo/web browser, hoặc HTTP cleartext khi build Android release.

### Kiểm tra
- `curl -i --max-time 3 http://127.0.0.1:5055/health`: fail trước khi chạy backend với `curl (7) Couldn't connect to server`.
- `dotnet run --project backend/CashTrack.Api/CashTrack.Api.csproj --urls http://127.0.0.1:5055`: chạy được.
- `curl -i http://127.0.0.1:5055/health`: trả `200 OK` và `{"status":"ok"}`.
- `curl -i http://127.0.0.1:5055/api/v1/categories`: trả `200 OK`, camelCase.
- `curl -i http://127.0.0.1:5055/api/v1/transactions`: trả `200 OK`, có `items` và `nextCursor`.
- `curl -k -i https://127.0.0.1:5055/health`: fail vì backend đang chạy HTTP, không phải HTTPS.
- CORS preflight từ `http://localhost:8081`: allow; từ `http://192.168.0.108:8081`: chưa allow.
- `curl -i -X POST ... /api/v1/transactions` với `{}`: trả `400 application/problem+json` với `errors.amount/type/category`.
- `curl -i -X POST ... /api/v1/notifications/parse`: trả `200 OK`, response camelCase đúng contract.
- `dotnet ef migrations list --no-build --project backend/CashTrack.Api/CashTrack.Api.csproj --startup-project backend/CashTrack.Api/CashTrack.Api.csproj`: thấy `20260611182529_InitialCreate`.
- `dotnet build backend/CashTrack.Api/CashTrack.Api.csproj`: pass, 0 warning, 0 error.
- `npx tsc --noEmit`: pass.

## 12/06/2026 08:13:07 +0700

### Thay đổi
- Sửa lỗi TypeScript strict mode đã ghi trong `harness-engineering/FAILED_TESTCASE.md`.
- Cập nhật `src/components/common/Card.tsx` để `gradientColors` có kiểu tuple readonly tối thiểu 2 màu, khớp `expo-linear-gradient`.
- Cập nhật `src/theme/colors.ts` để `Theme` là union của `lightTheme` và `darkTheme`.
- Cập nhật `src/store/transactionStore.ts` để dùng `encryptedTransactionStorage` cho persisted transactions và bổ sung `lockInMemoryData`.
- Viết lại `harness-engineering/FAILED_TESTCASE.md` với trạng thái testcase đã pass.

### Lý do
- Làm sạch lỗi `npx tsc --noEmit` project-wide để TypeScript strict mode pass.
- Đồng bộ transaction store với luồng auth/encryption đã có: logout khóa data key và xóa dữ liệu khỏi memory, login/registration hydrate lại sau khi unlock.

### Kiểm tra
- `npx tsc --noEmit`: pass, exit code 0.

## 12/06/2026 07:54:07 +0700

### Thay đổi
- Chẩn đoán luồng nối frontend-backend theo thứ tự: backend runtime, URL base, emulator/device network, CORS, HTTP/HTTPS, route/method, status code, request JSON, response JSON casing, validation và SQLite/migration.
- Xác định root cause vận hành ban đầu: backend không chạy trên `http://127.0.0.1:5055`, khiến request frontend/backend không thể kết nối.
- Khởi động backend local để kiểm chứng endpoint `/health`, `/api/v1/categories`, `/api/v1/transactions` và `/api/v1/notifications/parse`.
- Không sửa source runtime frontend/backend vì API adapter, route, JSON casing, validation và SQLite migration hiện đang khớp khi backend chạy đúng.
- Viết mới `harness-engineering/FAILED_TESTCASE.md` để lưu lỗi `npx tsc --noEmit` theo quy tắc testing của repo.
- Cập nhật `harness-engineering/project_definition.md` và `build.sh` với kết quả chẩn đoán/cách chạy hiện tại.

### Lý do
- Làm rõ lỗi kết nối hiện tại nằm ở backend chưa chạy hoặc cấu hình host khi chạy trên thiết bị thật, không phải lệch API contract.
- Ghi lại rủi ro còn lại: Expo/web qua LAN origin có thể bị CORS chặn, Android release gọi HTTP có thể cần cấu hình cleartext hoặc chuyển sang HTTPS.

### Kiểm tra
- `curl -i --max-time 3 http://127.0.0.1:5055/health`: fail trước khi chạy backend với `curl (7) Couldn't connect to server`.
- `dotnet run --project backend/CashTrack.Api/CashTrack.Api.csproj --urls http://127.0.0.1:5055`: chạy được, Kestrel lắng nghe `127.0.0.1:5055`.
- `curl -i http://127.0.0.1:5055/health`: trả `200 OK` và `{"status":"ok"}`.
- `curl -i http://127.0.0.1:5055/api/v1/categories`: trả `200 OK`, JSON camelCase.
- `curl -i http://127.0.0.1:5055/api/v1/transactions`: trả `200 OK`, JSON có `items` và `nextCursor`.
- `curl -k -i https://127.0.0.1:5055/health`: fail đúng kỳ vọng vì backend hiện chỉ chạy HTTP.
- CORS preflight từ `http://localhost:8081`: được allow; từ `http://192.168.0.108:8081`: không có `Access-Control-Allow-Origin`.
- `curl -i -X POST ... /api/v1/transactions` với `{}`: trả `400 application/problem+json` và `errors.amount/type/category`.
- `curl -i -X POST ... /api/v1/notifications/parse`: trả `200 OK`, response camelCase đúng contract.
- `dotnet ef migrations list --no-build --project backend/CashTrack.Api/CashTrack.Api.csproj --startup-project backend/CashTrack.Api/CashTrack.Api.csproj`: thấy `20260611182529_InitialCreate`.
- `dotnet build backend/CashTrack.Api/CashTrack.Api.csproj`: pass, 0 warning, 0 error.
- `npx tsc --noEmit`: fail bởi lỗi TypeScript có sẵn ngoài phạm vi kết nối API.

## 12/06/2026 02:38:06 +0700

### Thay đổi
- Chỉ sửa `src/services/apiClient.ts` để thêm xử lý loading/error cơ bản ở lớp API adapter.
- Thêm `ApiRequestStatus`, `ApiRequestState<T>` và `ApiCallOptions<T>` với `onStateChange` và `AbortSignal`.
- Các API method như `health`, `listTransactions`, `createTransaction`, `updateTransaction`, `deleteTransaction`, `listCategories`, `listBanks`, `parseNotification` có thể emit state `loading`, `success`, `error`.
- Chuẩn hóa lỗi backend ProblemDetails, lỗi network và lỗi abort thành `ApiClientError`.
- Không sửa Zustand store shape, không sửa màn hình UI, không sửa notification parser.
- Không sửa backend vì response hiện tại đã khớp đủ với DTO frontend adapter.

### Lý do
- Giữ frontend behavior là source of truth và gom toàn bộ logic kết nối backend vào API service/adapter.
- Cho phép caller sau này xử lý loading/error mà không phải tự lặp lại fetch boilerplate hoặc hardcode backend URL trong screen.

### Kiểm tra
- `npx tsc --noEmit`: không có lỗi từ `src/services/apiClient.ts`; project vẫn còn lỗi TypeScript ngoài phạm vi được phép sửa ở `src/components/common/Card.tsx`, `src/store/authStore.ts`, `src/theme/ThemeContext.tsx`, `src/theme/index.ts`.
- `dotnet run --project backend/CashTrack.Api/CashTrack.Api.csproj --no-build --urls http://127.0.0.1:5055`: chạy được backend local.
- `curl -i http://127.0.0.1:5055/health`: trả `200 OK` và `{"status":"ok"}`.
- `curl -i http://127.0.0.1:5055/api/v1/transactions`: trả `200 OK` với `items` và `nextCursor`.

## 12/06/2026 02:28:43 +0700

### Thay đổi
- Thêm DTO TypeScript khớp backend response trong `src/types/api.ts`.
- Thêm fetch API client trong `src/services/apiClient.ts`, dùng `EXPO_PUBLIC_API_BASE_URL` và không hardcode URL trong screen.
- API client có fallback dev URL: Android emulator `http://10.0.2.2:5055`, iOS simulator/web `http://localhost:5055`, máy thật yêu cầu cấu hình LAN IP qua `EXPO_PUBLIC_API_BASE_URL`.
- Export API client/type qua `src/services/index.ts` và `src/types/index.ts`.
- Không sửa UI/screen.
- Cập nhật `build.sh` với cách chạy backend và frontend kèm biến môi trường API.

### Lý do
- Tạo lớp adapter/service duy nhất để frontend gọi backend mà không rải URL hoặc fetch logic vào UI.
- Giữ frontend TypeScript strict mode là source of truth, đồng thời bám DTO camelCase từ backend ASP.NET Core.

### Kiểm tra
- `npx tsc --noEmit`: phần API client/type mới không còn lỗi; project vẫn còn lỗi TypeScript có sẵn ở `src/components/common/Card.tsx`, `src/store/authStore.ts`, `src/theme/ThemeContext.tsx`, `src/theme/index.ts`.
- `dotnet run --project backend/CashTrack.Api/CashTrack.Api.csproj --no-build --urls http://127.0.0.1:5055`: chạy được backend local.
- `curl -i http://127.0.0.1:5055/health`: trả `200 OK` và `{"status":"ok"}`.
- `curl -i http://127.0.0.1:5055/api/v1/transactions`: trả `200 OK` với `items` và `nextCursor`.

## 12/06/2026 02:18:01 +0700

### Thay đổi
- Implement REST API v1 bằng ASP.NET Core minimal API nhất quán.
- Thêm DTO riêng trong `backend/CashTrack.Api/Contracts/ApiContracts.cs`, không expose trực tiếp EF entity ra frontend.
- Thêm endpoint mapping trong `backend/CashTrack.Api/Endpoints/ApiEndpointExtensions.cs` cho transaction CRUD, category/list, bank/list, notification parse, webhook CRUD/send/test và backup JSON import/export.
- Thêm parser notification backend trong `backend/CashTrack.Api/Services/NotificationParsingService.cs`.
- Cập nhật `Program.cs` để bật JSON camelCase, ProblemDetails, CORS cho Expo dev origins, Swagger/OpenAPI và HttpClient cho webhook.
- Thêm package `Swashbuckle.AspNetCore` version `6.5.0` để có Swagger UI/OpenAPI.
- Dùng async EF Core cho truy vấn/ghi dữ liệu và validate input trước khi ghi DB.
- Dùng dev user nội bộ `dev-user` cho API scaffold hiện tại vì contract frontend hiện chưa có auth API backend.
- Cập nhật `build.sh` với lệnh chạy backend trên `http://127.0.0.1:5055` và Swagger tại `/swagger`.

### Lý do
- Cung cấp API backend tối thiểu để frontend React Native có thể tích hợp qua API adapter/service sau này.
- Giữ contract JSON khớp frontend hiện tại, đồng thời tách DTO khỏi EF entity để tránh rò rỉ schema nội bộ.
- Đảm bảo có Swagger để kiểm tra endpoint và có ProblemDetails/validation nhất quán cho lỗi input.

### Kiểm tra
- `dotnet build backend/CashTrack.Api/CashTrack.Api.csproj`: pass, 0 warning, 0 error.
- `dotnet run --project backend/CashTrack.Api/CashTrack.Api.csproj --no-build --urls http://127.0.0.1:5055`: chạy được.
- `curl http://127.0.0.1:5055/swagger/v1/swagger.json`: pass, thấy `/api/v1/transactions` và các endpoint API v1.
- `curl http://127.0.0.1:5055/api/v1/transactions`: pass, trả `200 OK`.
- `curl -X POST http://127.0.0.1:5055/api/v1/transactions ...`: pass, trả `201 Created` với `TransactionDto` camelCase.
- Không chạy `dotnet test` vì không tìm thấy backend test project.

## 12/06/2026 01:56:16 +0700

### Thay đổi
- Tạo thư mục `harness-engineering/` để gom các markdown phục vụ harness engineering.
- Di chuyển các file harness engineering markdown vào `harness-engineering/`: `ERROR_LOG.md`, `missing.md`, `my_api_contract.md`, `my_entity_list.md`, `my_plan.md`, `project_definition.md`, `testcases.md`.
- Cập nhật `AGENTS.md` để quy định đường dẫn mới cho project definition, testcase, error log, API contract, entity list, missing report và kế hoạch.
- Cập nhật `harness-engineering/project_definition.md` theo cấu trúc ghi nhận thay đổi của dự án.
- Viết lại `build.sh` với cách chạy code hiện tại.
- Cập nhật `.gitignore` để markdown trong `harness-engineering/` không bị ignore.

### Lý do
- Giữ root repository gọn hơn, chỉ giữ `AGENTS.md` và `CHANGELOG.md` cho harness-level markdown bắt buộc.
- Gom các tài liệu kỹ thuật/harness vào một nơi để các bước thiết kế, kiểm thử và log không bị rải rác.

## 12/06/2026 01:28:04 +0700

### Thay đổi
- Tạo migration EF Core đầu tiên `InitialCreate` cho backend `backend/CashTrack.Api`.
- Tạo các file migration trong `backend/CashTrack.Api/Migrations`.
- Update SQLite database tại `backend/CashTrack.Api/cashtrack.db` bằng migration `20260611182529_InitialCreate`.
- Kiểm tra `dotnet ef migrations list` thấy migration `20260611182529_InitialCreate`.
- Chạy `dotnet build backend/CashTrack.Api/CashTrack.Api.csproj` thành công.
- Không thêm `IDesignTimeDbContextFactory<AppDbContext>` vì EF CLI đã tạo được `AppDbContext` lúc design-time.
- Cập nhật `project_definition.md` và `build.sh` theo hướng dẫn ghi nhận/thực thi của dự án.

### Lý do
- Khởi tạo schema SQLite thực tế từ mô hình EF Core code-first để backend có database chạy được.
- Xác nhận migration đầu tiên đã được apply vào database và backend vẫn build sạch sau khi sinh migration.

## 11/06/2026 22:02:22 +0700

### Thay đổi
- Thêm backend C# ASP.NET Core trong `backend/CashTrack.Api`.
- Thêm `backend/.gitignore` để bỏ qua `bin/`, `obj/` và file SQLite local sinh khi build/chạy.
- Thêm các entity EF Core code-first theo danh sách đã thiết kế trong `my_entity_list.md`.
- Thêm `AppDbContext` dùng SQLite, seed danh mục giao dịch và danh sách ngân hàng/ ví điện tử.
- Dùng `long` cho các giá trị tiền dạng minor units như `AmountMinorUnits`, `MonthlyBudgetMinorUnits`, `LimitMinorUnits`.
- Dùng `DateTime` với hậu tố `Utc` cho các trường thời gian.
- Thêm index/unique cho các trường hay truy vấn và đồng bộ như `TransactionDateUtc`, `BankCode`, `ExternalId`, `RawNotificationHash`, `RawTextHash`.

### Lý do
- Chuẩn bị nền backend code-first để đồng bộ dữ liệu tài chính từ frontend React Native/Expo/TypeScript strict mode.
- Giữ SQLite làm database mặc định theo quy tắc dự án.
- Tránh lỗi làm tròn tiền khi lưu bằng số thực bằng cách lưu tiền theo minor units.
- Chưa tạo migration vì yêu cầu hiện tại chỉ tạo entity và `AppDbContext`.

Tất cả thay đổi đáng chú ý của dự án sẽ được ghi nhận trong file này.

Format dựa trên [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
và project này tuân theo [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

---

## [1.2.0] - 2025-12-17

### 🚀 Added

#### Smart Advertisement Filtering
- **AI-Powered Detection**: Sử dụng Gemini AI để phân biệt thông báo giao dịch thực và quảng cáo
- **Keyword-Based Filtering**: Bộ lọc từ khóa với 30+ từ khóa quảng cáo tiếng Việt (có/không dấu)
- **Transaction Priority Keywords**: Từ khóa ưu tiên để nhận biết giao dịch thực sự
- **Confidence Scoring**: AI trả về độ tin cậy, chỉ lọc khi >= 70%
- **Fallback Logic**: Nếu AI không chắc chắn, tự động dùng rule-based detection


#### Background Webhook Service
- **Native Background Sending**: Gửi webhook trực tiếp từ Android NotificationListener service
- **No App Required**: Webhook được gửi ngay cả khi app không mở
- **New Event Type**: `notification.received` - event mới cho thông báo từ background
- **Webhook Config Sync**: Tự động đồng bộ cấu hình webhook từ React Native sang native code
- **Offline Queue**: Lưu webhook vào hàng đợi khi không có internet
- **Auto Retry**: Tự động gửi lại webhook khi có kết nối (max 5 lần retry)
- **Queue Expiration**: Webhook hết hạn sau 24 giờ, giữ tối đa 50 items

#### Gemini AI Integration
- **Smart Categorization**: Tích hợp Google Gemini AI để phân loại giao dịch thông minh
- **AI Reports**: Báo cáo tài chính hàng tháng với insights, recommendations và saving tips
- **Income Analysis**: Phân tích nguồn thu nhập tự động
- **Gemini API Configuration**: UI để cấu hình API key trong Settings
- **detectNotificationType()**: Hàm mới để phân loại thông báo là giao dịch hay quảng cáo
- **Extracted Amount**: AI có thể trích xuất số tiền từ thông báo
- **Transaction Type Detection**: AI xác định loại giao dịch (income/expense)

#### Webhook System
- **Webhook Service**: Gửi dữ liệu giao dịch đến third-party services (Discord, Slack, etc.)
- **Multiple Events**: Hỗ trợ các sự kiện:
  - `transaction.created` - Giao dịch mới
  - `transaction.updated` - Cập nhật giao dịch
  - `transaction.deleted` - Xóa giao dịch
  - `budget.exceeded` - Vượt ngân sách
  - `budget.warning` - Cảnh báo ngân sách
  - `daily.summary` - Tổng kết ngày
  - `weekly.summary` - Tổng kết tuần
  - `monthly.summary` - Tổng kết tháng
- **Webhook Management UI**: Thêm, xóa, test webhooks trong Settings
- **HMAC Signature**: Bảo mật webhook với secret key

#### Advanced Filters
- **Date Range Filters**: Lọc giao dịch theo 7 ngày, 30 ngày, tháng/năm
- **Month/Year Picker**: Modal chọn tháng và năm cho thống kê
- **Category Filter**: Modal lọc theo danh mục với UI grid
- **Active Filter Tags**: Hiển thị các filter đang active với nút xóa

#### Custom Budget
- **Flexible Budget Setting**: Đặt ngân sách hàng tháng tùy chỉnh
- **Preset Values**: Quick preset (5M, 10M, 15M, 20M, 30M, 50M VND)
- **Budget Edit Modal**: UI đẹp để chỉnh sửa ngân sách

#### Backup & Restore System
- **Export Data**: Xuất dữ liệu giao dịch ra file Excel (.xlsx) chuẩn định dạng hoặc JSON backup
- **Import Functions**: Khôi phục dữ liệu từ file (JSON/Excel) vào ứng dụng
- **Smart Merging**: Tự động phát hiện giao dịch trùng lặp khi import notify hoặc backup
- **Backup UI**: Giao diện quản lý sao lưu trong Settings

#### Other Improvements
- **Dependencies**: Thêm các thư viện xử lý file (`xlsx`, `expo-file-system`, etc.)
- **AI Integration**: Kết nối trực tiếp Notification Service với Gemini Service để phân loại realtime
- **Settings**: Hiển thị dynamic version từ `app.json`

### 💅 Changed

#### Native NotificationListener.kt
- Thêm logic lọc quảng cáo trực tiếp trong native code
- Thêm khả năng đọc webhook config từ file
- Thêm HTTP client để gửi webhook từ background

#### Notification Processing Flow
- AI detection chạy trước rule-based nếu có API key
- Log chi tiết hơn về quá trình lọc quảng cáo
- Cải thiện hiệu suất với confidence threshold

#### Settings Screen Redesign
- **AI Section**: Mới thêm section "Trí tuệ nhân tạo" với cấu hình Gemini
- **Webhook Section**: Mới thêm section quản lý webhooks
- **Premium Modal UI**: Webhook modal với bottom sheet style, grouped events

#### Stats Screen Enhancements
- **Month/Year Selector**: Picker ở header để chọn thời gian
- **Income Breakdown**: Section hiển thị phân loại thu nhập theo nguồn
- **AI Report Section**: Card hiển thị báo cáo AI với status badge
- **Saving Rate**: Hiển thị tỷ lệ tiết kiệm

#### Transactions Screen Improvements
- **Date Filter Chips**: Quick filters (Tất cả, 7 ngày, 30 ngày, Tháng này)
- **Type Filter Chips**: Filter theo loại (Chi tiêu, Thu nhập)
- **Category Filter Button**: Mở modal chọn danh mục
- **Clear Filters**: Nút xóa tất cả filter
- **Summary Update**: Hiển thị Cân đối thay vì số giao dịch

### 🛠️ Technical

#### New Services & Functions
- `src/services/geminiService.ts` - Gemini AI service
- `src/services/webhookService.ts` - Webhook management service
- `geminiService.detectNotificationType()` - AI notification classification
- `notificationParser.isAdvertisementNotification()` - Rule-based ad detection
- `webhookService.syncToNative()` - Sync webhook config to native
- `NotificationListener.queueWebhookForRetry()` - Queue failed webhooks
- `NotificationListener.processWebhookQueue()` - Process queued webhooks on retry

#### Updated Files
- `src/services/geminiService.ts` - Thêm hàm detectNotificationType
- `src/services/notificationService.ts` - Tích hợp AI ad detection
- `src/services/webhookService.ts` - Native sync và new event type
- `src/utils/notificationParser.ts` - Advertisement keywords và detection
- `android/.../NotificationListener.kt` - Background webhook, ad filtering, và retry queue

#### Constants Added
- `ADVERTISEMENT_KEYWORDS` - 30+ từ khóa quảng cáo tiếng Việt
- `TRANSACTION_PRIORITY_KEYWORDS` - Từ khóa giao dịch ưu tiên
- `notification.received` - New webhook event type

#### Store Updates
- `settingsStore.ts`:
  - Added: `geminiApiKey`, `useAICategorizaton`, `useAIReports`
  - Added: `webhooksEnabled`, `isCustomBudget`
  - Added: `currentFilter` with `setFilterMonth()`, `setFilterYear()`
  
- `transactionStore.ts`:
  - Integrated webhook triggers on transaction CRUD operations
  - Removed unused `uuid` import

### 🐛 Fixed
- Fixed lint error with Promise<void> type in notificationParser.ts
- Removed duplicate month filter state management
- Duplicate `settings` variable declaration in processNotification
- Webhook not triggering when app is closed

---

## [1.1.0] - 2024-12-15

### Added
- Transaction Detail Modal với khả năng xem và chỉnh sửa giao dịch
- Expanded Vietnamese keywords (có dấu và không dấu) trong notification parser
- More merchant patterns for better categorization

### Changed
- Updated README với hướng dẫn đóng góp và liên hệ
- Improved notification parsing accuracy

---

## [1.0.0] - 2024-12-10

### Added
- 🎉 Initial release
- Automatic bank notification reading
- Transaction management (CRUD)
- Dashboard with spending charts
- Category breakdown with pie chart
- Dark/Light theme support
- Monthly budget tracking
- Support for 11 Vietnamese banks and e-wallets
- Zustand state management with persistence
- React Navigation bottom tabs

### Tech Stack
- React Native + Expo SDK 54
- TypeScript
- Zustand + AsyncStorage
- React Navigation 7
- react-native-chart-kit
- expo-linear-gradient

---

## 🔮 Roadmap

### Upcoming Features
- [ ] Export to Excel/PDF
- [ ] Cloud sync with Firebase
- [ ] Recurring transactions
- [ ] Bill reminders
- [ ] Multi-currency support
- [ ] Family/Group expense sharing
- [ ] Receipt scanning with OCR
- [ ] Bank account integration (Open Banking)

---

## 📝 Notes

- Versions follow [Semantic Versioning](https://semver.org/)
- Dates are in YYYY-MM-DD format
- Each version section contains: Added, Changed, Fixed, Removed subsections

---

<p align="center">
  <strong>CashTrack</strong> - Made with ❤️ by <a href="https://github.com/phamquangvinhfpt">Phạm Quang Vinh</a>
</p>
