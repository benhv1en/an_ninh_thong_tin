# Expo Android loading error log

Thoi diem ghi nhan: 12/06/2026 02:57:02 +0700

## Nguon loi

Nguoi dung gui anh chup man hinh loi tren dien thoai Android khi chay app bang Expo/Expo Go/development build.

## Noi dung loi trong anh

- `There was a problem loading the project.`
- `This development build encountered the following error.`
- `com.facebook.react.common.DebugServerException`
- Development server tra `404`.
- URL dang request: `http://192.168.0.108:8081/_expo/loading.bundle?...platform=android...app=com.cashtrack.app...`
- Body co `type: UnableToResolveError`, `targetModuleName: ./_expo/loading`.

## Log repo lien quan

`metro.log` hien tai:

```text
Starting project at /home/benhv1en/Documents/an_ninh_thong_tin
› Port 8081 is running this app in another window
  /home/benhv1en/Documents/an_ninh_thong_tin (pid 35002)
Input is required, but 'npx expo' is in non-interactive mode.
Required input:
> Use port 8082 instead?
› Skipping dev server
```

## Chan doan ngan

Loi kha nang cao la Metro dev server khong chay dung cho project hien tai tren port `8081`, hoac port `8081` dang bi process cu/sai instance chiem. Development build tren dien thoai request bundle tu `192.168.0.108:8081` va nhan `404` cho `/_expo/loading.bundle`.

Day khong phai loi backend API vi backend dung port `5055`, con loi nay xay ra khi native app tai JS bundle tu Metro port `8081`.

## Huong xu ly de xuat

1. Dung process Metro/Expo cu dang chiem port `8081`.
2. Chay lai Metro dung che do development build: `npx expo start --dev-client --lan --clear`.
3. Neu LAN khong on dinh, thu: `npx expo start --dev-client --tunnel --clear`.
4. Neu dung dien thoai that de goi backend local, chay backend voi `--urls http://0.0.0.0:5055` va set `EXPO_PUBLIC_API_BASE_URL=http://<LAN-IP>:5055`.
