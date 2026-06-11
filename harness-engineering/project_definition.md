# 12/06/2026 01:28:04 +0700

## Nội dung đã thay đổi
- Tạo migration EF Core đầu tiên tên `InitialCreate` cho backend C# ASP.NET Core.
- Update SQLite database từ migration vừa tạo.
- Xác nhận `dotnet ef migrations list` thấy `20260611182529_InitialCreate`.
- Xác nhận `dotnet build backend/CashTrack.Api/CashTrack.Api.csproj` pass.
- Không thêm `IDesignTimeDbContextFactory<AppDbContext>` vì DbContext hiện tại đã tạo được lúc design-time.

## Project Definition

### Input Data
- Dữ liệu giao dịch tài chính từ frontend React Native/Expo/TypeScript strict mode.
- Dữ liệu lưu cục bộ hiện có từ Zustand store và AsyncStorage.
- Dữ liệu thông báo ngân hàng từ notification parser/native NotificationListener.
- Dữ liệu import/restore từ file backup JSON/XLSX.
- Dữ liệu cấu hình người dùng như ngân sách, danh mục, ngân hàng được chọn, AI settings và webhook settings.

### Methodology
- Frontend TypeScript strict mode là source of truth cho API contract.
- Backend dùng C# ASP.NET Core, EF Core code-first và SQLite.
- Tiền được lưu bằng minor units dạng `long` để tránh lỗi làm tròn.
- Thời gian lưu bằng `DateTime` UTC với tên trường rõ ràng có hậu tố `Utc`.
- Schema backend được quản lý bằng EF Core migration.
- Không tự ý sửa UI; nếu cần kết nối backend từ frontend thì chỉ sửa lớp API adapter/service và type contract liên quan.

### Expected Results
- Backend có schema SQLite đồng bộ với contract dữ liệu hiện có của frontend.
- Migration đầu tiên `InitialCreate` tạo đầy đủ bảng, quan hệ, index, unique constraint và seed data nền.
- Database SQLite local được update và có thể dùng để chạy backend.
- Backend build thành công sau khi tạo migration và update database.

## Flow hệ thống sau thay đổi
1. Người dùng thao tác trên app React Native/Expo.
2. Frontend quản lý state qua Zustand và lưu local bằng AsyncStorage.
3. NotificationListener/notification parser đọc thông báo ngân hàng, lọc quảng cáo và trích xuất giao dịch.
4. API adapter/service là lớp duy nhất nên dùng để trao đổi dữ liệu với backend khi tích hợp đồng bộ.
5. Backend ASP.NET Core nhận dữ liệu theo contract từ frontend.
6. EF Core ánh xạ entity code-first sang SQLite.
7. Migration `InitialCreate` định nghĩa schema ban đầu và được apply vào `backend/CashTrack.Api/cashtrack.db`.
8. Webhook/import/backup/restore sử dụng schema backend để lưu lịch sử xử lý, log giao hàng, batch import và dữ liệu đồng bộ.

# 12/06/2026 01:44:08 +0700

## Nội dung đã thay đổi
- Thiết kế REST API contract tối thiểu cho app React Native/Expo hiện tại.
- Lưu contract vào `my_api_contract.md`.
- Contract ưu tiên transaction CRUD, category/list, bank/list, notification parse, webhook send/test và backup import/export.
- Không sửa source code frontend, backend, UI, store hay service.

## Project Definition

### Input Data
- Dữ liệu giao dịch từ frontend `Transaction`: `id`, `amount`, `type`, `category`, `description`, `merchant`, `bankAccount`, `source`, `rawNotification`, `createdAt`, `updatedAt`.
- Danh mục từ `CATEGORIES` và danh sách ngân hàng từ `BANKS`.
- Dữ liệu notification từ `BankNotification`: `app`, `title`, `text`, `time`, `extra`, `processed`.
- Dữ liệu webhook từ `WebhookConfig`, `WebhookEvent`, `WebhookPayload`, `WebhookDeliveryResult`.
- Dữ liệu backup/import/export JSON/XLSX theo `backupService`.

### Methodology
- Frontend React Native/Expo/TypeScript strict mode vẫn là source of truth cho API contract.
- REST API dùng JSON, date/time qua API dùng ISO UTC string.
- Amount giữ tên `amount` trong API để khớp frontend, backend map sang `long AmountMinorUnits`.
- Endpoint parse notification chỉ parse và trả kết quả, không tự tạo transaction để tránh side effect ngoài ý muốn.
- Webhook secret không được trả plaintext trong response.
- Backup import/export hỗ trợ JSON/XLSX vì frontend hiện có hai flow này.

