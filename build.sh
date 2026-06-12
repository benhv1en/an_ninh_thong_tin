#!/usr/bin/env bash
set -euo pipefail

# Harness engineering docs are stored in harness-engineering/.
# Root markdown harness files intentionally kept: AGENTS.md and CHANGELOG.md.

# Cai dependency neu chua co hoac sau khi package-lock thay doi
npm install

# Kiem tra TypeScript frontend strict mode
npx tsc --noEmit

# Cach chay frontend React Native/Expo
# Expo Go thuan sau fix nay khong con dung ExpoCryptoAES/native AES cua expo-crypto.
# NotificationListener/native background behavior van khong day du trong Expo Go thuan.
# npm run start
# npx expo start --lan --clear
# npx expo start --tunnel --clear

# Project nay co expo-dev-client va native NotificationListener neu can tinh nang native day du.
# Khi chay tren development build/dev client cua CashTrack, dung:
# npx expo start --dev-client --lan --clear
# Neu dien thoai khong truy cap duoc LAN/port 8081, dung tunnel:
# npx expo start --dev-client --tunnel --clear

# Neu gap loi 404 voi /_expo/loading.bundle:
# 1. Kiem tra process dang chiem 8081: lsof -i :8081
# 2. Dung Metro/Expo process cu neu no sai project.
# 3. Chay lai: npx expo start --lan --clear

# Android emulator tro ve host machine qua 10.0.2.2:
# EXPO_PUBLIC_API_BASE_URL=http://10.0.2.2:5055 npm run android
# iOS simulator/web co the dung localhost:
# EXPO_PUBLIC_API_BASE_URL=http://localhost:5055 npm run ios
# EXPO_PUBLIC_API_BASE_URL=http://localhost:5055 npm run web
# May that phai dung LAN IP cua may dang chay backend:
# EXPO_PUBLIC_API_BASE_URL=http://<LAN-IP>:5055 npx expo start --lan --clear
# Build APK production bang EAS:
# npm run build:android

# Cach chay backend C# ASP.NET Core + EF Core SQLite
dotnet build backend/CashTrack.Api/CashTrack.Api.csproj

dotnet ef database update --project backend/CashTrack.Api/CashTrack.Api.csproj --startup-project backend/CashTrack.Api/CashTrack.Api.csproj

dotnet ef migrations list --no-build --project backend/CashTrack.Api/CashTrack.Api.csproj --startup-project backend/CashTrack.Api/CashTrack.Api.csproj

# Chay backend API local tren may dev
# Swagger UI: http://127.0.0.1:5055/swagger
# Health: http://127.0.0.1:5055/health
# Transactions: http://127.0.0.1:5055/api/v1/transactions
dotnet run --project backend/CashTrack.Api/CashTrack.Api.csproj --urls http://127.0.0.1:5055

# Neu dien thoai that can goi backend qua LAN, chay backend bind tat ca interface:
# dotnet run --project backend/CashTrack.Api/CashTrack.Api.csproj --urls http://0.0.0.0:5055
