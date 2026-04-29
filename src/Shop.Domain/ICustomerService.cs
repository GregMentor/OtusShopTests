namespace Shop.Domain;

/// <summary>
/// Внешняя зависимость для DiscountService и точек принятия решений по клиенту.
/// В юнит-тестах будем мокать через Moq.
/// </summary>
public interface ICustomerService
{
    /// <summary>Признак VIP (расширенная скидка по правилам лояльности).</summary>
    bool IsVipCustomer(int customerId);

    /// <summary>
    /// Аккаунт заблокирован для оформления заказа (просрочка, решение службы безопасности и т.д.).
    /// При <c>true</c> создание заказа должно быть отклонено на уровне домена/API.
    /// </summary>
    bool IsAccountSuspended(int customerId);
}