### Expected Results
- Backend có tài liệu contract tối thiểu để triển khai controller/DTO mà không lệch frontend.
- Frontend sau này có thể thêm API adapter/service mà không cần sửa UI.
- Transaction CRUD, category/list, notification parser, webhook delivery/test và backup import/export có status code và validation rule rõ ràng.

## Flow hệ thống sau thay đổi
1. Frontend tạo, sửa, xóa, lọc giao dịch qua API adapter/service.
2. API transaction CRUD trả `TransactionDto` tương thích type frontend.
3. Frontend lấy metadata qua `/api/v1/categories` và `/api/v1/banks`.
4. Notification listener gửi `BankNotificationDto` lên `/api/v1/notifications/parse`.
5. Backend parse notification, trả `ParsedTransactionDto` và `suggestedCategory`; frontend quyết định có tạo transaction hay không.
6. Khi transaction thay đổi, frontend hoặc backend trigger webhook qua endpoint send/test.
7. Backup export trả JSON/XLSX; backup import nhận file hoặc danh sách transaction và trả thống kê import/update/skip.

# 12/06/2026 01:56:16 +0700

## Nội dung đã thay đổi
- Tạo thư mục `harness-engineering/` để lưu chung các markdown phục vụ harness engineering.
- Di chuyển `ERROR_LOG.md`, `missing.md`, `my_api_contract.md`, `my_entity_list.md`, `my_plan.md`, `project_definition.md`, `testcases.md` vào `harness-engineering/`.
- Giữ `AGENTS.md` và `CHANGELOG.md` ở root repository.
- Cập nhật `AGENTS.md` để từ nay các file markdown harness engineering mặc định được lưu dưới `harness-engineering/`.
- Không sửa frontend UI, frontend service/store, backend implementation hay migration.
- Cập nhật `.gitignore` để các markdown trong `harness-engineering/` có thể được git nhìn thấy.

## Project Definition

### Input Data
- Source code frontend React Native/Expo/TypeScript strict mode.
- Backend C# ASP.NET Core/EF Core/SQLite hiện có.
- Các tài liệu harness engineering: plan, entity list, API contract, missing report, testcase, error log và project definition.
- Quy tắc làm việc trong `AGENTS.md` và lịch sử thay đổi trong `CHANGELOG.md`.

### Methodology
- Giữ root repository gọn bằng cách để `AGENTS.md` và `CHANGELOG.md` ở root, còn tài liệu harness engineering ở `harness-engineering/`.
- Không tạo markdown harness engineering mới ở root nếu không có yêu cầu rõ.
- Khi cần ghi project definition sau mỗi prompt, append vào `harness-engineering/project_definition.md`.
- Khi cần ghi log lỗi testcase hoặc Expo/mobile, ghi vào `harness-engineering/FAILED_TESTCASE.md` hoặc `harness-engineering/ERROR_LOG.md`.
- Tiếp tục giữ frontend là source of truth cho API contract và backend dùng C# ASP.NET Core, EF Core code-first, SQLite.

### Expected Results
- Các tài liệu kỹ thuật phụ không còn rải rác ở root repository.
- Agent có quy tắc rõ ràng để lưu tài liệu harness engineering về sau.
- Cấu trúc project dễ đọc hơn mà không ảnh hưởng runtime code.

## Flow hệ thống sau thay đổi
1. Người dùng đưa yêu cầu.
2. Agent đọc `AGENTS.md` ở root để lấy quy tắc làm việc.
3. Nếu cần ghi changelog, agent cập nhật đầu file `CHANGELOG.md` ở root.
4. Nếu cần ghi tài liệu harness engineering, agent lưu vào `harness-engineering/`.
5. Nếu cần ghi project definition, agent append vào `harness-engineering/project_definition.md`.
6. Nếu cần ghi cách chạy code, agent viết lại `build.sh` ở root.
7. Source frontend/backend vẫn giữ nguyên trừ khi người dùng yêu cầu sửa code cụ thể.

# 12/06/2026 02:18:01 +0700

## Nội dung đã thay đổi
- Implement REST API v1 cho backend ASP.NET Core dựa trên `harness-engineering/my_api_contract.md`.
- Thêm DTO riêng, endpoint minimal API, notification parser service và Swagger/OpenAPI.
- Bật JSON camelCase, ProblemDetails, CORS cho Expo dev origin và HttpClient webhook.
- Test runtime được Swagger JSON, GET transaction và POST transaction.
- Không sửa frontend UI, frontend store/service hay API adapter trong lượt này.

