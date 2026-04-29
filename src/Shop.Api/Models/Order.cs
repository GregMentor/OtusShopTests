namespace Shop.Api.Models;

public class Order
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public decimal Amount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal PayableAmount { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}