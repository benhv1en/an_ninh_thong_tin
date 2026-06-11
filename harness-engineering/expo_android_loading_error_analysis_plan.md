# Phan tich loi Expo Android loading bundle

Thoi diem ghi nhan: 12/06/2026 02:57:02 +0700

## Tom tat loi

Nguoi dung chay app tren dien thoai Android va thay man hinh:

- `There was a problem loading the project.`
- `DebugServerException`
- Development server tra `404`
- URL dang bi request: `http://192.168.0.108:8081/_expo/loading.bundle?...platform=android...app=com.cashtrack.app...`
- Body co `type: UnableToResolveError`, `targetModuleName: ./_expo/loading`

Day la loi o buoc native app/dev client tai JavaScript bundle tu Metro, xay ra truoc khi code UI/business logic cua app chay. Vi vay loi nay khong phai do backend API `/health`, transaction endpoint, hay `EXPO_PUBLIC_API_BASE_URL`.

## Quan sat tu repo hien tai

- `package.json` co `expo-dev-client` version `~6.0.21`.
- `eas.json` co profile `development` voi `developmentClient: true`.
- `app.json`/native Android dung package `com.cashtrack.app`.
- Repo co thu muc native `android/` va `ios/`.
- Android manifest co permission/native flow cho notification listener: `android.permission.BIND_NOTIFICATION_LISTENER_SERVICE`.
- `src/services/notificationService.ts` cung ghi ro full notification listener can EAS Development Build, khong phai Expo Go thuan.
- `metro.log` ghi:
  - Port `8081` dang bi mot process khac chiem.
  - Expo CLI hoi co dung port `8082` khong.
  - Vi dang non-interactive nen CLI khong nhan duoc input va `Skipping dev server`.

## Nguyen nhan kha nang cao nhat

### 1. Dien thoai dang noi vao Metro server cu/sai tren port 8081

`metro.log` cho thay port `8081` da co process khac chay. Khi lenh Expo moi duoc chay trong moi truong non-interactive, no khong tu chuyen sang `8082`, nen khong start dev server moi cho project hien tai.

Dien thoai van request `192.168.0.108:8081`, nhung server o cong do co the la instance cu, sai project, hoac trang thai Metro khong dung. Ket qua la request `/_expo/loading.bundle` bi tra `404` va dev client hien man hinh loi.

### 2. Dang nham lan giua Expo Go va development build

Anh loi hien `app=com.cashtrack.app`, trong khi Expo Go thuan khong co package name nay. Repo cung co `expo-dev-client`, native Android project va EAS development profile. Do do day gan nhu la custom development build/dev client cua CashTrack, khong phai Expo Go thuan tu Play Store.

Neu muon dung tinh nang notification listener/native permission, khong nen chay bang Expo Go thuan. Can dung development build cua app va Metro server phu hop.

### 3. Ket noi LAN/port chua on dinh

Dien thoai dang goi `192.168.0.108:8081`. Neu laptop va dien thoai khong cung Wi-Fi, router chan client-to-client, firewall chan port `8081`, hoac Metro chay tren port khac, bundle se khong load duoc. Truong hop nay thuong nen dung `--tunnel` de xac minh nhanh.

### 4. Backend API khong phai nguyen nhan truc tiep

Backend chay o port `5055`, con loi trong anh la Metro bundle o port `8081`. API base URL sai co the gay loi network sau khi app da vao UI, nhung khong tao ra `/_expo/loading.bundle 404`.

## Tai lieu doi chieu

- Expo docs ve development build: https://docs.expo.dev/develop/development-builds/introduction/
- Expo docs ve cach dung development build: https://docs.expo.dev/develop/development-builds/use-development-builds/
- Expo docs ve start dev server, cung Wi-Fi va tunnel: https://docs.expo.dev/get-started/start-developing/

## Ke hoach sua loi

### Buoc 1. Lam sach Metro server dang sai cong

Muc tieu: dam bao port `8081` khong bi instance cu/sai project chiem.

Lenh de kiem tra/lam sach du kien:

```bash
lsof -i :8081
kill <PID>
```

Neu khong co `lsof`, dung cong cu tuong duong nhu `ss -ltnp` hoac `fuser 8081/tcp`.

### Buoc 2. Chon dung che do chay

Vi project nay co native code/dev client, luong uu tien la development build:

```bash
npx expo start --dev-client --lan --clear
```

Sau do mo app CashTrack development build tren dien thoai, khong quet bang Expo Go thuan neu can full native notification listener.

Neu van bi loi do mang LAN, thu tunnel de xac minh:

```bash
npx expo start --dev-client --tunnel --clear
```

Neu thuc su muon test bang Expo Go thuan, phai chap nhan rang notification listener/native behavior khong day du. Khi do nen chay:

```bash
npx expo start --lan --clear
```

va quet QR bang Expo Go, nhung khong dung de ket luan tinh nang native notification listener.

### Buoc 3. Xac minh Metro dang phuc vu dung project

Tren may dev, kiem tra:

```bash
curl http://127.0.0.1:8081/status
```

Tren dien thoai hoac tu may khac cung mang, kiem tra URL tuong ung:

```bash
curl http://192.168.0.108:8081/status
```

Ky vong Metro tra status hop le. Neu khong truy cap duoc tu LAN, dung `--tunnel` hoac kiem tra firewall/router.

### Buoc 4. Neu dung development build, rebuild khi native dependency thay doi

Neu app development build tren dien thoai duoc tao truoc khi them/sua native dependency/plugin, can rebuild va cai lai:

```bash
npm run android
```

Hoac tao EAS development build moi:

```bash
eas build --profile development --platform android
```

### Buoc 5. Sau khi bundle load duoc, moi kiem tra API backend

Khi chay tren dien thoai that, backend khong nen chi bind `127.0.0.1`. Nen chay backend tren LAN:

```bash
dotnet run --project backend/CashTrack.Api/CashTrack.Api.csproj --urls http://0.0.0.0:5055
```

Va start Expo voi LAN IP cua may dev:

```bash
EXPO_PUBLIC_API_BASE_URL=http://192.168.0.108:5055 npx expo start --dev-client --lan --clear
```

### Buoc 6. Neu van loi, thu thap log tiep theo

Can thu them:

- Terminal log cua `npx expo start --dev-client --lan --clear`.
- Ket qua `curl http://127.0.0.1:8081/status`.
- Ket qua `curl http://192.168.0.108:8081/status`.
- Anh loi moi sau khi da clear port/cache.

## De xuat sua repo o buoc tiep theo

Neu duoc phep sua file cau hinh, nen them script vao `package.json` de tranh chay sai che do:

```json
"scripts": {
  "start:dev-client": "expo start --dev-client --lan --clear",
  "start:tunnel": "expo start --dev-client --tunnel --clear"
}
```

Neu muon app chay tren dien thoai that va goi backend local, nen cap nhat huong dan chay backend thanh `http://0.0.0.0:5055` va dung `EXPO_PUBLIC_API_BASE_URL=http://<LAN-IP>:5055`.
