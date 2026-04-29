using Bogus;
using Shop.Domain;
using FluentAssertions;
using Moq;

namespace Shop.Tests;

/// <summary>
/// Юнит-тесты для DiscountService.
/// Все тесты следуют паттерну AAA (Arrange — Act — Assert).
/// Именование: MethodName_Условие_ОжидаемыйРезультат.
/// </summary>
public class DiscountServiceTests
{
    // ============================================================
    // ТЕСТ 1: Простой случай — заказ < 1000, скидка 0
    // ============================================================
    [Fact]
    public void CalculateDiscount_OrderBelow1000_ReturnsZero()
    {
        // Arrange
        var faker = new Faker();
        var customerId = faker.Random.Int(min: 1, max: 10_000);
        var customerServiceMock = new Mock<ICustomerService>();
        customerServiceMock
            .Setup(x => x.IsVipCustomer(It.IsAny<int>()))
            .Returns(false);

        var sut = new DiscountService(customerServiceMock.Object);
        const decimal orderAmount = 500m;

        // Act
        var discount = sut.CalculateDiscount(customerId, orderAmount);

        // Assert
        discount.Should().Be(0m);
    }

    // ============================================================
    // ТЕСТ 2: Параметризация — все диапазоны и границы
    // ============================================================
    [Theory]
    [InlineData(0,     0)]        // 0 руб → 0
    [InlineData(999,   0)]        // граница: 999 → 0%
    [InlineData(1000,  50)]       // граница: 1000 → 5% = 50
    [InlineData(4999,  249.95)]   // 5% = 249.95
    [InlineData(5000,  500)]      // граница: 5000 → 10% = 500
    [InlineData(9999,  999.9)]    // 10%
    [InlineData(10000, 1500)]     // граница: 10000 → 15% = 1500
    [InlineData(50000, 7500)]     // 15%
    public void CalculateDiscount_NotVipCustomer_ReturnsCorrectDiscount(
        decimal orderAmount,
        decimal expectedDiscount)
    {
        // Arrange
        var customerServiceMock = new Mock<ICustomerService>();
        customerServiceMock
            .Setup(x => x.IsVipCustomer(It.IsAny<int>()))
            .Returns(false);

        var sut = new DiscountService(customerServiceMock.Object);

        // Act
        var discount = sut.CalculateDiscount(1, orderAmount);

        // Assert
        discount.Should().Be(expectedDiscount);
    }

    // ============================================================
    // ТЕСТ 3: VIP-клиент — скидка удваивается
    // ============================================================
    [Fact]
    public void CalculateDiscount_VipCustomer_DoublesDiscount()
    {
        // Arrange
        var customerServiceMock = new Mock<ICustomerService>();
        customerServiceMock
            .Setup(x => x.IsVipCustomer(42))
            .Returns(true);

        var sut = new DiscountService(customerServiceMock.Object);

        // Act
        var discount = sut.CalculateDiscount(customerId: 42, orderAmount: 3000m);

        // Assert
        // 3000 * 5% = 150, VIP удваивает → 300
        discount.Should().Be(300m);
    }

    // ============================================================
    // ТЕСТ 4: VIP при большой сумме — cap на 30%
    // ============================================================
    [Fact]
    public void CalculateDiscount_VipCustomerHighAmount_CapsAt30Percent()
    {
        // Arrange
        var customerServiceMock = new Mock<ICustomerService>();
        customerServiceMock
            .Setup(x => x.IsVipCustomer(It.IsAny<int>()))
            .Returns(true);

        var sut = new DiscountService(customerServiceMock.Object);

        // Act
        // 50000 → 15% → VIP × 2 = 30% (граница cap)
        var discount = sut.CalculateDiscount(1, 50_000m);

        // Assert
        discount.Should().Be(15_000m);
    }

    // ============================================================
    // ТЕСТ 5: Проверка исключения на отрицательную сумму
    // ============================================================
    [Fact]
    public void CalculateDiscount_NegativeAmount_ThrowsArgumentException()
    {
        // Arrange
        var customerServiceMock = new Mock<ICustomerService>();
        var sut = new DiscountService(customerServiceMock.Object);

        // Act
        var act = () => sut.CalculateDiscount(1, -100m);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*отрицательной*");
    }

    // ============================================================
    // ТЕСТ 6: заблокированный аккаунт — исключение до расчёта скидки
    // ============================================================
    [Fact]
    public void CalculateDiscount_SuspendedCustomer_ThrowsInvalidOperationException()
    {
        // Arrange
        const int suspendedCustomerId = 99;
        var customerServiceMock = new Mock<ICustomerService>();
        customerServiceMock
            .Setup(x => x.IsAccountSuspended(suspendedCustomerId))
            .Returns(true);

        var sut = new DiscountService(customerServiceMock.Object);

        // Act
        var act = () => sut.CalculateDiscount(suspendedCustomerId, 1000m);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*заблокирован*");
        customerServiceMock.Verify(x => x.IsVipCustomer(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public void CalculateDiscount_Always_AsksSuspendAndVipStatus()
    {
        // Arrange
        var customerServiceMock = new Mock<ICustomerService>();
        customerServiceMock
            .Setup(x => x.IsVipCustomer(It.IsAny<int>()))
            .Returns(false);

        var sut = new DiscountService(customerServiceMock.Object);

        // Act
        sut.CalculateDiscount(customerId: 777, orderAmount: 2000m);

        // Assert
        customerServiceMock.Verify(x => x.IsAccountSuspended(777), Times.Once);
        customerServiceMock.Verify(
            x => x.IsVipCustomer(777),
            Times.Once);
    }
}
