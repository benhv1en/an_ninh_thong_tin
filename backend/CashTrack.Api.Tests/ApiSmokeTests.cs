using System.Data;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using CashTrack.Api.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CashTrack.Api.Tests;

public sealed class ApiSmokeTests
{
    [Fact]
    public async Task Health_ReturnsOk()
    {
        using var factory = new CashTrackApiFactory();
        using var client = await factory.CreateInitializedClientAsync();

        using var response = await client.GetAsync("/health");
        using var payload = await response.Content.ReadFromJsonAsync<JsonDocument>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(payload);
        Assert.Equal("ok", payload.RootElement.GetProperty("status").GetString());
    }

    [Fact]
    public async Task GetTransactions_ReturnsEmptyList()
    {
        using var factory = new CashTrackApiFactory();
        using var client = await factory.CreateInitializedClientAsync();

        using var response = await client.GetAsync("/api/v1/transactions");
        using var payload = await response.Content.ReadFromJsonAsync<JsonDocument>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(payload);
        Assert.Equal(0, payload.RootElement.GetProperty("items").GetArrayLength());
        Assert.True(payload.RootElement.TryGetProperty("nextCursor", out _));
    }

    [Fact]
    public async Task PostTransaction_CreatesTransactionAndCanBeListed()
    {
        using var factory = new CashTrackApiFactory();
        using var client = await factory.CreateInitializedClientAsync();
        var request = new
        {
            amount = 123456,
            type = "expense",
            category = "food",
            description = "API smoke test",
            merchant = "Test Merchant",
            source = "manual",
            externalId = $"test-{Guid.NewGuid():N}",
            createdAt = "2026-06-12T00:00:00.000Z"
        };

        using var createResponse = await client.PostAsJsonAsync("/api/v1/transactions", request);
        using var createdPayload = await createResponse.Content.ReadFromJsonAsync<JsonDocument>();
        using var listResponse = await client.GetAsync("/api/v1/transactions");
        using var listPayload = await listResponse.Content.ReadFromJsonAsync<JsonDocument>();

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        Assert.NotNull(createdPayload);
        Assert.False(string.IsNullOrWhiteSpace(createdPayload.RootElement.GetProperty("id").GetString()));
        Assert.Equal(123456, createdPayload.RootElement.GetProperty("amount").GetInt32());
        Assert.Equal("expense", createdPayload.RootElement.GetProperty("type").GetString());
        Assert.Equal("food", createdPayload.RootElement.GetProperty("category").GetString());

        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);
        Assert.NotNull(listPayload);
        Assert.Equal(1, listPayload.RootElement.GetProperty("items").GetArrayLength());
    }

    [Fact]
    public async Task PostTransaction_WithInvalidBody_ReturnsValidationProblem()
    {
        using var factory = new CashTrackApiFactory();
        using var client = await factory.CreateInitializedClientAsync();

        using var response = await client.PostAsJsonAsync("/api/v1/transactions", new { });
        using var payload = await response.Content.ReadFromJsonAsync<JsonDocument>();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        Assert.NotNull(payload);
        var errors = payload.RootElement.GetProperty("errors");
        Assert.True(errors.TryGetProperty("amount", out _));
        Assert.True(errors.TryGetProperty("type", out _));
        Assert.True(errors.TryGetProperty("category", out _));
    }
}

internal sealed class CashTrackApiFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection = new("Data Source=:memory:");

    public async Task<HttpClient> CreateInitializedClientAsync()
    {
        var client = CreateClient();
        await ResetDatabaseAsync();
        return client;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            if (_connection.State != ConnectionState.Open)
            {
                _connection.Open();
            }

            services.AddDbContext<AppDbContext>(options => options.UseSqlite(_connection));
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        _connection.Dispose();
    }

    private async Task ResetDatabaseAsync()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.EnsureDeletedAsync();
        await db.Database.EnsureCreatedAsync();
    }
}
