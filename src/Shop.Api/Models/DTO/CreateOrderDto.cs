namespace Shop.Api.Models.DTO;
public class CreateOrderDto
{
    public int CustomerId { get; set; }
    public decimal Amount { get; set; }
}