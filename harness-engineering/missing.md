# Missing backend EF Core setup

## Ket luan hien tai

Khong thieu package/tool bat buoc cho EF Core SQLite va design-time check.

Backend hien da co:

- `Microsoft.EntityFrameworkCore.Sqlite` version `8.0.0`
- `Microsoft.EntityFrameworkCore.Design` version `8.0.0`
- `Microsoft.EntityFrameworkCore.Tools` version `8.0.0`
- Global CLI tool `dotnet-ef` version `8.0.0`
- SQLite connection string `DefaultConnection`
- `AppDbContext` dang ky trong DI bang `AddDbContext<AppDbContext>(...).UseSqlite(...)`
- `dotnet ef dbcontext info` tao duoc `CashTrack.Api.Data.AppDbContext` luc design-time

## Da kiem tra

### Package references

Lenh da chay:

```bash
dotnet list backend/CashTrack.Api/CashTrack.Api.csproj package
```

Ket qua:

```text
Microsoft.EntityFrameworkCore.Design   8.0.0
Microsoft.EntityFrameworkCore.Sqlite   8.0.0
Microsoft.EntityFrameworkCore.Tools    8.0.0
```

### EF CLI tool

Lenh da chay:

```bash
dotnet tool list -g
```

Ket qua:

```text
dotnet-ef  8.0.0  dotnet-ef
```

### Build check

Lenh da chay:

```bash
dotnet build backend/CashTrack.Api/CashTrack.Api.csproj --no-restore
```

Ket qua:

```text
Build succeeded.
0 Warning(s)
0 Error(s)
```

### Design-time DbContext check

Lenh da chay:

```bash
dotnet ef dbcontext info --project backend/CashTrack.Api/CashTrack.Api.csproj --startup-project backend/CashTrack.Api/CashTrack.Api.csproj
```

Ket qua:

```text
Build started...
Build succeeded.
Type: CashTrack.Api.Data.AppDbContext
Provider name: Microsoft.EntityFrameworkCore.Sqlite
Database name: main
Data source: cashtrack.db
Options: None
```

## Connection string SQLite

File:

```text
backend/CashTrack.Api/appsettings.json
```

Gia tri hien tai:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=cashtrack.db"
  }
}
```

Trang thai: hop le cho SQLite.

Ghi chu: `cashtrack.db` la relative path theo working directory khi app chay. Neu muon de database trong folder rieng, co the doi sau thanh `Data Source=Data/cashtrack.db` va dam bao thu muc `Data/` ton tai. Khong bat buoc o buoc nay.

## DI registration

File:

```text
backend/CashTrack.Api/Program.cs
```

Trang thai: da dang ky dung:

```csharp
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(
        builder.Configuration.GetConnectionString("DefaultConnection")
        ?? "Data Source=cashtrack.db"));
```

## Con thieu gi?

### Bat buoc

Khong co muc bat buoc nao dang thieu.

### Nen can nhac them

1. Chua co `IDesignTimeDbContextFactory<AppDbContext>`
   - Khong phai blocker vi `dotnet ef dbcontext info` da tao duoc DbContext qua startup project.
   - Nen them neu muon design-time DbContext on dinh hon, nhat la khi `Program.cs` sau nay co logic startup phuc tap, doc secret, goi service ngoai, hoac phu thuoc environment runtime.

## Lenh dotnet de chay neu muon kiem tra lai

```bash
dotnet restore backend/CashTrack.Api/CashTrack.Api.csproj
dotnet build backend/CashTrack.Api/CashTrack.Api.csproj
dotnet ef dbcontext info --project backend/CashTrack.Api/CashTrack.Api.csproj --startup-project backend/CashTrack.Api/CashTrack.Api.csproj
```

## Lenh dotnet khong can chay lai luc nay

Vi package/tool da co du, hien khong can chay:

```bash
dotnet add backend/CashTrack.Api/CashTrack.Api.csproj package Microsoft.EntityFrameworkCore.Sqlite --version 8.0.0
dotnet add backend/CashTrack.Api/CashTrack.Api.csproj package Microsoft.EntityFrameworkCore.Design --version 8.0.0
dotnet add backend/CashTrack.Api/CashTrack.Api.csproj package Microsoft.EntityFrameworkCore.Tools --version 8.0.0
dotnet tool install --global dotnet-ef --version 8.0.0
```

Neu can sua/cap nhat version sau nay, dung:

```bash
dotnet tool update --global dotnet-ef --version 8.0.0
```

## Khong chay trong buoc nay

- Khong chay `dotnet ef migrations add ...`
- Khong chay `dotnet ef database update`
