using FluentAssertions;
using Shop.Api.Services;

namespace Shop.Tests;

/// <summary>
/// Юнит-тесты для <see cref="LoyaltyTierCustomerService"/> (<see cref="LoyaltyTierCustomerService.IsVipCustomer"/> и
/// <see cref="LoyaltyTierCustomerService.IsAccountSuspended"/>).
/// Каждый тест: Arrange — Act — Assert.
/// </summary>
public class LoyaltyTierCustomerServiceTests
{
    [Theory]
    [InlineData(100)]
    [InlineData(200)]
    [InlineData(10_000)]
    public void IsVipCustomer_GoldTierPositiveIdDivisibleBy100_ReturnsTrue(int customerId)
    {
        // Arrange
        var sut = new LoyaltyTierCustomerService();

        // Act
        var isVip = sut.IsVipCustomer(customerId);

        // Assert
        isVip.Should().BeTrue();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(99)]
    [InlineData(101)]
    [InlineData(9_999)]
    public void IsVipCustomer_NotGoldTier_ReturnsFalse(int customerId)
    {
        // Arrange
        var sut = new LoyaltyTierCustomerService();

        // Act
        var isVip = sut.IsVipCustomer(customerId);

        // Assert
        isVip.Should().BeFalse();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void IsVipCustomer_NonPositiveId_ReturnsFalse(int customerId)
    {
        // Arrange
        var sut = new LoyaltyTierCustomerService();

        // Act
        var isVip = sut.IsVipCustomer(customerId);

        // Assert
        isVip.Should().BeFalse();
    }

    [Theory]
    [InlineData(523)]
    [InlineData(1046)]
    public void IsAccountSuspended_PositiveIdsDivisibleBy523_ReturnsTrue(int customerId)
    {
        // Arrange
        var sut = new LoyaltyTierCustomerService();

        // Act
        var suspended = sut.IsAccountSuspended(customerId);

        // Assert
        suspended.Should().BeTrue();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(524)]
    [InlineData(10_001)]
    public void IsAccountSuspended_ActiveRetailId_ReturnsFalse(int customerId)
    {
        // Arrange
        var sut = new LoyaltyTierCustomerService();

        // Act
        var suspended = sut.IsAccountSuspended(customerId);

        // Assert
        suspended.Should().BeFalse();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-523)]
    public void IsAccountSuspended_NonPositiveId_ReturnsTrue(int customerId)
    {
        // Arrange
        var sut = new LoyaltyTierCustomerService();

        // Act
        var suspended = sut.IsAccountSuspended(customerId);

        // Assert
        suspended.Should().BeTrue();
    }
}
