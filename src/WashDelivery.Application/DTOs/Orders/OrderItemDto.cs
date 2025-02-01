namespace WashDelivery.Application.DTOs.Orders;

public class OrderItemDto
{
    public string Id { get; set; } = null!;
    public string Name { get; set; } = null!;
    public decimal Price { get; set; }
    public decimal Weight { get; set; }
    public string ServiceId { get; set; } = null!;
    public int Quantity { get; set; }
} 