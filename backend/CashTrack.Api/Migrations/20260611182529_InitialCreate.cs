using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CashTrack.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BankApps",
                columns: table => new
                {
                    Code = table.Column<string>(type: "TEXT", maxLength: 16, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    ShortName = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                    PackageName = table.Column<string>(type: "TEXT", maxLength: 160, nullable: false),
                    Color = table.Column<string>(type: "TEXT", maxLength: 16, nullable: false),
                    Logo = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BankApps", x => x.Code);
                    table.UniqueConstraint("AK_BankApps_PackageName", x => x.PackageName);
                });

            migrationBuilder.CreateTable(
                name: "TransactionCategories",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    Label = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                    LabelVi = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                    Icon = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                    Color = table.Column<string>(type: "TEXT", maxLength: 16, nullable: false),
                    GradientStart = table.Column<string>(type: "TEXT", maxLength: 16, nullable: false),
                    GradientEnd = table.Column<string>(type: "TEXT", maxLength: 16, nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransactionCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    FullName = table.Column<string>(type: "TEXT", maxLength: 160, nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 320, nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    PasswordSalt = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    DataKeySalt = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    EncryptedDataKey = table.Column<string>(type: "TEXT", nullable: false),
                    PasswordAlgorithm = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    BcryptRounds = table.Column<int>(type: "INTEGER", nullable: false),
                    DataKeyAlgorithm = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    KeyDerivation = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    KeyDerivationIterations = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CategoryBudgets",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    UserId = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    CategoryId = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    LimitMinorUnits = table.Column<long>(type: "INTEGER", nullable: false),
                    Period = table.Column<string>(type: "TEXT", maxLength: 16, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoryBudgets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CategoryBudgets_TransactionCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "TransactionCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CategoryBudgets_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ClientStorageEnvelopes",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    UserId = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    StorageKey = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    Version = table.Column<int>(type: "INTEGER", nullable: false),
                    Algorithm = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    KeyFingerprint = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true),
                    Payload = table.Column<string>(type: "TEXT", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientStorageEnvelopes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClientStorageEnvelopes_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ImportBatches",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    UserId = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    FileName = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    FileType = table.Column<string>(type: "TEXT", maxLength: 16, nullable: false),
                    StartedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CompletedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Status = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    TotalRows = table.Column<int>(type: "INTEGER", nullable: false),
                    ImportedCount = table.Column<int>(type: "INTEGER", nullable: false),
                    UpdatedCount = table.Column<int>(type: "INTEGER", nullable: false),
                    SkippedCount = table.Column<int>(type: "INTEGER", nullable: false),
                    ErrorMessage = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImportBatches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ImportBatches_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserSelectedBankApps",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    PackageName = table.Column<string>(type: "TEXT", maxLength: 160, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSelectedBankApps", x => new { x.UserId, x.PackageName });
                    table.ForeignKey(
                        name: "FK_UserSelectedBankApps_BankApps_PackageName",
                        column: x => x.PackageName,
                        principalTable: "BankApps",
                        principalColumn: "PackageName",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserSelectedBankApps_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserSessions",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    UserId = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 320, nullable: false),
                    AuthenticatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    SecureChannelAlgorithm = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    SessionKeyAlgorithm = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    ServerKeyFingerprint = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    EncryptedSessionKey = table.Column<string>(type: "TEXT", nullable: false),
                    EstablishedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    RevokedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserSessions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserSettings",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Language = table.Column<string>(type: "TEXT", maxLength: 8, nullable: false),
                    Currency = table.Column<string>(type: "TEXT", maxLength: 8, nullable: false),
                    NotificationEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    NotificationPermission = table.Column<string>(type: "TEXT", maxLength: 16, nullable: false),
                    MonthlyBudgetMinorUnits = table.Column<long>(type: "INTEGER", nullable: false),
                    IsCustomBudget = table.Column<bool>(type: "INTEGER", nullable: false),
                    BudgetAlertThreshold = table.Column<int>(type: "INTEGER", nullable: false),
                    ShowCents = table.Column<bool>(type: "INTEGER", nullable: false),
                    CompactNumbers = table.Column<bool>(type: "INTEGER", nullable: false),
                    GeminiApiKeyEncrypted = table.Column<string>(type: "TEXT", nullable: true),
                    UseAICategorization = table.Column<bool>(type: "INTEGER", nullable: false),
                    UseAIReports = table.Column<bool>(type: "INTEGER", nullable: false),
                    WebhooksEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    CurrentFilterPeriod = table.Column<string>(type: "TEXT", maxLength: 16, nullable: false),
                    FilterStartDateUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    FilterEndDateUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    SelectedMonth = table.Column<int>(type: "INTEGER", nullable: true),
                    SelectedYear = table.Column<int>(type: "INTEGER", nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSettings", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_UserSettings_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WebhookEndpoints",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    UserId = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 160, nullable: false),
                    Url = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false),
                    Enabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    SecretEncrypted = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastTriggeredAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    FailCount = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WebhookEndpoints", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WebhookEndpoints_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Transactions",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    ExternalId = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    UserId = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    AmountMinorUnits = table.Column<long>(type: "INTEGER", nullable: false),
                    Type = table.Column<string>(type: "TEXT", maxLength: 16, nullable: false),
                    CategoryId = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Merchant = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    BankCode = table.Column<string>(type: "TEXT", maxLength: 16, nullable: true),
                    BankAccount = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    Source = table.Column<string>(type: "TEXT", maxLength: 16, nullable: false),
                    RawNotification = table.Column<string>(type: "TEXT", nullable: true),
                    RawNotificationHash = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    TransactionDateUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ImportBatchId = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    DeletedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Transactions_BankApps_BankCode",
                        column: x => x.BankCode,
                        principalTable: "BankApps",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Transactions_ImportBatches_ImportBatchId",
                        column: x => x.ImportBatchId,
                        principalTable: "ImportBatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Transactions_TransactionCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "TransactionCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Transactions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WebhookEventSubscriptions",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    WebhookEndpointId = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Event = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WebhookEventSubscriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WebhookEventSubscriptions_WebhookEndpoints_WebhookEndpointId",
                        column: x => x.WebhookEndpointId,
                        principalTable: "WebhookEndpoints",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WebhookHeaders",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    WebhookEndpointId = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    ValueEncrypted = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WebhookHeaders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WebhookHeaders_WebhookEndpoints_WebhookEndpointId",
                        column: x => x.WebhookEndpointId,
                        principalTable: "WebhookEndpoints",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BankNotifications",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    ExternalId = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true),
                    UserId = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    AppPackage = table.Column<string>(type: "TEXT", maxLength: 160, nullable: false),
                    BankCode = table.Column<string>(type: "TEXT", maxLength: 16, nullable: true),
                    Title = table.Column<string>(type: "TEXT", maxLength: 300, nullable: false),
                    Text = table.Column<string>(type: "TEXT", nullable: false),
                    NotificationTimeUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ExtraJson = table.Column<string>(type: "TEXT", nullable: true),
                    Processed = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsBankingNotification = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsAdvertisement = table.Column<bool>(type: "INTEGER", nullable: false),
                    RawTextHash = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    ParsedAmountMinorUnits = table.Column<long>(type: "INTEGER", nullable: true),
                    ParsedType = table.Column<string>(type: "TEXT", maxLength: 16, nullable: true),
                    ParsedMerchant = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    ParsedDescription = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    ParsedTransactionId = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BankNotifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BankNotifications_BankApps_BankCode",
                        column: x => x.BankCode,
                        principalTable: "BankApps",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_BankNotifications_Transactions_ParsedTransactionId",
                        column: x => x.ParsedTransactionId,
                        principalTable: "Transactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_BankNotifications_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ImportRows",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    ImportBatchId = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    RowNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    ExternalTransactionId = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    RawJson = table.Column<string>(type: "TEXT", nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    ErrorMessage = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    TransactionId = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImportRows", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ImportRows_ImportBatches_ImportBatchId",
                        column: x => x.ImportBatchId,
                        principalTable: "ImportBatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ImportRows_Transactions_TransactionId",
                        column: x => x.TransactionId,
                        principalTable: "Transactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "WebhookDeliveryLogs",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    WebhookEndpointId = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Event = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    TransactionId = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    PayloadJson = table.Column<string>(type: "TEXT", nullable: false),
                    Success = table.Column<bool>(type: "INTEGER", nullable: false),
                    StatusCode = table.Column<int>(type: "INTEGER", nullable: true),
                    Error = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    TimestampUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WebhookDeliveryLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WebhookDeliveryLogs_Transactions_TransactionId",
                        column: x => x.TransactionId,
                        principalTable: "Transactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_WebhookDeliveryLogs_WebhookEndpoints_WebhookEndpointId",
                        column: x => x.WebhookEndpointId,
                        principalTable: "WebhookEndpoints",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "BankApps",
                columns: new[] { "Code", "Color", "Logo", "Name", "PackageName", "ShortName" },
                values: new object[,]
                {
                    { "ACB", "#004C6D", null, "Ngan hang TMCP A Chau", "mobile.acb.com.vn", "ACB" },
                    { "BIDV", "#003087", null, "Ngan hang TMCP Dau Tu va Phat Trien Viet Nam", "com.vnpay.bidv", "BIDV" },
                    { "MB", "#0066B3", null, "Ngan hang TMCP Quan Doi", "com.mbmobile", "MB Bank" },
                    { "MOMO", "#A50064", null, "Vi MoMo", "com.mservice.momotransfer", "MoMo" },
                    { "TCB", "#E01E3C", null, "Ngan hang TMCP Ky Thuong Viet Nam", "vn.com.techcombank.bb.app", "Techcombank" },
                    { "TPB", "#6D28D9", null, "Ngan hang TMCP Tien Phong", "com.tpb.mb.gprsandroid", "TPBank" },
                    { "VCB", "#006A4E", null, "Ngan hang TMCP Ngoai Thuong Viet Nam", "com.VCB", "Vietcombank" },
                    { "VNPAY", "#004C6D", null, "VNPay", "vn.com.vnpay.customer", "VNPay" },
                    { "VPB", "#00A859", null, "Ngan hang TMCP Viet Nam Thinh Vuong", "com.vnpay.vpbankonline", "VPBank" },
                    { "VTB", "#0033A0", null, "Ngan hang TMCP Cong Thuong Viet Nam", "com.vietinbank.ipay", "Vietinbank" },
                    { "ZALOPAY", "#0068FF", null, "ZaloPay", "vn.com.vng.zalopay", "ZaloPay" }
                });

            migrationBuilder.InsertData(
                table: "TransactionCategories",
                columns: new[] { "Id", "Color", "GradientEnd", "GradientStart", "Icon", "Label", "LabelVi", "SortOrder" },
                values: new object[,]
                {
                    { "bills", "#ef4444", "#dc2626", "#f87171", "receipt", "Bills & Utilities", "Hoa don", 5 },
                    { "education", "#6366f1", "#4f46e5", "#818cf8", "school", "Education", "Giao duc", 7 },
                    { "entertainment", "#a855f7", "#9333ea", "#c084fc", "movie", "Entertainment", "Giai tri", 4 },
                    { "food", "#f97316", "#ea580c", "#fb923c", "restaurant", "Food & Dining", "An uong", 1 },
                    { "gift", "#f43f5e", "#e11d48", "#fb7185", "card-giftcard", "Gift", "Qua tang", 11 },
                    { "health", "#14b8a6", "#0d9488", "#2dd4bf", "local-hospital", "Health & Medical", "Suc khoe", 6 },
                    { "investment", "#10b981", "#059669", "#34d399", "trending-up", "Investment", "Dau tu", 10 },
                    { "other", "#64748b", "#475569", "#94a3b8", "more-horiz", "Other", "Khac", 12 },
                    { "salary", "#22c55e", "#16a34a", "#4ade80", "account-balance-wallet", "Salary", "Luong", 8 },
                    { "shopping", "#ec4899", "#db2777", "#f472b6", "shopping-bag", "Shopping", "Mua sam", 2 },
                    { "transfer", "#06b6d4", "#0891b2", "#22d3ee", "swap-horiz", "Transfer", "Chuyen khoan", 9 },
                    { "transport", "#3b82f6", "#2563eb", "#60a5fa", "directions-car", "Transportation", "Di chuyen", 3 }
                });

            migrationBuilder.CreateIndex(
                name: "UX_BankApps_PackageName",
                table: "BankApps",
                column: "PackageName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BankNotifications_BankCode",
                table: "BankNotifications",
                column: "BankCode");

            migrationBuilder.CreateIndex(
                name: "IX_BankNotifications_UserId_AppPackage_NotificationTimeUtc",
                table: "BankNotifications",
                columns: new[] { "UserId", "AppPackage", "NotificationTimeUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_BankNotifications_UserId_BankCode",
                table: "BankNotifications",
                columns: new[] { "UserId", "BankCode" });

            migrationBuilder.CreateIndex(
                name: "IX_BankNotifications_UserId_Processed",
                table: "BankNotifications",
                columns: new[] { "UserId", "Processed" });

            migrationBuilder.CreateIndex(
                name: "UX_BankNotifications_ParsedTransactionId",
                table: "BankNotifications",
                column: "ParsedTransactionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_BankNotifications_UserId_ExternalId",
                table: "BankNotifications",
                columns: new[] { "UserId", "ExternalId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_BankNotifications_UserId_RawTextHash",
                table: "BankNotifications",
                columns: new[] { "UserId", "RawTextHash" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CategoryBudgets_CategoryId",
                table: "CategoryBudgets",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "UX_CategoryBudgets_UserId_CategoryId",
                table: "CategoryBudgets",
                columns: new[] { "UserId", "CategoryId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClientStorageEnvelopes_UpdatedAtUtc",
                table: "ClientStorageEnvelopes",
                column: "UpdatedAtUtc");

            migrationBuilder.CreateIndex(
                name: "UX_ClientStorageEnvelopes_UserId_StorageKey",
                table: "ClientStorageEnvelopes",
                columns: new[] { "UserId", "StorageKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ImportBatches_Status",
                table: "ImportBatches",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ImportBatches_UserId_StartedAtUtc",
                table: "ImportBatches",
                columns: new[] { "UserId", "StartedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_ImportRows_ExternalTransactionId",
                table: "ImportRows",
                column: "ExternalTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_ImportRows_TransactionId",
                table: "ImportRows",
                column: "TransactionId");

            migrationBuilder.CreateIndex(
                name: "UX_ImportRows_ImportBatchId_RowNumber",
                table: "ImportRows",
                columns: new[] { "ImportBatchId", "RowNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TransactionCategories_SortOrder",
                table: "TransactionCategories",
                column: "SortOrder");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_BankCode",
                table: "Transactions",
                column: "BankCode");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_CategoryId",
                table: "Transactions",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_DeletedAtUtc",
                table: "Transactions",
                column: "DeletedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_ImportBatchId",
                table: "Transactions",
                column: "ImportBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_IsDeleted",
                table: "Transactions",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_UserId_BankCode",
                table: "Transactions",
                columns: new[] { "UserId", "BankCode" });

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_UserId_CategoryId_TransactionDateUtc",
                table: "Transactions",
                columns: new[] { "UserId", "CategoryId", "TransactionDateUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_UserId_Source",
                table: "Transactions",
                columns: new[] { "UserId", "Source" });

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_UserId_TransactionDateUtc",
                table: "Transactions",
                columns: new[] { "UserId", "TransactionDateUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_UserId_Type_TransactionDateUtc",
                table: "Transactions",
                columns: new[] { "UserId", "Type", "TransactionDateUtc" });

            migrationBuilder.CreateIndex(
                name: "UX_Transactions_UserId_ExternalId",
                table: "Transactions",
                columns: new[] { "UserId", "ExternalId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_Transactions_UserId_RawNotificationHash",
                table: "Transactions",
                columns: new[] { "UserId", "RawNotificationHash" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserSelectedBankApps_PackageName",
                table: "UserSelectedBankApps",
                column: "PackageName");

            migrationBuilder.CreateIndex(
                name: "IX_UserSessions_RevokedAtUtc",
                table: "UserSessions",
                column: "RevokedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_UserSessions_UserId_EstablishedAtUtc",
                table: "UserSessions",
                columns: new[] { "UserId", "EstablishedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_UserSettings_NotificationEnabled",
                table: "UserSettings",
                column: "NotificationEnabled");

            migrationBuilder.CreateIndex(
                name: "IX_WebhookDeliveryLogs_Success_TimestampUtc",
                table: "WebhookDeliveryLogs",
                columns: new[] { "Success", "TimestampUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_WebhookDeliveryLogs_TransactionId",
                table: "WebhookDeliveryLogs",
                column: "TransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_WebhookDeliveryLogs_WebhookEndpointId_TimestampUtc",
                table: "WebhookDeliveryLogs",
                columns: new[] { "WebhookEndpointId", "TimestampUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_WebhookEndpoints_UserId_Enabled",
                table: "WebhookEndpoints",
                columns: new[] { "UserId", "Enabled" });

            migrationBuilder.CreateIndex(
                name: "IX_WebhookEndpoints_UserId_FailCount",
                table: "WebhookEndpoints",
                columns: new[] { "UserId", "FailCount" });

            migrationBuilder.CreateIndex(
                name: "UX_WebhookEventSubscriptions_WebhookEndpointId_Event",
                table: "WebhookEventSubscriptions",
                columns: new[] { "WebhookEndpointId", "Event" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_WebhookHeaders_WebhookEndpointId_Name",
                table: "WebhookHeaders",
                columns: new[] { "WebhookEndpointId", "Name" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BankNotifications");

            migrationBuilder.DropTable(
                name: "CategoryBudgets");

            migrationBuilder.DropTable(
                name: "ClientStorageEnvelopes");

            migrationBuilder.DropTable(
                name: "ImportRows");

            migrationBuilder.DropTable(
                name: "UserSelectedBankApps");

            migrationBuilder.DropTable(
                name: "UserSessions");

            migrationBuilder.DropTable(
                name: "UserSettings");

            migrationBuilder.DropTable(
                name: "WebhookDeliveryLogs");

            migrationBuilder.DropTable(
                name: "WebhookEventSubscriptions");

            migrationBuilder.DropTable(
                name: "WebhookHeaders");

            migrationBuilder.DropTable(
                name: "Transactions");

            migrationBuilder.DropTable(
                name: "WebhookEndpoints");

            migrationBuilder.DropTable(
                name: "BankApps");

            migrationBuilder.DropTable(
                name: "ImportBatches");

            migrationBuilder.DropTable(
                name: "TransactionCategories");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