## Project Definition

### Input Data
- Frontend React Native/Expo/TypeScript strict mode là source of truth cho API contract.
- REST API contract đã lưu trong `harness-engineering/my_api_contract.md`.
- EF Core entities và `AppDbContext` backend hiện có.
- SQLite database đã có migration `InitialCreate`.
- Transaction request/response, category metadata, bank metadata, bank notification, webhook config và backup JSON data.

### Methodology
- Backend dùng ASP.NET Core minimal API nhất quán.
- DTO tách riêng trong `Contracts`, không trả trực tiếp EF entity.
- Endpoint dùng async EF Core, validate thủ công và trả `ValidationProblem`/`ProblemDetails` khi lỗi.
- JSON response giữ camelCase để khớp frontend.
- Swagger/OpenAPI được bật để kiểm tra endpoint qua `/swagger` và `/swagger/v1/swagger.json`.
- CORS cho Expo dev origins được bật bằng policy `ExpoDev`.
- Vì auth backend chưa nằm trong contract hiện tại, API scaffold dùng dev user nội bộ `dev-user` để có thể test transaction CRUD.

### Expected Results
- Backend chạy được và có Swagger hiển thị API v1.
- Frontend có thể tích hợp transaction CRUD qua adapter/service mà không cần sửa UI.
- GET/POST transaction hoạt động với DTO camelCase.
- Category/list, bank/list, notification parse, webhook send/test và backup JSON import/export có endpoint sẵn để tiếp tục tích hợp.

## Flow hệ thống sau thay đổi
1. Developer chạy backend bằng `dotnet run --project backend/CashTrack.Api/CashTrack.Api.csproj --urls http://127.0.0.1:5055`.
2. Swagger UI mở tại `http://127.0.0.1:5055/swagger` và đọc OpenAPI JSON tại `/swagger/v1/swagger.json`.
3. Frontend/API client gọi `/api/v1/transactions` để list/create/update/delete transaction.
4. API validate request; nếu lỗi trả ProblemDetails hoặc ValidationProblem camelCase.
5. API map DTO sang EF entity, lưu bằng async EF Core vào SQLite.
6. API map EF entity về DTO trước khi trả frontend.
7. Notification client gửi notification tới `/api/v1/notifications/parse`; backend trả parsed transaction và suggested category, không tự tạo transaction.
8. Webhook endpoints lưu config và có thể test/send event qua HttpClient.
9. Backup endpoint export/import JSON transaction cho user dev hiện tại.

# 12/06/2026 02:28:43 +0700

## Nội dung đã thay đổi
- Đọc lại frontend React Native/Expo hiện tại và backend DTO contract đang có.
- Tạo `src/types/api.ts` chứa DTO TypeScript cho health, transaction, category, bank, notification parse, webhook delivery và import result.
- Tạo `src/services/apiClient.ts` dùng `fetch`, JSON camelCase, typed request/response và `ApiClientError` cho ProblemDetails.
- Dùng `EXPO_PUBLIC_API_BASE_URL` làm cấu hình chính cho API base URL.
- Thêm fallback dev URL theo nền tảng: Android emulator dùng `10.0.2.2`, iOS simulator/web dùng `localhost`, máy thật bắt buộc dùng LAN IP qua biến môi trường.
- Export API client và API DTO qua `src/services/index.ts` và `src/types/index.ts`.
- Không sửa UI/screen, store hoặc flow giao diện.
- Kiểm tra backend `/health` và `/api/v1/transactions` trả response đúng shape cho client.

## Project Definition

### Input Data
- Frontend React Native/Expo/TypeScript strict mode là nguồn chính cho kiểu dữ liệu và cách app tiêu thụ dữ liệu.
- Backend ASP.NET Core hiện có cung cấp REST API camelCase, health check và API v1 transaction/category/bank/notification/webhook/backup.
- DTO backend trong `backend/CashTrack.Api/Contracts/ApiContracts.cs`.
- Transaction data từ `src/types/transaction.ts`, notification/bank data từ `src/types/notification.ts`.
- Runtime API base URL lấy từ `EXPO_PUBLIC_API_BASE_URL`.

