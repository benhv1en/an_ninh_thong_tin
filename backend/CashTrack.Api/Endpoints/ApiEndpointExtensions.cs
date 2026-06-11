using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using CashTrack.Api.Contracts;
using CashTrack.Api.Data;
using CashTrack.Api.Domain;
using CashTrack.Api.Domain.Entities;
using CashTrack.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace CashTrack.Api.Endpoints;

public static class ApiEndpointExtensions
{
    private const string DefaultUserId = "dev-user";
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private static readonly string[] ValidCategories =
    [
        TransactionCategoryIds.Food,
        TransactionCategoryIds.Shopping,
        TransactionCategoryIds.Transport,
        TransactionCategoryIds.Entertainment,
        TransactionCategoryIds.Bills,
        TransactionCategoryIds.Health,
        TransactionCategoryIds.Education,
        TransactionCategoryIds.Salary,
        TransactionCategoryIds.Transfer,
        TransactionCategoryIds.Investment,
        TransactionCategoryIds.Gift,
        TransactionCategoryIds.Other
    ];

    private static readonly string[] ValidWebhookEvents =
    [
        WebhookEvents.TransactionCreated,
        WebhookEvents.TransactionUpdated,
        WebhookEvents.TransactionDeleted,
        WebhookEvents.BudgetExceeded,
        WebhookEvents.BudgetWarning,
        WebhookEvents.DailySummary,
        WebhookEvents.WeeklySummary,
        WebhookEvents.MonthlySummary,
        WebhookEvents.NotificationReceived
    ];

    public static IEndpointRouteBuilder MapCashTrackApi(this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup("/api/v1");

        api.MapGet("/transactions", GetTransactionsAsync)
            .WithTags("Transactions")
            .WithName("ListTransactions")
            .Produces<TransactionListResponse>()
            .ProducesValidationProblem();

        api.MapGet("/transactions/{id}", GetTransactionAsync)
            .WithTags("Transactions")
            .WithName("GetTransaction")
            .Produces<TransactionDto>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        api.MapPost("/transactions", CreateTransactionAsync)
            .WithTags("Transactions")
            .WithName("CreateTransaction")
            .Produces<TransactionDto>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status409Conflict);

