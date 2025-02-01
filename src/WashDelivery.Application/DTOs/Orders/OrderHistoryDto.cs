using WashDelivery.Domain.Enums;

namespace WashDelivery.Application.DTOs.Orders;

public class OrderHistoryDto
{
    public string Id { get; set; } = string.Empty;
    public OrderStatus Status { get; set; }
    public string? Comment { get; set; }
    public DateTime Timestamp { get; set; }
} 