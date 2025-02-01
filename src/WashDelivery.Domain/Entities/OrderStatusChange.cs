using WashDelivery.Domain.Enums;

namespace WashDelivery.Domain.Entities;

public class OrderStatusChange : BaseEntity
{
    public string OrderId { get; private set; }
    public Order Order { get; private set; } = null!;
    public OrderStatus FromStatus { get; private set; }
    public OrderStatus ToStatus { get; private set; }
    public string? Comment { get; private set; }
    public DateTime ChangedAt { get; private set; }
    public string? ChangedBy { get; private set; }

    protected OrderStatusChange() { }

    public OrderStatusChange(
        Order order,
        OrderStatus fromStatus,
        OrderStatus toStatus,
        string? comment = null,
        string? changedBy = null)
    {
        Id = Guid.NewGuid().ToString();
        Order = order;
        OrderId = order.Id;
        FromStatus = fromStatus;
        ToStatus = toStatus;
        Comment = comment;
        ChangedAt = DateTime.UtcNow;
        ChangedBy = changedBy;
    }
} 