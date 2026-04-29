using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Shop.Api.Data;
using Shop.Api.Models;
using Shop.Api.Models.DTO;
using Shop.Api.Services;
using Shop.Domain;

namespace Shop.Api.Controllers;

[ApiController]
[Route("api/orders")]
public class OrdersController : ControllerBase
{
    private readonly IOrderRepository _repo;
    private readonly DiscountService _discounts;
    private readonly INotificationService _notifications;
    private readonly ILogger<OrdersController> _log;

    public OrdersController(
        IOrderRepository repo,
        DiscountService discounts,
        INotificationService notifications,
        ILogger<OrdersController> log)
    {
        _repo = repo;
        _discounts = discounts;
        _notifications = notifications;
        _log = log;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOrderDto dto)
    {
        _log.LogInformation("Create order requested: customerId={CustomerId}, amount={Amount}", dto.CustomerId, dto.Amount);

        if (dto.Amount <= 0)
            return BadRequest("Сумма должна быть больше 0");

        decimal discountAmount;
        try
        {
            discountAmount = _discounts.CalculateDiscount(dto.CustomerId, dto.Amount);
        }
        catch (InvalidOperationException ex)
        {
            _log.LogWarning(ex, "Order rejected for customerId={CustomerId}: blocked account", dto.CustomerId);
            return StatusCode(StatusCodes.Status403Forbidden, ex.Message);
        }
        var payableAmount = dto.Amount - discountAmount;

        _log.LogInformation(
            "Discount calculated: discountAmount={DiscountAmount}, payableAmount={PayableAmount}",
            discountAmount,
            payableAmount);

        var order = new Order
        {
            CustomerId = dto.CustomerId,
            Amount = dto.Amount,
            DiscountAmount = discountAmount,
            PayableAmount = payableAmount
        };

        await _repo.AddAsync(order);
        _log.LogInformation("Order saved: orderId={OrderId}", order.Id);

        await _notifications.OrderCreatedAsync(order.Id, order.CustomerId, order.PayableAmount, HttpContext.RequestAborted);
        _log.LogInformation("Notification sent: orderId={OrderId}", order.Id);

        return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        _log.LogInformation("Get order requested: orderId={OrderId}", id);
        var order = await _repo.GetByIdAsync(id);
        _log.LogInformation("Order lookup finished: found={Found}", order is not null);
        return order is null ? NotFound() : Ok(order);
    }
}
