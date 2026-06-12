# Ke hoach fix Expo Go loi ExpoCryptoAES

Thoi diem lap ke hoach: 12/06/2026 09:31:41 +0700

## Muc tieu

- Fix bang duoc loi Expo Go da ghi trong `harness-engineering/error_report_via_phone_screenshot.md`: `[runtime not ready]: Error: Cannot find native module 'ExpoCryptoAES'`.
- Giu yeu cau cua nguoi dung: nhat quyet dung Expo Go, nen khong yeu cau dev client de giai quyet loi nay.
- Bo phu thuoc runtime vao AES native cua `expo-crypto` trong duong startup/auth/encrypted storage.

## File/vung se sua

- `src/services/securityService.ts`: thay AES-256-GCM native `expo-crypto` bang AES-GCM thuan JS qua `node-forge`; them helper random/UUID khong import `expo-crypto`.
- `src/services/authService.ts`: bo import `expo-crypto`, dung helper random/UUID tu `securityService`.
- `package.json` va `package-lock.json`: khai bao ro cac dependency JS dang duoc source import neu can.
- `harness-engineering/FAILED_TESTCASE.md`: chi viet moi neu lenh kiem chung fail.
- `CHANGELOG.md`, `harness-engineering/project_definition.md`, `build.sh`: cap nhat sau thay doi theo quy tac repo.

## Khong sua

- Khong sua UI, screen, navigation, theme, layout hoac text hien thi.
- Khong sua backend/API contract/EF migration.
- Khong doi AsyncStorage key hay shape store.

## Kiem chung du kien

- Chay `npx tsc --noEmit`.
- Chay `npm ls bcryptjs node-forge` de xac nhan dependency JS duoc khai bao.
- Kiem tra source khong con `ExpoCryptoAES`, `AESEncryptionKey`, `aesEncryptAsync`, `aesDecryptAsync` trong `src`.
