using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Shop.Api.Services;

namespace Shop.IntegrationTests.Services;

public sealed class StubNotificationService : INotificationService
{
    private ILogger<StubNotificationService> _log = NullLogger<StubNotificationService>.Instance;

    public List<(int OrderId, int CustomerId, decimal PayableAmount)> Sent { get; } = new();

    internal void SetLogger(ILogger<StubNotificationService> logger) => _log = logger ?? NullLogger<StubNotificationService>.Instance;

    public Task OrderCreatedAsync(int orderId, int customerId, decimal payableAmount, CancellationToken ct = default)
    {
        Sent.Add((orderId, customerId, payableAmount));
        _log.LogInformation(
            "StubNotificationService.OrderCreatedAsync: orderId={OrderId}, customerId={CustomerId}, payableAmount={PayableAmount}",
            orderId, customerId, payableAmount);
        return Task.CompletedTask;
    }
}

