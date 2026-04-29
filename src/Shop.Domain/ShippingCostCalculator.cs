namespace Shop.Domain;

/// <summary>
/// Мини-практика по TDD: калькулятор стоимости доставки.
/// Правила:
/// - orderAmount <= 1000 => доставка 0
/// - далее каждые полные/неполные 1000 сверху добавляют +200 (1001..2000 => 200, 2001..3000 => 400, ...)
/// </summary>
public static class ShippingCostCalculator
{
    public static decimal Calculate(decimal orderAmount)
    {
        if (orderAmount < 0)
            throw new ArgumentException("Сумма заказа не может быть отрицательной", nameof(orderAmount));

        if (orderAmount <= 1000m)
            return 0m;

        var extra = orderAmount - 1000m;
        var steps = (int)Math.Ceiling(extra / 1000m);
        return steps * 200m;
    }
}

