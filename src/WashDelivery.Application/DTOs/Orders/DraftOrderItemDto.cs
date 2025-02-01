namespace WashDelivery.Application.DTOs.Orders;

public class DraftOrderItemDto
{
    public string ServiceId { get; set; } = null!;
    public string Name { get; set; } = null!;
    public decimal Price { get; set; }
    public decimal? Weight { get; set; }
    public bool IsExtra { get; set; }
} 