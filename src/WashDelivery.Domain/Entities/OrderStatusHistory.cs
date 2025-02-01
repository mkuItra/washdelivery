using WashDelivery.Domain.Enums;

namespace WashDelivery.Domain.Entities;

public class OrderStatusHistory : IBaseEntity
{
    public string Id { get; private set; }
    public string OrderId { get; private set; }
    public Order Order { get; private set; } = null!;
    public OrderStatus Status { get; private set; }
    public string? Note { get; private set; }
    public DateTime ChangedAt { get; private set; }

    protected OrderStatusHistory() { }

    public OrderStatusHistory(
        OrderStatus status,
        string? note = null)
    {
        Id = Guid.NewGuid().ToString();
        Status = status;
        Note = note;
        ChangedAt = DateTime.UtcNow;
    }

    public void SetOrder(Order order)
    {
        Order = order;
        OrderId = order.Id;
    }
} 