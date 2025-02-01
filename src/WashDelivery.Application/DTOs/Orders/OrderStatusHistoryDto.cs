using WashDelivery.Domain.Enums;

namespace WashDelivery.Application.DTOs.Orders;

public class OrderStatusHistoryDto
{
    public string Id { get; set; } = string.Empty;
    public OrderStatus Status { get; set; }
    public string? Comment { get; set; }
    public DateTime Timestamp { get; set; }
} 