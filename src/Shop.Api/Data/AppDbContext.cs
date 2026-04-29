using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shop.Api.Models;

namespace Shop.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Order> Orders => Set<Order>();
}

public interface IOrderRepository
{
    Task<Order> AddAsync(Order order);
    Task<Order?> GetByIdAsync(int id);
}

public class OrderRepository : IOrderRepository
{
    private readonly AppDbContext _db;
    private readonly ILogger<OrderRepository> _log;

    public OrderRepository(AppDbContext db, ILogger<OrderRepository> log)
    {
        _db = db;
        _log = log;
    }

    public async Task<Order> AddAsync(Order order)
    {
        _log.LogInformation("Saving order: customerId={CustomerId}, amount={Amount}, payableAmount={PayableAmount}",
            order.CustomerId, order.Amount, order.PayableAmount);
        _db.Orders.Add(order);
        await _db.SaveChangesAsync();
        _log.LogInformation("Order saved with id={OrderId}", order.Id);
        return order;
    }

    public async Task<Order?> GetByIdAsync(int id)
    {
        _log.LogInformation("Finding order by id={OrderId}", id);
        return await _db.Orders.FindAsync(id);
    }
}
