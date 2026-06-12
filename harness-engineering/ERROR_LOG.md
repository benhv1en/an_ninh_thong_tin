# Expo Go Android runtime error log

Thoi diem ghi nhan: 12/06/2026 09:05:06 +0700

## Nguon loi

Nguoi dung gui anh chup man hinh Android khi load app bang Expo Go.

## Noi dung loi trong anh

```text
[runtime not ready]: Error: Cannot find native module 'ExpoCryptoAES'
```

Stack hien thi cac frame `requireNativeModule`, `loadModuleImplementation`, `guardedLoadModule`, `metroRequire`.

## Chan doan ngan

Day la loi native module bi thieu trong runtime Expo Go. Source hien tai import `expo-crypto` va dung cac API AES trong `src/services/securityService.ts`, cu the la `AESEncryptionKey`, `aesEncryptAsync`, `AESSealedData` va `aesDecryptAsync`.

Expo Go khong the tu dong co them native module rieng sau khi JS import. Neu app can module native nay, can dung development build/dev client va rebuild native app, hoac thay duong crypto de tuong thich Expo Go.

## Huong xu ly de xuat

1. Huong khuyen nghi: dung development build/dev client, cai dung dependency native va rebuild Android app.
2. Neu bat buoc chay Expo Go: sua lop crypto/service de khong phu thuoc `ExpoCryptoAES` o startup hoac thay bang implementation tuong thich Expo Go.
3. Giu nguyen UI, navigation, Zustand store, AsyncStorage schema va notification parser khi fix.

Bao cao va ke hoach day du da luu tai `harness-engineering/error_report_via_phone_screenshot.md`.
