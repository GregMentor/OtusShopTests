using System.Net;
using FluentAssertions;
using Shop.Api.Models;
using Shop.Api.Models.DTO;
using Shop.IntegrationTests.Infrastructure;
using Shop.IntegrationTests.Infrastructure.TestTokens;

namespace Shop.IntegrationTests;

/// <summary>
/// Интеграционные тесты для OrdersController.
///
/// IClassFixture<ApiFactory> — стандартный механизм xUnit для shared-контекста:
/// фабрика создаётся ОДИН раз на весь класс, конструктор класса вызывается перед
/// КАЖДЫМ тестом (но ApiFactory переиспользуется).
///
/// HttpClient подключается напрямую к приложению в памяти — без сети и Kestrel.
///
/// В каждом тесте явно выделяются фазы Arrange — Act — Assert (при нескольких шагах
/// сценария цикл Act/Assert может повторяться).
/// </summary>
[UseTestToken]
public class OrdersApiTests : IClassFixture<TestFixture>
{
    private readonly TestFixture _fx;

    public OrdersApiTests(TestFixture fx) => _fx = fx;

    // ============================================================
    // ТЕСТ 1: Сценарий "создал → прочитал"
    // ============================================================
    [Fact]
    public async Task CreateOrder_ValidData_ReturnsCreatedAndCanBeRetrieved()
    {
        // Arrange
        var newOrder = new CreateOrderDto { CustomerId = 1, Amount = 2500m };

        // Act
        var createResponse = await _fx.Orders.CreateAsync(newOrder);

        // Assert
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var createdOrder = await _fx.Orders.ReadOrderAsync(createResponse);
        createdOrder.Should().NotBeNull();
        createdOrder!.Id.Should().BeGreaterThan(0);

        // Act
        var getResponse = await _fx.Orders.GetByIdAsync(createdOrder.Id);

        // Assert
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var fetched = await _fx.Orders.ReadOrderAsync(getResponse);
        fetched!.CustomerId.Should().Be(1);
        fetched.Amount.Should().Be(2500m);
    }

    // ============================================================
    // ТЕСТ 2: Негативный сценарий — отрицательная сумма
    // ============================================================
    [Fact]
    public async Task CreateOrder_NegativeAmount_Returns400()
    {
        // Arrange
        var badOrder = new CreateOrderDto { CustomerId = 1, Amount = -100m };

        // Act
        var response = await _fx.Orders.CreateAsync(badOrder);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ============================================================
    // ТЕСТ 3 (только для 90-минутной версии): несуществующий id → 404
    // ============================================================
    [Fact]
    public async Task GetOrder_NonExistentId_Returns404()
    {
        // Arrange
        const int nonExistentOrderId = 99_999;

        // Act
        var response = await _fx.Orders.GetByIdAsync(nonExistentOrderId);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ============================================================
    // ТЕСТ 4: Подмена внешнего сервиса (ICustomerService) в интеграционном тесте
    // ============================================================
    [Fact]
    public async Task CreateOrder_VipCustomer_AppliesVipDiscount()
    {
        // Arrange
        // В тестовом окружении мы можем управлять "внешней" зависимостью через DI.
        _fx.Customers.VipCustomerIds.Add(1);

        // 1000–4999 => 5%, VIP => удвоение => 10%
        var dto = new CreateOrderDto { CustomerId = 1, Amount = 2500m };

        // Act
        var createResponse = await _fx.Orders.CreateAsync(dto);

        // Assert
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await _fx.Orders.ReadOrderAsync(createResponse);
        created.Should().NotBeNull();

        created!.Amount.Should().Be(2500m);
        created.DiscountAmount.Should().Be(250m);
        created.PayableAmount.Should().Be(2250m);
    }

    // ============================================================
    // ТЕСТ 5: Shift-left (top-down) — тестируем раньше, стабим низ
    // ============================================================
    [Fact]
    public async Task CreateOrder_SendsNotification_ViaStubService()
    {
        // Arrange
        _fx.Notifications.Sent.Clear();
        var dto = new CreateOrderDto { CustomerId = 2, Amount = 1000m };

        // Act
        var createResponse = await _fx.Orders.CreateAsync(dto);

        // Assert
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        _fx.Notifications.Sent.Should().HaveCount(1);
        _fx.Notifications.Sent[0].CustomerId.Should().Be(2);
        _fx.Notifications.Sent[0].PayableAmount.Should().Be(950m); // 5% скидка, не VIP
    }

    // ============================================================
    // ТЕСТ 6: заблокированный клиент через TestCustomerService → 403
    // ============================================================
    [Fact]
    public async Task CreateOrder_SuspendedCustomer_Returns403()
    {
        // Arrange
        const int suspendedId = 501;
        _fx.Customers.SuspendedCustomerIds.Add(suspendedId);
        var dto = new CreateOrderDto { CustomerId = suspendedId, Amount = 1000m };

        // Act
        var response = await _fx.Orders.CreateAsync(dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
