using Microsoft.Extensions.DependencyInjection;
using Shop.Api.Data;
using Shop.Api.Models;
using Shop.IntegrationTests.HttpClients;
using Shop.IntegrationTests.Services;
using Shop.IntegrationTests.Infrastructure.TestTokens;
using Xunit;

namespace Shop.IntegrationTests.Infrastructure;

/// <summary>
/// Shared-контекст для интеграционных тестов.
/// Здесь удобно держать typed API clients и общие хелперы.
/// </summary>
public sealed class TestFixture : IAsyncLifetime
{
    private readonly ApiFactory _factory = new();

    public HttpClient HttpClient { get; private set; } = default!;
    public OrdersApiClient Orders { get; private set; } = default!;
    public TestCustomerService Customers => _factory.CustomerService;
    public StubNotificationService Notifications => _factory.Notifications;

    public Task InitializeAsync()
    {
        HttpClient = _factory.CreateClient();
        Orders = new OrdersApiClient(HttpClient, () => TestTokenContext.CurrentToken);
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        HttpClient.Dispose();
        _factory.Dispose();
        return Task.CompletedTask;
    }

    public async Task<Order> SeedOrderAsync(Order order, CancellationToken ct = default)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Orders.Add(order);
        await db.SaveChangesAsync(ct);
        return order;
    }
}