### Methodology
- Không hardcode URL backend trong screen/UI.
- Mọi request backend đi qua lớp API adapter/service `src/services/apiClient.ts`.
- DTO TypeScript được tách riêng trong `src/types/api.ts` để không phụ thuộc trực tiếp vào EF entity backend.
- API client ưu tiên `EXPO_PUBLIC_API_BASE_URL`; nếu thiếu thì tự chọn URL dev an toàn cho emulator/simulator, còn máy thật phải dùng LAN IP rõ ràng.
- Request dùng `fetch`, header JSON, async Promise, query string encoder và parse lỗi ProblemDetails.
- Không sửa UI nếu không bắt buộc; thay đổi hiện tại chỉ nằm ở service/type export.

### Expected Results
- Frontend có thể gọi `apiClient.health()` để kiểm tra `/health`.
- Frontend có thể gọi `apiClient.listTransactions()` và `apiClient.createTransaction()` để làm việc với `/api/v1/transactions`.
- Các screen sau này chỉ cần import service thay vì tự biết backend URL.
- Android emulator, iOS simulator và máy thật có hướng cấu hình API base URL rõ ràng.
- TypeScript strict mode không phát sinh lỗi mới từ API client/type vừa thêm.

## Flow hệ thống sau thay đổi
1. Developer chạy backend ASP.NET Core ở `http://127.0.0.1:5055` hoặc host LAN tương ứng.
2. Developer chạy Expo với `EXPO_PUBLIC_API_BASE_URL` phù hợp môi trường.
3. Screen hoặc store frontend gọi `apiClient` từ `src/services/apiClient.ts` khi cần dữ liệu backend.
4. `apiClient` resolve base URL từ biến môi trường hoặc fallback dev theo platform.
5. `apiClient.health()` gọi `/health` và nhận `HealthResponse`.
6. `apiClient.listTransactions()` gọi `/api/v1/transactions` và nhận `TransactionListResponse`.
7. `apiClient.createTransaction()` gửi `CreateTransactionRequest` dạng JSON lên backend và nhận `TransactionDto`.
8. Nếu backend trả lỗi JSON ProblemDetails, client ném `ApiClientError` kèm `status` và `problem` để lớp gọi xử lý.
9. UI hiện tại chưa bị thay đổi; việc tích hợp vào screen sẽ là bước riêng nếu cần.

# 12/06/2026 02:38:06 +0700

## Nội dung đã thay đổi
- Chỉ sửa lớp API service/adapter `src/services/apiClient.ts`.
- Thêm loading/error state callback qua `ApiCallOptions<T>.onStateChange`.
- Thêm `AbortSignal` để caller có thể hủy request khi lifecycle UI cần, nhưng không sửa UI hiện tại.
- Mọi request tiếp tục dùng `EXPO_PUBLIC_API_BASE_URL` và fallback platform đã có.
- Chuẩn hóa lỗi từ backend ProblemDetails, lỗi network và lỗi abort thành `ApiClientError`.
- Không đổi Zustand store shape, không sửa screen, không sửa parser thông báo ngân hàng.
- Không sửa backend vì response `/health` và `/api/v1/transactions` đang khớp DTO frontend adapter.

## Project Definition

### Input Data
- Frontend React Native/Expo/TypeScript strict mode là source of truth về behavior.
- Backend ASP.NET Core trả JSON camelCase theo DTO trong `backend/CashTrack.Api/Contracts/ApiContracts.cs`.
- API base URL lấy từ `EXPO_PUBLIC_API_BASE_URL`, với fallback dev theo platform.
- API service/adapter là ranh giới duy nhất để nối backend từ frontend.

### Methodology
- Không hardcode URL trong screen.
- Không đổi shape của Zustand store hiện tại.
- Không sửa màn hình UI hoặc parser thông báo ngân hàng khi chỉ nối backend.
- Nếu backend response lệch frontend expectation thì ưu tiên đề xuất/sửa backend trước khi ép frontend đổi behavior.
- API adapter chịu trách nhiệm encode query, serialize body, parse JSON response, parse ProblemDetails, emit loading/success/error state và ném `ApiClientError` nhất quán.
- TypeScript strict mode được dùng để kiểm tra adapter; các lỗi ngoài phạm vi adapter không được sửa trong prompt này vì ràng buộc của người dùng.

### Expected Results
- `apiClient.health()` gọi được `/health` và có thể emit `loading -> success/error`.
- `apiClient.listTransactions()` gọi được `/api/v1/transactions` và có thể emit `loading -> success/error`.
- Caller tương lai có thể dùng `onStateChange` để cập nhật loading/error mà không cần sửa logic fetch trong screen.
- Backend contract hiện tại vẫn là nguồn response; frontend adapter ánh xạ đúng DTO mà không đổi behavior/store/parser.