        api.MapPatch("/transactions/{id}", UpdateTransactionAsync)
            .WithTags("Transactions")
            .WithName("UpdateTransaction")
            .Produces<TransactionDto>()
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        api.MapDelete("/transactions/{id}", DeleteTransactionAsync)
            .WithTags("Transactions")
            .WithName("DeleteTransaction")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);

        api.MapGet("/categories", GetCategoriesAsync)
            .WithTags("Metadata")
            .WithName("ListCategories")
            .Produces<IReadOnlyList<CategoryDto>>();

        api.MapGet("/banks", GetBanksAsync)
            .WithTags("Metadata")
            .WithName("ListBanks")
            .Produces<IReadOnlyList<BankInfoDto>>();

        api.MapPost("/notifications/parse", ParseNotificationAsync)
            .WithTags("Notifications")
            .WithName("ParseNotification")
            .Produces<ParseNotificationResponse>()
            .ProducesValidationProblem();

        api.MapGet("/webhooks", GetWebhooksAsync)
            .WithTags("Webhooks")
            .WithName("ListWebhooks")
            .Produces<IReadOnlyList<WebhookConfigDto>>();

        api.MapPost("/webhooks", CreateWebhookAsync)
            .WithTags("Webhooks")
            .WithName("CreateWebhook")
            .Produces<WebhookConfigDto>(StatusCodes.Status201Created)
            .ProducesValidationProblem();

        api.MapPatch("/webhooks/{id}", UpdateWebhookAsync)
            .WithTags("Webhooks")
            .WithName("UpdateWebhook")
            .Produces<WebhookConfigDto>()
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound);

        api.MapDelete("/webhooks/{id}", DeleteWebhookAsync)
            .WithTags("Webhooks")
            .WithName("DeleteWebhook")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);

        api.MapPost("/webhooks/{id}/test", TestWebhookAsync)
            .WithTags("Webhooks")
            .WithName("TestWebhook")
            .Produces<WebhookDeliveryResultDto>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status502BadGateway);

        api.MapPost("/webhooks/send", SendWebhooksAsync)
            .WithTags("Webhooks")
            .WithName("SendWebhooks")
            .Produces<IReadOnlyList<WebhookDeliveryResultDto>>()
            .ProducesValidationProblem();

        api.MapGet("/backup/export", ExportBackupAsync)
            .WithTags("Backup")
            .WithName("ExportBackup")
            .Produces(StatusCodes.Status200OK, contentType: "application/json")
            .ProducesValidationProblem();

        api.MapPost("/backup/import", ImportBackupAsync)
            .WithTags("Backup")
            .WithName("ImportBackup")
            .Produces<ImportResultDto>()
            .ProducesValidationProblem();

        return app;
    }

    private static async Task<IResult> GetTransactionsAsync(
        AppDbContext db,
        string? type,
        string? category,
        DateTime? startDate,
        DateTime? endDate,
        long? minAmount,
        long? maxAmount,
        string? search,
        int? limit,
        string? cursor,
        CancellationToken cancellationToken)
    {
        var errors = new ErrorBag();
        if (!string.IsNullOrWhiteSpace(type) && !IsValidTransactionType(type)) errors.Add("type", "type must be income or expense.");
        if (!string.IsNullOrWhiteSpace(category) && !IsValidCategory(category)) errors.Add("category", "category is not supported.");
        if (minAmount is < 0) errors.Add("minAmount", "minAmount must be greater than or equal to 0.");
        if (maxAmount is < 0) errors.Add("maxAmount", "maxAmount must be greater than or equal to 0.");
        if (minAmount is not null && maxAmount is not null && minAmount > maxAmount) errors.Add("maxAmount", "maxAmount must be greater than or equal to minAmount.");
        var pageSize = limit ?? 50;
        if (pageSize is < 1 or > 100) errors.Add("limit", "limit must be between 1 and 100.");

        DateTime? cursorDate = null;
        if (!string.IsNullOrWhiteSpace(cursor))
        {
            if (DateTimeOffset.TryParse(cursor, out var parsedCursor)) cursorDate = parsedCursor.UtcDateTime;
            else errors.Add("cursor", "cursor must be an ISO UTC date string.");
        }

        if (errors.HasErrors) return Results.ValidationProblem(errors.ToDictionary());

        var query = db.Transactions
            .AsNoTracking()
            .Where(transaction => transaction.UserId == DefaultUserId && !transaction.IsDeleted);

        if (!string.IsNullOrWhiteSpace(type) && Enum.TryParse<TransactionType>(type, out var transactionType)) query = query.Where(transaction => transaction.Type == transactionType);
        if (!string.IsNullOrWhiteSpace(category)) query = query.Where(transaction => transaction.CategoryId == category);
        if (startDate is not null) query = query.Where(transaction => transaction.TransactionDateUtc >= ToUtc(startDate.Value));
        if (endDate is not null) query = query.Where(transaction => transaction.TransactionDateUtc <= ToUtc(endDate.Value));
        if (minAmount is not null) query = query.Where(transaction => transaction.AmountMinorUnits >= minAmount.Value);
        if (maxAmount is not null) query = query.Where(transaction => transaction.AmountMinorUnits <= maxAmount.Value);
        if (cursorDate is not null) query = query.Where(transaction => transaction.CreatedAtUtc < cursorDate.Value);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(transaction => transaction.Description.Contains(term) || (transaction.Merchant != null && transaction.Merchant.Contains(term)) || transaction.CategoryId.Contains(term));
        }

        var rows = await query
            .OrderByDescending(transaction => transaction.CreatedAtUtc)
            .Take(pageSize + 1)
            .Select(transaction => ToTransactionDto(transaction))
            .ToListAsync(cancellationToken);

        var nextCursor = rows.Count > pageSize ? rows[^1].CreatedAt.ToString("O") : null;
        var items = rows.Take(pageSize).ToList();
        return Results.Ok(new TransactionListResponse(items, nextCursor));
    }

    private static async Task<IResult> GetTransactionAsync(AppDbContext db, string id, CancellationToken cancellationToken)
    {
        var transaction = await db.Transactions
            .AsNoTracking()
            .Where(row => row.UserId == DefaultUserId && !row.IsDeleted && row.Id == id)
            .Select(row => ToTransactionDto(row))
            .FirstOrDefaultAsync(cancellationToken);

        return transaction is null
            ? NotFoundProblem("Transaction not found.")
            : Results.Ok(transaction);
    }

    private static async Task<IResult> CreateTransactionAsync(AppDbContext db, CreateTransactionRequest request, CancellationToken cancellationToken)
    {
        await EnsureDefaultUserAsync(db, cancellationToken);
        var errors = await ValidateCreateTransactionAsync(db, request, cancellationToken);
        if (errors.HasErrors) return Results.ValidationProblem(errors.ToDictionary());

        var rawNotificationHash = string.IsNullOrWhiteSpace(request.RawNotification) ? null : Sha256Hex(request.RawNotification.Trim());
        if (await HasTransactionConflictAsync(db, request.ExternalId, rawNotificationHash, null, cancellationToken))
        {
            return ConflictProblem("Transaction with the same externalId or rawNotification already exists.");
        }

        var now = DateTime.UtcNow;
        var transactionDate = request.CreatedAt is null ? now : ToUtc(request.CreatedAt.Value);
        var source = string.IsNullOrWhiteSpace(request.Source) ? TransactionSource.manual : Enum.Parse<TransactionSource>(request.Source.Trim());
        var transaction = new Transaction
        {
            Id = NewId(),
            UserId = DefaultUserId,
            ExternalId = TrimToNull(request.ExternalId),
            AmountMinorUnits = request.Amount!.Value,
            Type = Enum.Parse<TransactionType>(request.Type!.Trim()),
            CategoryId = request.Category!.Trim(),
            Description = TrimToNull(request.Description) ?? request.Category!.Trim(),
            Merchant = TrimToNull(request.Merchant),
            BankAccount = TrimToNull(request.BankAccount),
            BankCode = await ResolveBankCodeAsync(db, request.BankAccount, cancellationToken),
            Source = source,
            RawNotification = TrimToNull(request.RawNotification),
            RawNotificationHash = rawNotificationHash,
            TransactionDateUtc = transactionDate,
            CreatedAtUtc = transactionDate,
            UpdatedAtUtc = now,
            IsDeleted = false
        };

        db.Transactions.Add(transaction);
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            return ConflictProblem("Transaction conflicts with an existing unique key.");
        }

        return Results.Created($"/api/v1/transactions/{transaction.Id}", ToTransactionDto(transaction));
    }

    private static async Task<IResult> UpdateTransactionAsync(AppDbContext db, string id, UpdateTransactionRequest request, CancellationToken cancellationToken)
    {
        var transaction = await db.Transactions.FirstOrDefaultAsync(row => row.UserId == DefaultUserId && !row.IsDeleted && row.Id == id, cancellationToken);
        if (transaction is null) return NotFoundProblem("Transaction not found.");

        var errors = await ValidateUpdateTransactionAsync(db, request, cancellationToken);
        if (errors.HasErrors) return Results.ValidationProblem(errors.ToDictionary());

        if (request.Amount is not null) transaction.AmountMinorUnits = request.Amount.Value;
        if (!string.IsNullOrWhiteSpace(request.Type)) transaction.Type = Enum.Parse<TransactionType>(request.Type.Trim());
        if (!string.IsNullOrWhiteSpace(request.Category)) transaction.CategoryId = request.Category.Trim();
        if (request.Description is not null) transaction.Description = request.Description.Trim();
        if (request.Merchant is not null) transaction.Merchant = TrimToNull(request.Merchant);
        if (request.BankAccount is not null)
        {
            transaction.BankAccount = TrimToNull(request.BankAccount);
            transaction.BankCode = await ResolveBankCodeAsync(db, request.BankAccount, cancellationToken);
        }
        transaction.UpdatedAtUtc = DateTime.UtcNow;

        await db.SaveChangesAsync(cancellationToken);
        return Results.Ok(ToTransactionDto(transaction));
    }

    private static async Task<IResult> DeleteTransactionAsync(AppDbContext db, string id, CancellationToken cancellationToken)
    {
        var transaction = await db.Transactions.FirstOrDefaultAsync(row => row.UserId == DefaultUserId && !row.IsDeleted && row.Id == id, cancellationToken);
        if (transaction is null) return NotFoundProblem("Transaction not found.");

        transaction.IsDeleted = true;
        transaction.DeletedAtUtc = DateTime.UtcNow;
        transaction.UpdatedAtUtc = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
        return Results.NoContent();
    }

    private static async Task<IResult> GetCategoriesAsync(AppDbContext db, CancellationToken cancellationToken)
    {
        var categories = await db.TransactionCategories
            .AsNoTracking()
            .OrderBy(category => category.SortOrder)
            .Select(category => new CategoryDto(
                category.Id,
                category.Label,
                category.LabelVi,
                category.Icon,
                category.Color,
                new[] { category.GradientStart, category.GradientEnd }))
            .ToListAsync(cancellationToken);

        return Results.Ok(categories);
    }

    private static async Task<IResult> GetBanksAsync(AppDbContext db, CancellationToken cancellationToken)
    {
        var banks = await db.BankApps
            .AsNoTracking()
            .OrderBy(bank => bank.Code)
            .Select(bank => new BankInfoDto(bank.Code, bank.Name, bank.ShortName, bank.PackageName, bank.Color, bank.Logo))
            .ToListAsync(cancellationToken);

        return Results.Ok(banks);
    }

    private static async Task<IResult> ParseNotificationAsync(AppDbContext db, NotificationParsingService parser, ParseNotificationRequest request, CancellationToken cancellationToken)
    {
        var errors = new ErrorBag();
        if (request.Notification is null)
        {
            errors.Add("notification", "notification is required.");
            return Results.ValidationProblem(errors.ToDictionary());
        }

        if (string.IsNullOrWhiteSpace(request.Notification.App)) errors.Add("notification.app", "app is required.");
        if (string.IsNullOrWhiteSpace(request.Notification.Title)) errors.Add("notification.title", "title is required.");
        if (string.IsNullOrWhiteSpace(request.Notification.Text)) errors.Add("notification.text", "text is required.");
        if (request.Notification.Time is null) errors.Add("notification.time", "time is required.");
        if (request.SelectedBankApps is { Count: > 0 } && !request.SelectedBankApps.Contains(request.Notification.App)) errors.Add("selectedBankApps", "notification app is not selected.");
        if (errors.HasErrors) return Results.ValidationProblem(errors.ToDictionary());

        var packageToBankCode = await db.BankApps
            .AsNoTracking()
            .ToDictionaryAsync(bank => bank.PackageName, bank => bank.Code, cancellationToken);

        return Results.Ok(parser.Parse(request.Notification, packageToBankCode));
    }

    private static async Task<IResult> GetWebhooksAsync(AppDbContext db, CancellationToken cancellationToken)
    {
        var webhooks = await db.WebhookEndpoints
            .AsNoTracking()
            .Include(webhook => webhook.EventSubscriptions)
            .Include(webhook => webhook.Headers)
            .Where(webhook => webhook.UserId == DefaultUserId)
            .OrderBy(webhook => webhook.CreatedAtUtc)
            .Select(webhook => ToWebhookDto(webhook))
            .ToListAsync(cancellationToken);

        return Results.Ok(webhooks);
    }

    private static async Task<IResult> CreateWebhookAsync(AppDbContext db, CreateWebhookRequest request, CancellationToken cancellationToken)
    {
        await EnsureDefaultUserAsync(db, cancellationToken);
        var errors = ValidateWebhook(request.Name, request.Url, request.Events, request.Headers, requireEvents: true, requireNameAndUrl: true);
        if (errors.HasErrors) return Results.ValidationProblem(errors.ToDictionary());

        var webhook = new WebhookEndpoint
        {
            Id = NewId(),
            UserId = DefaultUserId,
            Name = request.Name!.Trim(),
            Url = request.Url!.Trim(),
            Enabled = request.Enabled ?? true,
            SecretEncrypted = TrimToNull(request.Secret),
            CreatedAtUtc = DateTime.UtcNow,
            FailCount = 0
        };

        foreach (var eventName in request.Events!.Distinct(StringComparer.Ordinal))
        {
            webhook.EventSubscriptions.Add(new WebhookEventSubscription { Id = NewId(), Event = eventName.Trim() });
        }

        foreach (var header in request.Headers ?? [])
        {
            webhook.Headers.Add(new WebhookHeader { Id = NewId(), Name = header.Key.Trim(), ValueEncrypted = header.Value });
        }

        db.WebhookEndpoints.Add(webhook);
        await db.SaveChangesAsync(cancellationToken);
        return Results.Created($"/api/v1/webhooks/{webhook.Id}", ToWebhookDto(webhook));
    }

    private static async Task<IResult> UpdateWebhookAsync(AppDbContext db, string id, UpdateWebhookRequest request, CancellationToken cancellationToken)
    {
        var webhook = await db.WebhookEndpoints
            .Include(row => row.EventSubscriptions)
            .Include(row => row.Headers)
            .FirstOrDefaultAsync(row => row.UserId == DefaultUserId && row.Id == id, cancellationToken);
        if (webhook is null) return NotFoundProblem("Webhook not found.");

        var errors = ValidateWebhook(request.Name, request.Url, request.Events, request.Headers, requireEvents: false, requireNameAndUrl: false);
        if (errors.HasErrors) return Results.ValidationProblem(errors.ToDictionary());

        if (request.Name is not null) webhook.Name = request.Name.Trim();
        if (request.Url is not null) webhook.Url = request.Url.Trim();
        if (request.Enabled is not null) webhook.Enabled = request.Enabled.Value;
        if (request.Secret is not null) webhook.SecretEncrypted = TrimToNull(request.Secret);
        if (request.Events is not null)
        {
            webhook.EventSubscriptions.Clear();
            foreach (var eventName in request.Events.Distinct(StringComparer.Ordinal))
            {
                webhook.EventSubscriptions.Add(new WebhookEventSubscription { Id = NewId(), WebhookEndpointId = webhook.Id, Event = eventName.Trim() });
            }
        }
        if (request.Headers is not null)
        {
            webhook.Headers.Clear();
            foreach (var header in request.Headers)
            {
                webhook.Headers.Add(new WebhookHeader { Id = NewId(), WebhookEndpointId = webhook.Id, Name = header.Key.Trim(), ValueEncrypted = header.Value });
            }
        }

        await db.SaveChangesAsync(cancellationToken);
        return Results.Ok(ToWebhookDto(webhook));
    }

    private static async Task<IResult> DeleteWebhookAsync(AppDbContext db, string id, CancellationToken cancellationToken)
    {
        var webhook = await db.WebhookEndpoints.FirstOrDefaultAsync(row => row.UserId == DefaultUserId && row.Id == id, cancellationToken);
        if (webhook is null) return NotFoundProblem("Webhook not found.");

        db.WebhookEndpoints.Remove(webhook);
        await db.SaveChangesAsync(cancellationToken);
        return Results.NoContent();
    }

    private static async Task<IResult> TestWebhookAsync(AppDbContext db, IHttpClientFactory httpClientFactory, string id, TestWebhookRequest? request, CancellationToken cancellationToken)
    {
        var webhook = await LoadWebhookAsync(db, id, cancellationToken);
        if (webhook is null) return NotFoundProblem("Webhook not found.");

        var payload = request?.Payload ?? new { test = true, message = "This is a test webhook from CashTrack" };
        var result = await DeliverWebhookAsync(db, httpClientFactory.CreateClient("webhooks"), webhook, WebhookEvents.TransactionCreated, payload, cancellationToken);
        return Results.Ok(result);
    }

    private static async Task<IResult> SendWebhooksAsync(AppDbContext db, IHttpClientFactory httpClientFactory, SendWebhookRequest request, CancellationToken cancellationToken)
    {
        var errors = new ErrorBag();
        if (string.IsNullOrWhiteSpace(request.Event) || !IsValidWebhookEvent(request.Event)) errors.Add("event", "event is not supported.");
        if (errors.HasErrors) return Results.ValidationProblem(errors.ToDictionary());

        var query = db.WebhookEndpoints
            .Include(webhook => webhook.EventSubscriptions)
            .Include(webhook => webhook.Headers)
            .Where(webhook => webhook.UserId == DefaultUserId && webhook.Enabled && webhook.FailCount < 5 && webhook.EventSubscriptions.Any(item => item.Event == request.Event));

        if (request.WebhookIds is { Count: > 0 })
        {
            query = query.Where(webhook => request.WebhookIds.Contains(webhook.Id));
        }

        var webhooks = await query.ToListAsync(cancellationToken);
        var client = httpClientFactory.CreateClient("webhooks");
        var results = new List<WebhookDeliveryResultDto>();
        foreach (var webhook in webhooks)
        {
            results.Add(await DeliverWebhookAsync(db, client, webhook, request.Event!, request.Data, cancellationToken));
        }

        return Results.Ok(results);
    }

    private static async Task<IResult> ExportBackupAsync(AppDbContext db, string? format, CancellationToken cancellationToken)
    {
        var actualFormat = string.IsNullOrWhiteSpace(format) ? "json" : format.Trim().ToLowerInvariant();
        if (actualFormat != "json")
        {
            var errors = new ErrorBag();
            errors.Add("format", "Only json export is implemented in this API scaffold.");
            return Results.ValidationProblem(errors.ToDictionary());
        }

        var transactions = await db.Transactions
            .AsNoTracking()
            .Where(transaction => transaction.UserId == DefaultUserId && !transaction.IsDeleted)
            .OrderByDescending(transaction => transaction.CreatedAtUtc)
            .Select(transaction => ToTransactionDto(transaction))
            .ToListAsync(cancellationToken);

        var json = JsonSerializer.Serialize(transactions, JsonOptions);
        var bytes = Encoding.UTF8.GetBytes(json);
        return Results.File(bytes, "application/json", $"cashtrack_backup_{DateTime.UtcNow:yyyyMMdd}.json");
    }

    private static async Task<IResult> ImportBackupAsync(AppDbContext db, HttpRequest httpRequest, CancellationToken cancellationToken)
    {
        await EnsureDefaultUserAsync(db, cancellationToken);
        BackupImportRequest? importRequest;
        if (httpRequest.HasFormContentType)
        {
            var form = await httpRequest.ReadFormAsync(cancellationToken);
            var file = form.Files.GetFile("file") ?? form.Files.FirstOrDefault();
            if (file is null)
            {
                var errors = new ErrorBag();
                errors.Add("file", "file is required.");
                return Results.ValidationProblem(errors.ToDictionary());
            }

            if (!file.FileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            {
                var errors = new ErrorBag();
                errors.Add("file", "Only json import is implemented in this API scaffold.");
                return Results.ValidationProblem(errors.ToDictionary());
            }

            await using var stream = file.OpenReadStream();
            var items = await JsonSerializer.DeserializeAsync<List<ImportTransactionDto>>(stream, JsonOptions, cancellationToken);
            importRequest = new BackupImportRequest { Transactions = items };
        }
        else
        {
            importRequest = await httpRequest.ReadFromJsonAsync<BackupImportRequest>(JsonOptions, cancellationToken);
        }

        if (importRequest?.Transactions is null)
        {
            var errors = new ErrorBag();
            errors.Add("transactions", "transactions are required.");
            return Results.ValidationProblem(errors.ToDictionary());
        }

        var imported = 0;
        var updated = 0;
        var skipped = 0;
        var importErrors = new List<ImportErrorDto>();
        var row = 0;
        foreach (var item in importRequest.Transactions)
        {
            row++;
            var create = new CreateTransactionRequest
            {
                Amount = item.Amount,
                Type = item.Type,
                Category = item.Category,
                Description = item.Description,
                Merchant = item.Merchant,
                BankAccount = item.BankAccount,
                Source = item.Source,
                RawNotification = item.RawNotification,
                ExternalId = item.ExternalId,
                CreatedAt = item.CreatedAt
            };
            var errors = await ValidateCreateTransactionAsync(db, create, cancellationToken);
            if (errors.HasErrors)
            {
                skipped++;
                importErrors.AddRange(errors.ToDictionary().SelectMany(pair => pair.Value.Select(message => new ImportErrorDto(row, pair.Key, message))));
                continue;
            }

            Transaction? existing = null;
            if (!string.IsNullOrWhiteSpace(item.Id))
            {
                existing = await db.Transactions.FirstOrDefaultAsync(transaction => transaction.UserId == DefaultUserId && transaction.Id == item.Id, cancellationToken);
            }

            if (existing is not null)
            {
                existing.AmountMinorUnits = item.Amount!.Value;
                existing.Type = Enum.Parse<TransactionType>(item.Type!.Trim());
                existing.CategoryId = item.Category!.Trim();
                existing.Description = TrimToNull(item.Description) ?? item.Category!.Trim();
                existing.Merchant = TrimToNull(item.Merchant);
                existing.BankAccount = TrimToNull(item.BankAccount);
                existing.BankCode = await ResolveBankCodeAsync(db, item.BankAccount, cancellationToken);
                existing.Source = string.IsNullOrWhiteSpace(item.Source) ? TransactionSource.manual : Enum.Parse<TransactionSource>(item.Source.Trim());
                existing.RawNotification = TrimToNull(item.RawNotification);
                existing.RawNotificationHash = string.IsNullOrWhiteSpace(item.RawNotification) ? null : Sha256Hex(item.RawNotification.Trim());
                existing.UpdatedAtUtc = item.UpdatedAt is null ? DateTime.UtcNow : ToUtc(item.UpdatedAt.Value);
                updated++;
            }
            else
            {
                var now = DateTime.UtcNow;
                var transactionDate = item.CreatedAt is null ? now : ToUtc(item.CreatedAt.Value);
                db.Transactions.Add(new Transaction
                {
                    Id = TrimToNull(item.Id) ?? NewId(),
                    UserId = DefaultUserId,
                    ExternalId = TrimToNull(item.ExternalId),
                    AmountMinorUnits = item.Amount!.Value,
                    Type = Enum.Parse<TransactionType>(item.Type!.Trim()),
                    CategoryId = item.Category!.Trim(),
                    Description = TrimToNull(item.Description) ?? item.Category!.Trim(),
                    Merchant = TrimToNull(item.Merchant),
                    BankAccount = TrimToNull(item.BankAccount),
                    BankCode = await ResolveBankCodeAsync(db, item.BankAccount, cancellationToken),
                    Source = string.IsNullOrWhiteSpace(item.Source) ? TransactionSource.manual : Enum.Parse<TransactionSource>(item.Source.Trim()),
                    RawNotification = TrimToNull(item.RawNotification),
                    RawNotificationHash = string.IsNullOrWhiteSpace(item.RawNotification) ? null : Sha256Hex(item.RawNotification.Trim()),
                    TransactionDateUtc = transactionDate,
                    CreatedAtUtc = transactionDate,
                    UpdatedAtUtc = now
                });
                imported++;
            }
        }

        await db.SaveChangesAsync(cancellationToken);
        return Results.Ok(new ImportResultDto(importErrors.Count == 0, importRequest.Transactions.Count, imported, updated, skipped, importErrors));
    }

    private static async Task<ErrorBag> ValidateCreateTransactionAsync(AppDbContext db, CreateTransactionRequest request, CancellationToken cancellationToken)
    {
        var errors = new ErrorBag();
        if (request.Amount is null) errors.Add("amount", "amount is required.");
        else if (request.Amount <= 0) errors.Add("amount", "amount must be greater than 0.");
        if (string.IsNullOrWhiteSpace(request.Type)) errors.Add("type", "type is required.");
        else if (!IsValidTransactionType(request.Type)) errors.Add("type", "type must be income or expense.");
        if (string.IsNullOrWhiteSpace(request.Category)) errors.Add("category", "category is required.");
        else if (!IsValidCategory(request.Category)) errors.Add("category", "category is not supported.");
        if (request.Source is not null && !IsValidTransactionSource(request.Source)) errors.Add("source", "source must be notification, manual, or api.");
        if (request.Description is { Length: > 200 }) errors.Add("description", "description must be 200 characters or fewer.");
        if (request.Merchant is { Length: > 100 }) errors.Add("merchant", "merchant must be 100 characters or fewer.");
        if (!string.IsNullOrWhiteSpace(request.Category) && IsValidCategory(request.Category))
        {
            var exists = await db.TransactionCategories.AnyAsync(category => category.Id == request.Category.Trim(), cancellationToken);
            if (!exists) errors.Add("category", "category does not exist in the database.");
        }
        return errors;
    }

    private static async Task<ErrorBag> ValidateUpdateTransactionAsync(AppDbContext db, UpdateTransactionRequest request, CancellationToken cancellationToken)
    {
        var errors = new ErrorBag();
        if (request.Amount is null && request.Type is null && request.Category is null && request.Description is null && request.Merchant is null && request.BankAccount is null)
        {
            errors.Add("request", "at least one field is required.");
        }
        if (request.Amount is <= 0) errors.Add("amount", "amount must be greater than 0.");
        if (request.Type is not null && !IsValidTransactionType(request.Type)) errors.Add("type", "type must be income or expense.");
        if (request.Category is not null && !IsValidCategory(request.Category)) errors.Add("category", "category is not supported.");
        if (request.Description is { Length: > 200 }) errors.Add("description", "description must be 200 characters or fewer.");
        if (request.Merchant is { Length: > 100 }) errors.Add("merchant", "merchant must be 100 characters or fewer.");
        if (!string.IsNullOrWhiteSpace(request.Category) && IsValidCategory(request.Category))
        {
            var exists = await db.TransactionCategories.AnyAsync(category => category.Id == request.Category.Trim(), cancellationToken);
            if (!exists) errors.Add("category", "category does not exist in the database.");
        }
        return errors;
    }

    private static ErrorBag ValidateWebhook(string? name, string? url, IReadOnlyList<string>? events, IReadOnlyDictionary<string, string>? headers, bool requireEvents, bool requireNameAndUrl)
    {
        var errors = new ErrorBag();
        if (requireNameAndUrl || name is not null)
        {
            if (string.IsNullOrWhiteSpace(name)) errors.Add("name", "name is required.");
            else if (name.Length > 160) errors.Add("name", "name must be 160 characters or fewer.");
        }
        if (requireNameAndUrl || url is not null)
        {
            if (string.IsNullOrWhiteSpace(url)) errors.Add("url", "url is required.");
            else if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)) errors.Add("url", "url must be an absolute http or https URL.");
        }
        if (requireEvents || events is not null)
        {
            if (events is null || events.Count == 0) errors.Add("events", "at least one event is required.");
            else
            {
                foreach (var eventName in events)
                {
                    if (!IsValidWebhookEvent(eventName)) errors.Add("events", $"event '{eventName}' is not supported.");
                }
            }
        }
        foreach (var header in headers ?? new Dictionary<string, string>())
        {
            if (string.IsNullOrWhiteSpace(header.Key)) errors.Add("headers", "header name cannot be empty.");
            if (string.IsNullOrWhiteSpace(header.Value)) errors.Add("headers", $"header '{header.Key}' value cannot be empty.");
        }
        return errors;
    }

    private static async Task<bool> HasTransactionConflictAsync(AppDbContext db, string? externalId, string? rawNotificationHash, string? currentId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(externalId) && string.IsNullOrWhiteSpace(rawNotificationHash)) return false;
        return await db.Transactions.AnyAsync(transaction =>
            transaction.UserId == DefaultUserId
            && !transaction.IsDeleted
            && (currentId == null || transaction.Id != currentId)
            && ((!string.IsNullOrWhiteSpace(externalId) && transaction.ExternalId == externalId.Trim())
                || (!string.IsNullOrWhiteSpace(rawNotificationHash) && transaction.RawNotificationHash == rawNotificationHash)), cancellationToken);
    }

    private static async Task EnsureDefaultUserAsync(AppDbContext db, CancellationToken cancellationToken)
    {
        if (await db.Users.AnyAsync(user => user.Id == DefaultUserId, cancellationToken)) return;
        var now = DateTime.UtcNow;
        db.Users.Add(new UserAccount
        {
            Id = DefaultUserId,
            FullName = "CashTrack Dev User",
            Email = "dev@cashtrack.local",
            PasswordHash = "not-used-in-dev-api",
            PasswordSalt = "not-used",
            DataKeySalt = "not-used",
            EncryptedDataKey = "not-used",
            PasswordAlgorithm = "dev",
            BcryptRounds = 0,
            DataKeyAlgorithm = "none",
            KeyDerivation = "none",
            KeyDerivationIterations = 0,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        });
        await db.SaveChangesAsync(cancellationToken);
    }

    private static async Task<string?> ResolveBankCodeAsync(AppDbContext db, string? bankAccount, CancellationToken cancellationToken)
    {
        var value = TrimToNull(bankAccount);
        if (value is null) return null;
        var upper = value.ToUpperInvariant();
        return await db.BankApps.AnyAsync(bank => bank.Code == upper, cancellationToken) ? upper : null;
    }

    private static async Task<WebhookEndpoint?> LoadWebhookAsync(AppDbContext db, string id, CancellationToken cancellationToken)
    {
        return await db.WebhookEndpoints
            .Include(webhook => webhook.EventSubscriptions)
            .Include(webhook => webhook.Headers)
            .FirstOrDefaultAsync(webhook => webhook.UserId == DefaultUserId && webhook.Id == id, cancellationToken);
    }

    private static async Task<WebhookDeliveryResultDto> DeliverWebhookAsync(AppDbContext db, HttpClient client, WebhookEndpoint webhook, string eventName, object? data, CancellationToken cancellationToken)
    {
        var timestamp = DateTime.UtcNow;
        var payload = new WebhookPayloadDto(eventName, timestamp, data);
        int? statusCode = null;
        string? error = null;
        var success = false;

        try
        {
            using var message = new HttpRequestMessage(HttpMethod.Post, webhook.Url)
            {
                Content = JsonContent.Create(payload, options: JsonOptions)
            };
            message.Headers.TryAddWithoutValidation("User-Agent", "CashTrack-Webhook/1.0");
            message.Headers.TryAddWithoutValidation("X-Webhook-Event", eventName);
            message.Headers.TryAddWithoutValidation("X-Webhook-Delivery", timestamp.Ticks.ToString());
            foreach (var header in webhook.Headers)
            {
                message.Headers.TryAddWithoutValidation(header.Name, header.ValueEncrypted);
            }
            var response = await client.SendAsync(message, cancellationToken);
            statusCode = (int)response.StatusCode;
            success = response.IsSuccessStatusCode;
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException)
        {
            error = exception.Message;
        }

        webhook.LastTriggeredAtUtc = timestamp;
        webhook.FailCount = success ? 0 : webhook.FailCount + 1;
        db.WebhookDeliveryLogs.Add(new WebhookDeliveryLog
        {
            Id = NewId(),
            WebhookEndpointId = webhook.Id,
            Event = eventName,
            PayloadJson = JsonSerializer.Serialize(payload, JsonOptions),
            Success = success,
            StatusCode = statusCode,
            Error = error,
            TimestampUtc = timestamp
        });
        await db.SaveChangesAsync(cancellationToken);
        return new WebhookDeliveryResultDto(webhook.Id, success, statusCode, error, timestamp);
    }

    private static TransactionDto ToTransactionDto(Transaction transaction)
    {
        return new TransactionDto(
            transaction.Id,
            transaction.AmountMinorUnits,
            transaction.Type.ToString(),
            transaction.CategoryId,
            transaction.Description,
            transaction.Merchant,
            transaction.BankAccount ?? transaction.BankCode,
            transaction.Source.ToString(),
            transaction.RawNotification,
            ToUtc(transaction.CreatedAtUtc),
            ToUtc(transaction.UpdatedAtUtc));
    }

    private static WebhookConfigDto ToWebhookDto(WebhookEndpoint webhook)
    {
        var headers = webhook.Headers.Count == 0
            ? null
            : webhook.Headers.ToDictionary(header => header.Name, header => header.ValueEncrypted, StringComparer.Ordinal);
        return new WebhookConfigDto(
            webhook.Id,
            webhook.Name,
            webhook.Url,
            webhook.Enabled,
            webhook.EventSubscriptions.Select(item => item.Event).Order().ToList(),
            headers,
            ToUtc(webhook.CreatedAtUtc),
            webhook.LastTriggeredAtUtc is null ? null : ToUtc(webhook.LastTriggeredAtUtc.Value),
            webhook.FailCount,
            !string.IsNullOrWhiteSpace(webhook.SecretEncrypted));
    }

    private static bool IsValidTransactionType(string value) => Enum.TryParse<TransactionType>(value.Trim(), ignoreCase: false, out _);
    private static bool IsValidTransactionSource(string value) => Enum.TryParse<TransactionSource>(value.Trim(), ignoreCase: false, out _);
    private static bool IsValidCategory(string value) => ValidCategories.Contains(value.Trim(), StringComparer.Ordinal);
    private static bool IsValidWebhookEvent(string? value) => value is not null && ValidWebhookEvents.Contains(value.Trim(), StringComparer.Ordinal);

    private static string NewId() => Guid.NewGuid().ToString("N");
    private static string? TrimToNull(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static DateTime ToUtc(DateTime value)
    {
        return value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
        };
    }

    private static string Sha256Hex(string value)
    {
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();
    }

    private static IResult NotFoundProblem(string detail) => Results.Problem(title: "Not Found", detail: detail, statusCode: StatusCodes.Status404NotFound);
    private static IResult ConflictProblem(string detail) => Results.Problem(title: "Conflict", detail: detail, statusCode: StatusCodes.Status409Conflict);

    private sealed class ErrorBag
    {
        private readonly Dictionary<string, List<string>> _errors = new(StringComparer.Ordinal);
        public bool HasErrors => _errors.Count > 0;

        public void Add(string key, string message)
        {
            if (!_errors.TryGetValue(key, out var values))
            {
                values = [];
                _errors[key] = values;
            }
            values.Add(message);
        }

        public Dictionary<string, string[]> ToDictionary()
        {
            return _errors.ToDictionary(pair => pair.Key, pair => pair.Value.Distinct().ToArray(), StringComparer.Ordinal);
        }
    }
}
