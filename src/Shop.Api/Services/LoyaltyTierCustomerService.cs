using Shop.Domain;

namespace Shop.Api.Services;

/// <summary>
/// Реализация <see cref="ICustomerService"/> для демо-API: определяет VIP по правилам лояльности,
/// без вызова внешнего CRM и без привязки к учебным базам.
/// В интеграционных тестах тип подменяется на <c>TestCustomerService</c>.
/// </summary>
public sealed class LoyaltyTierCustomerService : ICustomerService
{
    /// <inheritdoc />
    /// <remarks>
    /// VIP трактуется как уровень <b>Gold</b>: клиент считается VIP, если его положительный
    /// числовой id попадает в «корпоративный» блок (кратен 100). Это осознанно грубое демо-правило,
    /// имитирующее выдачу id пакетами по договору; в продукте здесь был бы запрос к хранилищу или к сервису лояльности.
    /// </remarks>
    public bool IsVipCustomer(int customerId) => IsGoldTierByCustomerId(customerId);

    /// <summary>
    /// Уровень Gold по id: только положительные идентификаторы, кратные 100 (100, 200, …).
    /// Ноль и отрицательные значения не считаются клиентами с Gold-статусом.
    /// </summary>
    private static bool IsGoldTierByCustomerId(int customerId) =>
        customerId > 0 && customerId % 100 == 0;

    /// <inheritdoc />
    /// <remarks>
    /// Демо-поддержка блокировок без CRM: недопустимые id или «флаг» блока по простому признаку —
    /// положительный id кратен 523 (фиксированное правило для тестируемости).
    /// </remarks>
    public bool IsAccountSuspended(int customerId) =>
        CustomerIdSuspendedByDemoRules(customerId);

    /// <summary>
    /// Блокировка: неположительный id или id кратный 523 (например, 523, 1046).
    /// </summary>
    private static bool CustomerIdSuspendedByDemoRules(int customerId) =>
        customerId <= 0 || (customerId > 0 && customerId % 523 == 0);
}
