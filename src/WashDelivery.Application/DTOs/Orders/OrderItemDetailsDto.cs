using WashDelivery.Domain.Entities;

namespace WashDelivery.Application.DTOs.Orders;

public class OrderItemDetailsDto
{
    public string Id { get; set; } = string.Empty;
    public string ServiceId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal? Weight { get; set; }
    public bool IsExtra { get; set; }
    public int Quantity { get; set; }
    public decimal? TotalPrice => Weight.HasValue ? Price * Weight.Value : null;
} 