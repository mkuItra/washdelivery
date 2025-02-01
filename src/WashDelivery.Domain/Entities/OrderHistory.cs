using WashDelivery.Domain.Enums;

namespace WashDelivery.Domain.Entities;

public class OrderHistory : BaseEntity
{
    public string OrderId { get; private set; }
    public Order Order { get; private set; } = null!;
    public OrderStatus Status { get; private set; }
    public string? Comment { get; private set; }
    public DateTime Timestamp { get; private set; }
    public string? UserId { get; private set; }

    protected OrderHistory() { }

    public OrderHistory(
        Order order,
        OrderStatus status,
        string? comment = null,
        string? userId = null)
    {
        Id = Guid.NewGuid().ToString();
        Order = order;
        OrderId = order.Id;
        Status = status;
        Comment = comment;
        Timestamp = DateTime.UtcNow;
        UserId = userId;
    }
} 