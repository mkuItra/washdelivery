using WashDelivery.Domain.Enums;

namespace WashDelivery.Application.DTOs.Orders;

public class UpdateOrderStatusDto
{
    public OrderStatus NewStatus { get; set; }
    public string? Comment { get; set; }
} 