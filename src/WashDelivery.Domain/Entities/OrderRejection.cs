using WashDelivery.Domain.Common;
using WashDelivery.Domain.Enums;

namespace WashDelivery.Domain.Entities;

public class OrderRejection : BaseEntity
{
    public string OrderId { get; private set; }
    public Order Order { get; private set; } = null!;
    public string Reason { get; private set; }
    public DateTime RejectedAt { get; private set; }
    public string? RejectedBy { get; private set; }

    protected OrderRejection() { }

    public OrderRejection(
        Order order,
        string reason,
        string? rejectedBy = null)
    {
        Id = Guid.NewGuid().ToString();
        Order = order;
        OrderId = order.Id;
        Reason = reason;
        RejectedAt = DateTime.UtcNow;
        RejectedBy = rejectedBy;
    }
} 