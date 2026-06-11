#!/usr/bin/env bash
set -euo pipefail

# Harness engineering docs are stored in harness-engineering/.
# Root markdown harness files intentionally kept: AGENTS.md and CHANGELOG.md.

# Cach chay frontend React Native/Expo
# Cai dependency neu chua co: npm install
# Android emulator tro ve host machine qua 10.0.2.2:
# EXPO_PUBLIC_API_BASE_URL=http://10.0.2.2:5055 npm run android
# iOS simulator/web co the dung localhost:
# EXPO_PUBLIC_API_BASE_URL=http://localhost:5055 npm run ios
# EXPO_PUBLIC_API_BASE_URL=http://localhost:5055 npm run web
# May that phai dung LAN IP cua may dang chay backend:
# EXPO_PUBLIC_API_BASE_URL=http://<LAN-IP>:5055 npm run start
# Chay Metro/Expo khong cai bien moi truong thi apiClient se dung fallback dev cho emulator/simulator:
# npm run start
# Build APK production bang EAS:
# npm run build:android

# Cach chay backend C# ASP.NET Core + EF Core SQLite
dotnet build backend/CashTrack.Api/CashTrack.Api.csproj

dotnet ef database update --project backend/CashTrack.Api/CashTrack.Api.csproj --startup-project backend/CashTrack.Api/CashTrack.Api.csproj

dotnet ef migrations list --no-build --project backend/CashTrack.Api/CashTrack.Api.csproj --startup-project backend/CashTrack.Api/CashTrack.Api.csproj

# Chay backend API local
# Swagger UI: http://127.0.0.1:5055/swagger
# Health: http://127.0.0.1:5055/health
# Transactions: http://127.0.0.1:5055/api/v1/transactions
dotnet run --project backend/CashTrack.Api/CashTrack.Api.csproj --urls http://127.0.0.1:5055
