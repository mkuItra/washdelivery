using WashDelivery.Domain.Enums;
using WashDelivery.Domain.ValueObjects;

namespace WashDelivery.Domain.Entities;

public class DraftOrder : IBaseEntity
{
    public string Id { get; private set; }
    public string CustomerId { get; private set; }
    public OrderStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime LastModified { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public OrderAddress? PickupAddress { get; private set; }
    public OrderAddress? DeliveryAddress { get; private set; }
    public DateTime? PickupTime { get; private set; }
    public bool LeaveAtDoor { get; private set; }
    public string? CourierInstructions { get; private set; }
    private readonly List<OrderItem> _items = new();
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    protected DraftOrder() { }

    public DraftOrder(string customerId)
    {
        Id = Guid.NewGuid().ToString();
        CustomerId = customerId;
        Status = OrderStatus.Draft;
        CreatedAt = DateTime.UtcNow;
        LastModified = DateTime.UtcNow;
        ExpiresAt = DateTime.UtcNow.AddHours(24);
    }

    public void UpdateAddressDetails(
        OrderAddress pickupAddress,
        OrderAddress deliveryAddress,
        DateTime? pickupTime,
        bool leaveAtDoor,
        string? courierInstructions)
    {
        PickupAddress = pickupAddress;
        DeliveryAddress = deliveryAddress;
        PickupTime = pickupTime;
        LeaveAtDoor = leaveAtDoor;
        CourierInstructions = courierInstructions;
        LastModified = DateTime.UtcNow;
    }

    public void UpdateServices(List<OrderItem> items)
    {
        _items.Clear();
        foreach (var item in items)
        {
            item.SetDraftOrder(this);
            _items.Add(item);
        }
        LastModified = DateTime.UtcNow;
    }

    public Order ToOrder()
    {
        if (PickupAddress == null || DeliveryAddress == null || !PickupTime.HasValue)
        {
            throw new InvalidOperationException("Cannot create order: missing required address or pickup time details");
        }

        if (!_items.Any())
        {
            throw new InvalidOperationException("Cannot create order: no items added");
        }

        var order = new Order(
            CustomerId,
            PickupAddress,
            DeliveryAddress,
            PickupTime.Value,
            LeaveAtDoor,
            CourierInstructions);

        foreach (var item in _items)
        {
            order.AddItem(item.Name, item.Price, item.Weight, item.ServiceId, item.IsExtra);
        }

        return order;
    }

    public void AddItem(string name, decimal price, decimal weight)
    {
        var item = new OrderItem(name, price, weight);
        item.SetDraftOrder(this);
        _items.Add(item);
        LastModified = DateTime.UtcNow;
    }

    public void RemoveItem(string itemId)
    {
        var item = _items.FirstOrDefault(i => i.Id == itemId);
        if (item != null)
        {
            _items.Remove(item);
            LastModified = DateTime.UtcNow;
        }
    }

    public void ClearItems()
    {
        _items.Clear();
        LastModified = DateTime.UtcNow;
    }

    public void ExtendExpiration()
    {
        ExpiresAt = DateTime.UtcNow.AddHours(24);
        LastModified = DateTime.UtcNow;
    }
} 