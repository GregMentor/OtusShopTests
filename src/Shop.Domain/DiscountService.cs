namespace Shop.Domain;

/// <summary>
/// Сервис расчёта скидок. Бизнес-правила:
/// - Заказ &lt; 1000 → скидка 0%
/// - Заказ 1000–4999 → 5%
/// - Заказ 5000–9999 → 10%
/// - Заказ ≥ 10000 → 15%
/// - заблокированные аккаунты — расчёт скидки и оформление заказа невозможны;
/// - VIP-клиенты получают удвоенную скидку, но не более 30%
/// </summary>
public class DiscountService
{
    private readonly ICustomerService _customerService;

    public DiscountService(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    public decimal CalculateDiscount(int customerId, decimal orderAmount)
    {
        if (orderAmount < 0)
            throw new ArgumentException("Сумма заказа не может быть отрицательной", nameof(orderAmount));

        if (_customerService.IsAccountSuspended(customerId))
            throw new InvalidOperationException("Аккаунт клиента заблокирован; оформление заказа недоступно.");

        decimal discountPercent = orderAmount switch
        {
            < 1000 => 0m,
            < 5000 => 5m,
            < 10000 => 10m,
            _ => 15m
        };

        if (_customerService.IsVipCustomer(customerId))
        {
            discountPercent *= 2;
            if (discountPercent > 30m)
                discountPercent = 30m;
        }

        return orderAmount * discountPercent / 100m;
    }
}
