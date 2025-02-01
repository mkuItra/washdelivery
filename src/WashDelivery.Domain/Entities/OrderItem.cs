using WashDelivery.Domain.Common;
using WashDelivery.Domain.ValueObjects;

namespace WashDelivery.Domain.Entities;

public class OrderItem : IBaseEntity
{
    public string Id { get; private set; }
    public string? OrderId { get; private set; }
    public Order? Order { get; private set; }
    public string? DraftOrderId { get; private set; }
    public DraftOrder? DraftOrder { get; private set; }
    public string ServiceId { get; private set; }
    public string Name { get; private set; }
    public decimal Price { get; private set; }
    public decimal Weight { get; private set; }
    public bool IsExtra { get; private set; }

    protected OrderItem() { }

    public OrderItem(
        string name,
        decimal price,
        decimal weight,
        string serviceId = "",
        bool isExtra = false)
    {
        Id = Guid.NewGuid().ToString();
        ServiceId = serviceId;
        Name = name;
        Price = price;
        Weight = weight;
        IsExtra = isExtra;
    }

    public void SetOrder(Order order)
    {
        Order = order;
        OrderId = order.Id;
        DraftOrder = null;
        DraftOrderId = null;
    }

    public void SetDraftOrder(DraftOrder draftOrder)
    {
        DraftOrder = draftOrder;
        DraftOrderId = draftOrder.Id;
        Order = null;
        OrderId = null;
    }
} 