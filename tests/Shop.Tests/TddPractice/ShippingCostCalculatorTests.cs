using FluentAssertions;
using Bogus;

namespace Shop.Tests.TddPractice;

/// <summary>Тесты калькулятора доставки в паттерне Arrange — Act — Assert.</summary>
public class ShippingCostCalculatorTests
{
    [Theory]
    [InlineData(0, 0)]
    [InlineData(999, 0)]
    [InlineData(1000, 0)]
    [InlineData(1001, 200)]
    [InlineData(1999, 200)]
    [InlineData(2000, 200)]
    [InlineData(2001, 400)]
    public void Calculate_StepsOf1000_CalculatesExpected(decimal orderAmount, decimal expected)
    {
        // Arrange
        var amount = orderAmount;

        // Act
        var cost = Shop.Domain.ShippingCostCalculator.Calculate(amount);

        // Assert
        cost.Should().Be(expected);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Calculate_NegativeAmount_Throws(decimal orderAmount)
    {
        // Arrange
        var amount = orderAmount;

        // Act
        var act = () => Shop.Domain.ShippingCostCalculator.Calculate(amount);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Calculate_OrderGeneratedByBogus_FillsAllFieldsAndCalculatesExpected()
    {
        // Arrange
        var faker = new Faker<OrderDraft>()
            .RuleFor(x => x.Id, f => f.Random.Guid())
            .RuleFor(x => x.CustomerId, f => f.Random.Int(1, 1_000_000))
            .RuleFor(x => x.Amount, f => f.Random.Decimal(0m, 20_000m))
            .RuleFor(x => x.Currency, f => f.Finance.Currency().Code)
            .RuleFor(x => x.DestinationCountry, f => f.Address.CountryCode())
            .RuleFor(x => x.City, f => f.Address.City())
            .RuleFor(x => x.AddressLine1, f => f.Address.StreetAddress())
            .RuleFor(x => x.PostalCode, f => f.Address.ZipCode())
            .RuleFor(x => x.WeightKg, f => Math.Round(f.Random.Decimal(0.1m, 50m), 2))
            .RuleFor(x => x.ItemsCount, f => f.Random.Int(1, 50))
            .RuleFor(x => x.IsExpress, f => f.Random.Bool())
            .RuleFor(x => x.Notes, f => f.Lorem.Sentence())
            .RuleFor(x => x.CreatedAtUtc, f => f.Date.PastOffset(1).ToUniversalTime());

        var order = faker.Generate();

        var expected = order.Amount <= 1000m
            ? 0m
            : (int)Math.Ceiling((order.Amount - 1000m) / 1000m) * 200m;

        // Act
        var cost = Shop.Domain.ShippingCostCalculator.Calculate(order.Amount);

        // Assert
        cost.Should().Be(expected);
        order.Should().NotBeNull();
        order.Id.Should().NotBe(Guid.Empty);
        order.CustomerId.Should().BeGreaterThan(0);
        order.Currency.Should().NotBeNullOrWhiteSpace();
        order.DestinationCountry.Should().NotBeNullOrWhiteSpace();
        order.City.Should().NotBeNullOrWhiteSpace();
        order.AddressLine1.Should().NotBeNullOrWhiteSpace();
        order.PostalCode.Should().NotBeNullOrWhiteSpace();
        order.WeightKg.Should().BeGreaterThan(0m);
        order.ItemsCount.Should().BeGreaterThan(0);
        order.Notes.Should().NotBeNullOrWhiteSpace();
    }

    private sealed class OrderDraft
    {
        public Guid Id { get; init; }
        public int CustomerId { get; init; }
        public decimal Amount { get; init; }
        public string Currency { get; init; } = "";
        public string DestinationCountry { get; init; } = "";
        public string City { get; init; } = "";
        public string AddressLine1 { get; init; } = "";
        public string PostalCode { get; init; } = "";
        public decimal WeightKg { get; init; }
        public int ItemsCount { get; init; }
        public bool IsExpress { get; init; }
        public string Notes { get; init; } = "";
        public DateTimeOffset CreatedAtUtc { get; init; }
    }
}

