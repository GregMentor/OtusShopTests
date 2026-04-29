namespace Shop.Api.Services;

/// <summary>
/// Заглушка для демо: в реальном приложении здесь был бы вызов внешнего провайдера
/// (email/SMS/шина событий). Для примера интеграционного теста нам важен сам факт
/// зависимости, которую можно заменить stub-реализацией.
/// </summary>
public sealed class NotificationService : INotificationService
{
    public Task OrderCreatedAsync(int orderId, int customerId, decimal payableAmount, CancellationToken ct = default)
        => Task.CompletedTask;
}

