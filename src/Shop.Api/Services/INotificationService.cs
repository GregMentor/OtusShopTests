namespace Shop.Api.Services;

public interface INotificationService
{
    Task OrderCreatedAsync(int orderId, int customerId, decimal payableAmount, CancellationToken ct = default);
}

