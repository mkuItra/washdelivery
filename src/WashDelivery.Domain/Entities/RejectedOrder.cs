namespace WashDelivery.Domain.Entities;

public class RejectedOrder
{
    public string Id { get; private set; }
    public string CourierId { get; private set; }
    public string OrderId { get; private set; }
    public DateTime RejectedAt { get; private set; }

    private RejectedOrder() { }

    public RejectedOrder(string courierId, string orderId)
    {
        Id = Guid.NewGuid().ToString();
        CourierId = courierId;
        OrderId = orderId;
        RejectedAt = DateTime.UtcNow;
    }
} 