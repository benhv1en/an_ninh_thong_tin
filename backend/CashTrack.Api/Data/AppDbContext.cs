using CashTrack.Api.Domain;
using CashTrack.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CashTrack.Api.Data;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<UserAccount> Users => Set<UserAccount>();
    public DbSet<UserSession> UserSessions => Set<UserSession>();
    public DbSet<TransactionCategory> TransactionCategories => Set<TransactionCategory>();
    public DbSet<BankApp> BankApps => Set<BankApp>();
    public DbSet<UserSettings> UserSettings => Set<UserSettings>();
    public DbSet<UserSelectedBankApp> UserSelectedBankApps => Set<UserSelectedBankApp>();
    public DbSet<CategoryBudget> CategoryBudgets => Set<CategoryBudget>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<BankNotification> BankNotifications => Set<BankNotification>();
    public DbSet<WebhookEndpoint> WebhookEndpoints => Set<WebhookEndpoint>();
    public DbSet<WebhookEventSubscription> WebhookEventSubscriptions => Set<WebhookEventSubscription>();
    public DbSet<WebhookHeader> WebhookHeaders => Set<WebhookHeader>();
    public DbSet<WebhookDeliveryLog> WebhookDeliveryLogs => Set<WebhookDeliveryLog>();
    public DbSet<ImportBatch> ImportBatches => Set<ImportBatch>();
    public DbSet<ImportRow> ImportRows => Set<ImportRow>();
    public DbSet<ClientStorageEnvelope> ClientStorageEnvelopes => Set<ClientStorageEnvelope>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ConfigureUsers(modelBuilder);
        ConfigureSessions(modelBuilder);
        ConfigureReferenceData(modelBuilder);
        ConfigureSettings(modelBuilder);
        ConfigureTransactions(modelBuilder);
        ConfigureNotifications(modelBuilder);
        ConfigureWebhooks(modelBuilder);
        ConfigureImports(modelBuilder);
        ConfigureClientStorage(modelBuilder);
        SeedReferenceData(modelBuilder);
    }

    private static void ConfigureUsers(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserAccount>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id).HasMaxLength(64);
            entity.Property(x => x.FullName).HasMaxLength(160).IsRequired();
            entity.Property(x => x.Email).HasMaxLength(320).IsRequired();
            entity.Property(x => x.PasswordHash).HasMaxLength(256).IsRequired();
            entity.Property(x => x.PasswordSalt).HasMaxLength(128).IsRequired();
            entity.Property(x => x.DataKeySalt).HasMaxLength(128).IsRequired();
            entity.Property(x => x.EncryptedDataKey).IsRequired();
            entity.Property(x => x.PasswordAlgorithm).HasMaxLength(32).IsRequired();
            entity.Property(x => x.DataKeyAlgorithm).HasMaxLength(32).IsRequired();
            entity.Property(x => x.KeyDerivation).HasMaxLength(32).IsRequired();

            entity.HasIndex(x => x.Email).IsUnique().HasDatabaseName("UX_Users_Email");
        });
    }

    private static void ConfigureSessions(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserSession>(entity =>
        {
            entity.ToTable("UserSessions");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id).HasMaxLength(64);
            entity.Property(x => x.UserId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Email).HasMaxLength(320).IsRequired();
            entity.Property(x => x.SecureChannelAlgorithm).HasMaxLength(64).IsRequired();
            entity.Property(x => x.SessionKeyAlgorithm).HasMaxLength(32).IsRequired();
            entity.Property(x => x.ServerKeyFingerprint).HasMaxLength(128).IsRequired();
            entity.Property(x => x.EncryptedSessionKey).IsRequired();

            entity.HasOne(x => x.User)
                .WithMany(x => x.Sessions)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(x => new { x.UserId, x.EstablishedAtUtc }).HasDatabaseName("IX_UserSessions_UserId_EstablishedAtUtc");
            entity.HasIndex(x => x.RevokedAtUtc).HasDatabaseName("IX_UserSessions_RevokedAtUtc");
        });
    }

    private static void ConfigureReferenceData(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TransactionCategory>(entity =>
        {
            entity.ToTable("TransactionCategories");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id).HasMaxLength(32);
            entity.Property(x => x.Label).HasMaxLength(80).IsRequired();
            entity.Property(x => x.LabelVi).HasMaxLength(80).IsRequired();
            entity.Property(x => x.Icon).HasMaxLength(80).IsRequired();
            entity.Property(x => x.Color).HasMaxLength(16).IsRequired();
            entity.Property(x => x.GradientStart).HasMaxLength(16).IsRequired();
            entity.Property(x => x.GradientEnd).HasMaxLength(16).IsRequired();
            entity.HasIndex(x => x.SortOrder).HasDatabaseName("IX_TransactionCategories_SortOrder");
        });

        modelBuilder.Entity<BankApp>(entity =>
        {
            entity.ToTable("BankApps");
            entity.HasKey(x => x.Code);

            entity.Property(x => x.Code).HasMaxLength(16);
            entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
            entity.Property(x => x.ShortName).HasMaxLength(80).IsRequired();
            entity.Property(x => x.PackageName).HasMaxLength(160).IsRequired();
            entity.Property(x => x.Color).HasMaxLength(16).IsRequired();
            entity.Property(x => x.Logo).HasMaxLength(256);

            entity.HasAlternateKey(x => x.PackageName).HasName("AK_BankApps_PackageName");
            entity.HasIndex(x => x.PackageName).IsUnique().HasDatabaseName("UX_BankApps_PackageName");
        });
    }

    private static void ConfigureSettings(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserSettings>(entity =>
        {
            entity.ToTable("UserSettings");
            entity.HasKey(x => x.UserId);

            entity.Property(x => x.UserId).HasMaxLength(64);
            entity.Property(x => x.Language).HasMaxLength(8).IsRequired();
            entity.Property(x => x.Currency).HasMaxLength(8).IsRequired();
            entity.Property(x => x.NotificationPermission).HasConversion<string>().HasMaxLength(16);
            entity.Property(x => x.CurrentFilterPeriod).HasConversion<string>().HasMaxLength(16);
            entity.Property(x => x.GeminiApiKeyEncrypted);

            entity.HasOne(x => x.User)
                .WithOne(x => x.Settings)
                .HasForeignKey<UserSettings>(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(x => x.NotificationEnabled).HasDatabaseName("IX_UserSettings_NotificationEnabled");
        });

        modelBuilder.Entity<UserSelectedBankApp>(entity =>
        {
            entity.ToTable("UserSelectedBankApps");
            entity.HasKey(x => new { x.UserId, x.PackageName });

            entity.Property(x => x.UserId).HasMaxLength(64);
            entity.Property(x => x.PackageName).HasMaxLength(160);

            entity.HasOne(x => x.User)
                .WithMany(x => x.SelectedBankApps)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.BankApp)
                .WithMany(x => x.SelectedByUsers)
                .HasForeignKey(x => x.PackageName)
                .HasPrincipalKey(x => x.PackageName)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CategoryBudget>(entity =>
        {
            entity.ToTable("CategoryBudgets");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id).HasMaxLength(64);
            entity.Property(x => x.UserId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.CategoryId).HasMaxLength(32).IsRequired();
            entity.Property(x => x.Period).HasConversion<string>().HasMaxLength(16);

            entity.HasOne(x => x.User)
                .WithMany(x => x.CategoryBudgets)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.Category)
                .WithMany(x => x.CategoryBudgets)
                .HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => new { x.UserId, x.CategoryId })
                .IsUnique()
                .HasDatabaseName("UX_CategoryBudgets_UserId_CategoryId");
        });
    }

    private static void ConfigureTransactions(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.ToTable("Transactions");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id).HasMaxLength(64);
            entity.Property(x => x.ExternalId).HasMaxLength(64);
            entity.Property(x => x.UserId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Type).HasConversion<string>().HasMaxLength(16);
            entity.Property(x => x.CategoryId).HasMaxLength(32).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(500).IsRequired();
            entity.Property(x => x.Merchant).HasMaxLength(200);
            entity.Property(x => x.BankCode).HasMaxLength(16);
            entity.Property(x => x.BankAccount).HasMaxLength(64);
            entity.Property(x => x.Source).HasConversion<string>().HasMaxLength(16);
            entity.Property(x => x.RawNotification);
            entity.Property(x => x.RawNotificationHash).HasMaxLength(64);
            entity.Property(x => x.ImportBatchId).HasMaxLength(64);

            entity.HasOne(x => x.User)
                .WithMany(x => x.Transactions)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.Category)
                .WithMany(x => x.Transactions)
                .HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.BankApp)
                .WithMany(x => x.Transactions)
                .HasForeignKey(x => x.BankCode)
                .HasPrincipalKey(x => x.Code)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(x => x.ImportBatch)
                .WithMany(x => x.Transactions)
                .HasForeignKey(x => x.ImportBatchId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(x => new { x.UserId, x.TransactionDateUtc }).HasDatabaseName("IX_Transactions_UserId_TransactionDateUtc");
            entity.HasIndex(x => new { x.UserId, x.CategoryId, x.TransactionDateUtc }).HasDatabaseName("IX_Transactions_UserId_CategoryId_TransactionDateUtc");
            entity.HasIndex(x => new { x.UserId, x.Type, x.TransactionDateUtc }).HasDatabaseName("IX_Transactions_UserId_Type_TransactionDateUtc");
            entity.HasIndex(x => new { x.UserId, x.BankCode }).HasDatabaseName("IX_Transactions_UserId_BankCode");
            entity.HasIndex(x => new { x.UserId, x.Source }).HasDatabaseName("IX_Transactions_UserId_Source");
            entity.HasIndex(x => new { x.UserId, x.ExternalId }).IsUnique().HasDatabaseName("UX_Transactions_UserId_ExternalId");
            entity.HasIndex(x => new { x.UserId, x.RawNotificationHash }).IsUnique().HasDatabaseName("UX_Transactions_UserId_RawNotificationHash");
            entity.HasIndex(x => x.IsDeleted).HasDatabaseName("IX_Transactions_IsDeleted");
            entity.HasIndex(x => x.DeletedAtUtc).HasDatabaseName("IX_Transactions_DeletedAtUtc");
        });
    }

    private static void ConfigureNotifications(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BankNotification>(entity =>
        {
            entity.ToTable("BankNotifications");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id).HasMaxLength(128);
            entity.Property(x => x.ExternalId).HasMaxLength(128);
            entity.Property(x => x.UserId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.AppPackage).HasMaxLength(160).IsRequired();
            entity.Property(x => x.BankCode).HasMaxLength(16);
            entity.Property(x => x.Title).HasMaxLength(300).IsRequired();
            entity.Property(x => x.Text).IsRequired();
            entity.Property(x => x.ExtraJson);
            entity.Property(x => x.RawTextHash).HasMaxLength(64).IsRequired();
            entity.Property(x => x.ParsedType).HasConversion<string>().HasMaxLength(16);
            entity.Property(x => x.ParsedMerchant).HasMaxLength(200);
            entity.Property(x => x.ParsedDescription).HasMaxLength(500);
            entity.Property(x => x.ParsedTransactionId).HasMaxLength(64);

            entity.HasOne(x => x.User)
                .WithMany(x => x.BankNotifications)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.BankApp)
                .WithMany(x => x.Notifications)
                .HasForeignKey(x => x.BankCode)
                .HasPrincipalKey(x => x.Code)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(x => x.ParsedTransaction)
                .WithOne(x => x.SourceNotification)
                .HasForeignKey<BankNotification>(x => x.ParsedTransactionId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(x => new { x.UserId, x.RawTextHash }).IsUnique().HasDatabaseName("UX_BankNotifications_UserId_RawTextHash");
            entity.HasIndex(x => new { x.UserId, x.ExternalId }).IsUnique().HasDatabaseName("UX_BankNotifications_UserId_ExternalId");
            entity.HasIndex(x => new { x.UserId, x.Processed }).HasDatabaseName("IX_BankNotifications_UserId_Processed");
            entity.HasIndex(x => new { x.UserId, x.AppPackage, x.NotificationTimeUtc }).HasDatabaseName("IX_BankNotifications_UserId_AppPackage_NotificationTimeUtc");
            entity.HasIndex(x => new { x.UserId, x.BankCode }).HasDatabaseName("IX_BankNotifications_UserId_BankCode");
            entity.HasIndex(x => x.ParsedTransactionId).IsUnique().HasDatabaseName("UX_BankNotifications_ParsedTransactionId");
        });
    }

    private static void ConfigureWebhooks(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<WebhookEndpoint>(entity =>
        {
            entity.ToTable("WebhookEndpoints");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id).HasMaxLength(64);
            entity.Property(x => x.UserId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Name).HasMaxLength(160).IsRequired();
            entity.Property(x => x.Url).HasMaxLength(2048).IsRequired();
            entity.Property(x => x.SecretEncrypted);

            entity.HasOne(x => x.User)
                .WithMany(x => x.WebhookEndpoints)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(x => new { x.UserId, x.Enabled }).HasDatabaseName("IX_WebhookEndpoints_UserId_Enabled");
            entity.HasIndex(x => new { x.UserId, x.FailCount }).HasDatabaseName("IX_WebhookEndpoints_UserId_FailCount");
        });

        modelBuilder.Entity<WebhookEventSubscription>(entity =>
        {
            entity.ToTable("WebhookEventSubscriptions");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id).HasMaxLength(64);
            entity.Property(x => x.WebhookEndpointId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Event).HasMaxLength(64).IsRequired();

            entity.HasOne(x => x.WebhookEndpoint)
                .WithMany(x => x.EventSubscriptions)
                .HasForeignKey(x => x.WebhookEndpointId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(x => new { x.WebhookEndpointId, x.Event })
                .IsUnique()
                .HasDatabaseName("UX_WebhookEventSubscriptions_WebhookEndpointId_Event");
        });

        modelBuilder.Entity<WebhookHeader>(entity =>
        {
            entity.ToTable("WebhookHeaders");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id).HasMaxLength(64);
            entity.Property(x => x.WebhookEndpointId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Name).HasMaxLength(128).IsRequired();
            entity.Property(x => x.ValueEncrypted).IsRequired();

            entity.HasOne(x => x.WebhookEndpoint)
                .WithMany(x => x.Headers)
                .HasForeignKey(x => x.WebhookEndpointId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(x => new { x.WebhookEndpointId, x.Name })
                .IsUnique()
                .HasDatabaseName("UX_WebhookHeaders_WebhookEndpointId_Name");
        });

        modelBuilder.Entity<WebhookDeliveryLog>(entity =>
        {
            entity.ToTable("WebhookDeliveryLogs");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id).HasMaxLength(64);
            entity.Property(x => x.WebhookEndpointId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Event).HasMaxLength(64).IsRequired();
            entity.Property(x => x.TransactionId).HasMaxLength(64);
            entity.Property(x => x.PayloadJson).IsRequired();
            entity.Property(x => x.Error).HasMaxLength(1000);

            entity.HasOne(x => x.WebhookEndpoint)
                .WithMany(x => x.DeliveryLogs)
                .HasForeignKey(x => x.WebhookEndpointId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.Transaction)
                .WithMany(x => x.WebhookDeliveryLogs)
                .HasForeignKey(x => x.TransactionId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(x => new { x.WebhookEndpointId, x.TimestampUtc }).HasDatabaseName("IX_WebhookDeliveryLogs_WebhookEndpointId_TimestampUtc");
            entity.HasIndex(x => new { x.Success, x.TimestampUtc }).HasDatabaseName("IX_WebhookDeliveryLogs_Success_TimestampUtc");
            entity.HasIndex(x => x.TransactionId).HasDatabaseName("IX_WebhookDeliveryLogs_TransactionId");
        });
    }

    private static void ConfigureImports(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ImportBatch>(entity =>
        {
            entity.ToTable("ImportBatches");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id).HasMaxLength(64);
            entity.Property(x => x.UserId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.FileName).HasMaxLength(255).IsRequired();
            entity.Property(x => x.FileType).HasConversion<string>().HasMaxLength(16);
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(32);
            entity.Property(x => x.ErrorMessage).HasMaxLength(2000);

            entity.HasOne(x => x.User)
                .WithMany(x => x.ImportBatches)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(x => new { x.UserId, x.StartedAtUtc }).HasDatabaseName("IX_ImportBatches_UserId_StartedAtUtc");
            entity.HasIndex(x => x.Status).HasDatabaseName("IX_ImportBatches_Status");
        });

        modelBuilder.Entity<ImportRow>(entity =>
        {
            entity.ToTable("ImportRows");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id).HasMaxLength(64);
            entity.Property(x => x.ImportBatchId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.ExternalTransactionId).HasMaxLength(64);
            entity.Property(x => x.RawJson).IsRequired();
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(32);
            entity.Property(x => x.ErrorMessage).HasMaxLength(2000);
            entity.Property(x => x.TransactionId).HasMaxLength(64);

            entity.HasOne(x => x.ImportBatch)
                .WithMany(x => x.Rows)
                .HasForeignKey(x => x.ImportBatchId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.Transaction)
                .WithMany()
                .HasForeignKey(x => x.TransactionId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(x => new { x.ImportBatchId, x.RowNumber })
                .IsUnique()
                .HasDatabaseName("UX_ImportRows_ImportBatchId_RowNumber");
            entity.HasIndex(x => x.ExternalTransactionId).HasDatabaseName("IX_ImportRows_ExternalTransactionId");
            entity.HasIndex(x => x.TransactionId).HasDatabaseName("IX_ImportRows_TransactionId");
        });
    }

    private static void ConfigureClientStorage(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ClientStorageEnvelope>(entity =>
        {
            entity.ToTable("ClientStorageEnvelopes");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id).HasMaxLength(64);
            entity.Property(x => x.UserId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.StorageKey).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Algorithm).HasMaxLength(32).IsRequired();
            entity.Property(x => x.KeyFingerprint).HasMaxLength(128);
            entity.Property(x => x.Payload).IsRequired();

            entity.HasOne(x => x.User)
                .WithMany(x => x.ClientStorageEnvelopes)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(x => new { x.UserId, x.StorageKey })
                .IsUnique()
                .HasDatabaseName("UX_ClientStorageEnvelopes_UserId_StorageKey");
            entity.HasIndex(x => x.UpdatedAtUtc).HasDatabaseName("IX_ClientStorageEnvelopes_UpdatedAtUtc");
        });
    }

    private static void SeedReferenceData(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TransactionCategory>().HasData(
            new TransactionCategory { Id = TransactionCategoryIds.Food, Label = "Food & Dining", LabelVi = "An uong", Icon = "restaurant", Color = "#f97316", GradientStart = "#fb923c", GradientEnd = "#ea580c", SortOrder = 1 },
            new TransactionCategory { Id = TransactionCategoryIds.Shopping, Label = "Shopping", LabelVi = "Mua sam", Icon = "shopping-bag", Color = "#ec4899", GradientStart = "#f472b6", GradientEnd = "#db2777", SortOrder = 2 },
            new TransactionCategory { Id = TransactionCategoryIds.Transport, Label = "Transportation", LabelVi = "Di chuyen", Icon = "directions-car", Color = "#3b82f6", GradientStart = "#60a5fa", GradientEnd = "#2563eb", SortOrder = 3 },
            new TransactionCategory { Id = TransactionCategoryIds.Entertainment, Label = "Entertainment", LabelVi = "Giai tri", Icon = "movie", Color = "#a855f7", GradientStart = "#c084fc", GradientEnd = "#9333ea", SortOrder = 4 },
            new TransactionCategory { Id = TransactionCategoryIds.Bills, Label = "Bills & Utilities", LabelVi = "Hoa don", Icon = "receipt", Color = "#ef4444", GradientStart = "#f87171", GradientEnd = "#dc2626", SortOrder = 5 },
            new TransactionCategory { Id = TransactionCategoryIds.Health, Label = "Health & Medical", LabelVi = "Suc khoe", Icon = "local-hospital", Color = "#14b8a6", GradientStart = "#2dd4bf", GradientEnd = "#0d9488", SortOrder = 6 },
            new TransactionCategory { Id = TransactionCategoryIds.Education, Label = "Education", LabelVi = "Giao duc", Icon = "school", Color = "#6366f1", GradientStart = "#818cf8", GradientEnd = "#4f46e5", SortOrder = 7 },
            new TransactionCategory { Id = TransactionCategoryIds.Salary, Label = "Salary", LabelVi = "Luong", Icon = "account-balance-wallet", Color = "#22c55e", GradientStart = "#4ade80", GradientEnd = "#16a34a", SortOrder = 8 },
            new TransactionCategory { Id = TransactionCategoryIds.Transfer, Label = "Transfer", LabelVi = "Chuyen khoan", Icon = "swap-horiz", Color = "#06b6d4", GradientStart = "#22d3ee", GradientEnd = "#0891b2", SortOrder = 9 },
            new TransactionCategory { Id = TransactionCategoryIds.Investment, Label = "Investment", LabelVi = "Dau tu", Icon = "trending-up", Color = "#10b981", GradientStart = "#34d399", GradientEnd = "#059669", SortOrder = 10 },
            new TransactionCategory { Id = TransactionCategoryIds.Gift, Label = "Gift", LabelVi = "Qua tang", Icon = "card-giftcard", Color = "#f43f5e", GradientStart = "#fb7185", GradientEnd = "#e11d48", SortOrder = 11 },
            new TransactionCategory { Id = TransactionCategoryIds.Other, Label = "Other", LabelVi = "Khac", Icon = "more-horiz", Color = "#64748b", GradientStart = "#94a3b8", GradientEnd = "#475569", SortOrder = 12 });

        modelBuilder.Entity<BankApp>().HasData(
            new BankApp { Code = "VCB", Name = "Ngan hang TMCP Ngoai Thuong Viet Nam", ShortName = "Vietcombank", PackageName = "com.VCB", Color = "#006A4E" },
            new BankApp { Code = "MB", Name = "Ngan hang TMCP Quan Doi", ShortName = "MB Bank", PackageName = "com.mbmobile", Color = "#0066B3" },
            new BankApp { Code = "TCB", Name = "Ngan hang TMCP Ky Thuong Viet Nam", ShortName = "Techcombank", PackageName = "vn.com.techcombank.bb.app", Color = "#E01E3C" },
            new BankApp { Code = "ACB", Name = "Ngan hang TMCP A Chau", ShortName = "ACB", PackageName = "mobile.acb.com.vn", Color = "#004C6D" },
            new BankApp { Code = "VPB", Name = "Ngan hang TMCP Viet Nam Thinh Vuong", ShortName = "VPBank", PackageName = "com.vnpay.vpbankonline", Color = "#00A859" },
            new BankApp { Code = "BIDV", Name = "Ngan hang TMCP Dau Tu va Phat Trien Viet Nam", ShortName = "BIDV", PackageName = "com.vnpay.bidv", Color = "#003087" },
            new BankApp { Code = "VTB", Name = "Ngan hang TMCP Cong Thuong Viet Nam", ShortName = "Vietinbank", PackageName = "com.vietinbank.ipay", Color = "#0033A0" },
            new BankApp { Code = "TPB", Name = "Ngan hang TMCP Tien Phong", ShortName = "TPBank", PackageName = "com.tpb.mb.gprsandroid", Color = "#6D28D9" },
            new BankApp { Code = "MOMO", Name = "Vi MoMo", ShortName = "MoMo", PackageName = "com.mservice.momotransfer", Color = "#A50064" },
            new BankApp { Code = "VNPAY", Name = "VNPay", ShortName = "VNPay", PackageName = "vn.com.vnpay.customer", Color = "#004C6D" },
            new BankApp { Code = "ZALOPAY", Name = "ZaloPay", ShortName = "ZaloPay", PackageName = "vn.com.vng.zalopay", Color = "#0068FF" });
    }
}
