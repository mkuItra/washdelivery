using System;

namespace WashDelivery.Domain.Entities;

public class CourierCompletedOrder : BaseEntity
{
    public CourierCompletedOrder()
    {
        Id = Guid.NewGuid().ToString();
    }

    public string CourierId { get; set; } = string.Empty;
    public User Courier { get; set; } = null!;
    public string OrderId { get; set; } = string.Empty;
    public Order Order { get; set; } = null!;
    public DateTime CompletedAt { get; set; }
    public string? Comment { get; set; }
} 