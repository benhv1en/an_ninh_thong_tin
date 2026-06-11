# Testcases

## 29/05/2026 10:04:29 +07

### Tên kịch bản kiểm thử

Đăng ký tài khoản mới và khởi tạo bảo mật AES/RSA/bcrypt.

### Mục đích của kịch bản kiểm thử

Đảm bảo người dùng có thể tạo tài khoản, mật khẩu được băm bằng bcrypt + salt, khóa dữ liệu AES-256 được tạo, khóa dữ liệu được bọc bằng mật khẩu, và phiên RSA được thiết lập.

### Đầu vào kiểm thử

- Họ tên: Nguyễn Văn A.
- Email: user@example.com.
- Mật khẩu: Password123.
- Xác nhận mật khẩu: Password123.

### Đầu ra mong đợi sau khi kiểm thử

- App tạo account trong `cashtrack-auth`.
- `passwordHash` là bcrypt hash.
- `passwordSalt` tồn tại.
- `encryptedDataKey` tồn tại và không phải plaintext data key.
- `session.secureChannel.algorithm` là `RSA-OAEP-SHA256`.
- Người dùng được chuyển vào bottom tabs.

### Kết quả kiểm thử

- Đã kiểm tra bằng TypeScript compile: `npx tsc --noEmit` pass.
- Chưa chạy kiểm thử thủ công trên thiết bị thật trong phiên này.

---

## 29/05/2026 10:04:29 +07

### Tên kịch bản kiểm thử

Đăng nhập sai mật khẩu không mở khóa dữ liệu giao dịch.

### Mục đích của kịch bản kiểm thử

Đảm bảo bcrypt compare chặn mật khẩu sai và transaction store không được rehydrate khi chưa có khóa AES hợp lệ.

### Đầu vào kiểm thử

- Account đã đăng ký: user@example.com.
- Mật khẩu nhập sai: WrongPassword123.

### Đầu ra mong đợi sau khi kiểm thử

- App hiển thị lỗi email hoặc mật khẩu không đúng.
- `isAuthenticated` vẫn là `false`.
- Không hiển thị bottom tabs.
- Không gọi mở khóa data key.

### Kết quả kiểm thử

- Đã kiểm tra bằng TypeScript compile: `npx tsc --noEmit` pass.
- Chưa chạy kiểm thử thủ công trên thiết bị thật trong phiên này.

---

## 29/05/2026 10:04:29 +07

### Tên kịch bản kiểm thử

Lưu giao dịch sau đăng nhập và mã hóa local storage bằng AES-256.

### Mục đích của kịch bản kiểm thử

Đảm bảo giao dịch chỉ được persist xuống AsyncStorage dưới dạng encrypted envelope, không lưu JSON plaintext.

### Đầu vào kiểm thử

- Người dùng đã đăng nhập thành công.
- Thêm giao dịch chi tiêu:
  - Amount: 50000.
  - Type: expense.
  - Category: food.
  - Description: Ăn trưa.

### Đầu ra mong đợi sau khi kiểm thử

- Giao dịch xuất hiện trong Home/Transactions/Stats.
- AsyncStorage key `cashtrack-transactions` chứa `__cashtrackEncrypted: true`.
- Envelope có `algorithm: AES-256-GCM`.
- Payload là bản mã base64, không chứa mô tả giao dịch plaintext.

### Kết quả kiểm thử

- Đã kiểm tra bằng TypeScript compile: `npx tsc --noEmit` pass.
- Chưa chạy kiểm thử thủ công trên thiết bị thật trong phiên này.

---

## 29/05/2026 10:04:29 +07

### Tên kịch bản kiểm thử

Đăng xuất khóa dữ liệu trong bộ nhớ.

### Mục đích của kịch bản kiểm thử

Đảm bảo khi đăng xuất, khóa AES bị xóa khỏi memory, transaction state runtime bị clear, và người dùng bị đưa về màn hình đăng nhập.

### Đầu vào kiểm thử

- Người dùng đã đăng nhập.
- Có ít nhất một giao dịch trong transaction store.
- Người dùng bấm Cài đặt > Đăng xuất > xác nhận Đăng xuất.

### Đầu ra mong đợi sau khi kiểm thử

- `isAuthenticated` chuyển thành `false`.
- `session` bị xóa.
- Transaction state runtime rỗng.
- Dữ liệu giao dịch đã mã hóa trên máy không bị xóa.
- App hiển thị `AuthScreen`.

### Kết quả kiểm thử

- Đã kiểm tra bằng TypeScript compile: `npx tsc --noEmit` pass.
- Chưa chạy kiểm thử thủ công trên thiết bị thật trong phiên này.
