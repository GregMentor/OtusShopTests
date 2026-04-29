using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Shop.Domain;

namespace Shop.IntegrationTests.Services;

public sealed class TestCustomerService : ICustomerService
{
    private ILogger<TestCustomerService> _log = NullLogger<TestCustomerService>.Instance;

    public HashSet<int> VipCustomerIds { get; } = new();

    /// <summary>Ид клиентов, для которых в тестах считают аккаунт заблокированным.</summary>
    public HashSet<int> SuspendedCustomerIds { get; } = new();

    internal void SetLogger(ILogger<TestCustomerService> logger) => _log = logger ?? NullLogger<TestCustomerService>.Instance;

    public bool IsVipCustomer(int customerId)
    {
        var isVip = VipCustomerIds.Contains(customerId);
        _log.LogInformation("TestCustomerService.IsVipCustomer: customerId={CustomerId}, isVip={IsVip}", customerId, isVip);
        return isVip;
    }

    public bool IsAccountSuspended(int customerId)
    {
        var suspended = SuspendedCustomerIds.Contains(customerId);
        _log.LogInformation(
            "TestCustomerService.IsAccountSuspended: customerId={CustomerId}, suspended={Suspended}",
            customerId,
            suspended);
        return suspended;
    }
}

