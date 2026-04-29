using Shop.Domain;

namespace Shop.Api.Services;

/// <summary>
/// Пример "внешнего" сервиса.
/// В реальном приложении это мог бы быть HTTP/gRPC/DB-клиент, но для демо
/// оставляем простую реализацию, которую удобно подменять в интеграционных тестах.
/// </summary>
public sealed class CustomerService : ICustomerService
{
    public bool IsVipCustomer(int customerId) => false;
    public bool IsAccountSuspended(int customerId) => true;

    public int NorthWind(int customerId) => 0;
}