## Flow hệ thống sau thay đổi
1. Frontend gọi method trên `apiClient`.
2. `apiClient` resolve base URL từ `EXPO_PUBLIC_API_BASE_URL` hoặc fallback theo platform.
3. `apiClient` emit state `loading` qua `onStateChange` nếu caller cung cấp callback.
4. `apiClient` gửi request bằng `fetch`, kèm JSON header/body khi cần.
5. Backend ASP.NET Core xử lý request và trả DTO JSON camelCase hoặc ProblemDetails.
6. Nếu response thành công, `apiClient` emit `success` kèm data và trả Promise resolve.
7. Nếu response lỗi hoặc network lỗi, `apiClient` emit `error` kèm `ApiClientError` và Promise reject.
8. Store/UI/parser hiện tại không bị thay đổi; tích hợp vào màn hình sẽ là bước riêng nếu người dùng cho phép sửa UI hoặc store action.

# 12/06/2026 02:57:02 +0700

## Nội dung đã thay đổi
- Phân tích lỗi app Android không load được project từ ảnh người dùng gửi.
- Đọc `package.json`, `app.json`, `eas.json`, `metro.log`, native Android config và notification service để xác định ngữ cảnh Expo Go/development build.
- Tạo file `harness-engineering/expo_android_loading_error_analysis_plan.md` chứa phân tích nguyên nhân và kế hoạch sửa lỗi.
- Viết mới `harness-engineering/ERROR_LOG.md` theo quy tắc khi app lỗi trên điện thoại/Expo Go.
- Cập nhật `build.sh` với hướng dẫn chạy Expo development build, tunnel, LAN và backend cho điện thoại thật.
- Không sửa source code app, store, UI, parser hoặc backend implementation.

## Project Definition

### Input Data
- Ảnh lỗi Android từ người dùng: development build báo `DebugServerException`, Metro trả `404` cho `/_expo/loading.bundle`.
- Log local `metro.log` cho thấy port `8081` đang bị process khác chiếm và Expo CLI đã skip dev server trong non-interactive mode.
- Frontend React Native/Expo/TypeScript strict mode với `expo-dev-client`, native Android package `com.cashtrack.app` và notification listener permission.
- Backend C# ASP.NET Core/SQLite hiện có, không liên quan trực tiếp đến lỗi Metro bundle port `8081`.

### Methodology
- Phân biệt lỗi Metro/dev client với lỗi backend API.
- Ưu tiên frontend behavior và cách chạy đúng môi trường: Expo Go thuần cho test giới hạn, development build cho native notification listener.
- Không sửa code khi người dùng chỉ yêu cầu phân tích và kế hoạch.
- Lưu phân tích/kế hoạch vào `harness-engineering/` và ghi log lỗi điện thoại vào `ERROR_LOG.md`.
- Cập nhật cách chạy code để tránh lặp lại lỗi port `8081` bị chiếm hoặc chạy sai chế độ.

### Expected Results
- Có tài liệu rõ ràng về nguyên nhân khả dĩ nhất: Metro server cũ/sai đang chiếm `8081`, khiến thiết bị nhận `404` khi tải bundle.
- Có kế hoạch sửa theo từng bước: dừng Metro cũ, chạy `expo start --dev-client --lan --clear`, dùng tunnel nếu LAN lỗi, rebuild dev client nếu native build cũ, và cấu hình backend LAN sau khi bundle load được.
- Không làm thay đổi behavior runtime của app trong bước phân tích này.

## Flow hệ thống sau thay đổi
1. Người dùng gặp lỗi app Android không load được JS bundle.
2. Agent ghi nhận lỗi vào `harness-engineering/ERROR_LOG.md`.
3. Agent phân tích repo và lưu kế hoạch tại `harness-engineering/expo_android_loading_error_analysis_plan.md`.
4. Developer dừng Metro server cũ đang chiếm `8081`.
5. Developer chạy Metro đúng chế độ development build bằng `npx expo start --dev-client --lan --clear` hoặc tunnel nếu LAN bị chặn.
6. Development build CashTrack trên điện thoại tải JS bundle từ Metro đúng project.
7. Sau khi bundle load thành công, frontend mới gọi backend qua API adapter; nếu dùng điện thoại thật thì backend cần bind `0.0.0.0:5055` và frontend dùng `EXPO_PUBLIC_API_BASE_URL=http://<LAN-IP>:5055`.
