# Bao cao loi qua anh chup man hinh dien thoai

Thoi diem ghi nhan: 12/06/2026 09:05:06 +0700

## Nguon loi

Nguoi dung gui anh chup man hinh Android khi load app bang Expo Go.

## Thong bao loi trong anh

```text
[runtime not ready]: Error: Cannot find native module 'ExpoCryptoAES'
stack:
requireNativeModule@...
loadModuleImplementation@...
guardedLoadModule@...
metroRequire@...
```

## Phan tich nguyen nhan

Day la loi runtime native module, khong phai loi backend API, khong phai loi navigation, va khong phai loi Metro 404 truoc do.

Frontend hien tai import `expo-crypto` trong:

- `src/services/securityService.ts`
- `src/services/authService.ts`

Trong `src/services/securityService.ts`, code dang goi cac API AES:

- `Crypto.AESEncryptionKey.import(...)`
- `Crypto.aesEncryptAsync(...)`
- `Crypto.AESSealedData.fromCombined(...)`
- `Crypto.aesDecryptAsync(...)`

Khi JS bundle duoc load trong Expo Go, native runtime khong tim thay module `ExpoCryptoAES`, nen app do man hinh do truoc khi React tree san sang render.

Nguyen nhan kha nang cao:

1. `expo-crypto`/native AES module khong co trong binary Expo Go dang chay, hoac khong khop voi Expo SDK/native runtime hien tai.
2. App dang dung cac API native AES khong the them vao Expo Go bang JS thuong. Neu can native module moi, phai dung development build/dev client va rebuild native app.
3. `package.json` hien tai chua khai bao truc tiep `expo-crypto`, `node-forge`, `bcryptjs`, trong khi source code dang import cac package nay. Dieu nay co the gay loi cai dat/phien ban va can duoc chuan hoa o buoc sua sau.
4. Project da co `expo-dev-client` va quyen Android `BIND_NOTIFICATION_LISTENER_SERVICE`; cac tinh nang native nhu vay phu hop hon voi development build thay vi Expo Go thuan.

## Ket luan ky thuat

Neu muon chay dung voi tinh nang ma app dang co, huong phu hop nhat la dung development build/dev client cua CashTrack, cai dung dependency native, va rebuild app. Expo Go chi phu hop de test JS/UI gioi han; no khong dam bao co native module rieng ma app dang can.

Neu bat buoc phai chay bang Expo Go, can thay doi lop crypto de khong phu thuoc vao `ExpoCryptoAES` khi app startup, hoac thay AES native bang mot implementation tuong thich Expo Go. Huong nay can can nhac bao mat vi app dang ma hoa du lieu giao dich.

## Ke hoach sua loi de xuat

### Buoc 1: Xac nhan duong import gay crash

- Chay TypeScript check de xem dependency/type hien tai co loi compile khong: `npx tsc --noEmit`.
- Tim toan bo import crypto/native lien quan bang `rg "expo-crypto|AESEncryptionKey|aesEncryptAsync|aesDecryptAsync|AESSealedData" src package.json`.
- Xac nhan app crash truoc render do import `securityService.ts` hoac `authService.ts`.

### Buoc 2: Chon huong runtime chinh

Huong khuyen nghi:

- Khong dung Expo Go cho ban co native module.
- Dung development build/dev client vi project da co `expo-dev-client` va native notification behavior.
- Cai dependency khop Expo SDK bang lenh du kien: `npx expo install expo-crypto`.
- Neu auth/session can luu token tren Android/iOS, cai them `expo-secure-store` bang lenh du kien: `npx expo install expo-secure-store`.
- Rebuild native app: `npx expo run:android`.
- Chay Metro cho dev client: `npx expo start --dev-client --lan --clear`.

Huong chi khi bat buoc ho tro Expo Go:

- Khong goi AES native API cua `expo-crypto` o startup.
- Sua `src/services/securityService.ts` de dung implementation AES-GCM tuong thich Expo Go hoac lazy-load crypto chi sau khi runtime duoc xac nhan ho tro.
- Giu nguyen UI, navigation, Zustand store shape, AsyncStorage schema va notification parser.
- Danh gia lai muc do bao mat neu thay native AES bang JS implementation.

### Buoc 3: Pham vi file du kien sua neu duoc phep fix sau

- `package.json` va `package-lock.json`: khai bao dung dependency thieu/khong ro rang nhu `expo-crypto`, `node-forge`, `bcryptjs`, va co the `expo-secure-store`.
- `src/services/securityService.ts`: sua cach dung AES de phu hop runtime da chon.
- `src/services/authService.ts`: giu token/session khong luu AsyncStorage; neu can persistent secure session thi dung `expo-secure-store`.
- `src/services/encryptedStorageService.ts`: chi sua neu can dong bo voi crypto adapter moi.
- Khong sua UI/screen/navigation neu khong bat buoc.

### Buoc 4: Kiem chung sau khi fix

- `npx tsc --noEmit` phai pass.
- Neu dung development build: `npx expo run:android` build thanh cong.
- Chay `npx expo start --dev-client --lan --clear`.
- Mo app tren dien thoai va xac nhan khong con red screen `Cannot find native module 'ExpoCryptoAES'`.
- Test dang ky/dang nhap neu flow auth dang duoc bat.
- Test du lieu giao dich co the load/lap lai sau reload ma khong mat AsyncStorage data cu.

## Viec khong lam trong luot nay

- Khong sua source code frontend/backend.
- Khong sua UI, navigation, Zustand store, AsyncStorage schema hoac notification parser.
- Khong chay install/build vi nguoi dung chi yeu cau ke hoach.
