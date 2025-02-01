using System;
using System.Collections.Generic;
using System.Linq;
using WashDelivery.Domain.Enums;
using WashDelivery.Domain.ValueObjects;

namespace WashDelivery.Domain.Entities;

public class Order : IBaseEntity
{
    public string Id { get; private set; }
    public string CustomerId { get; private set; }
    public string? LaundryId { get; private set; }
    public string? CourierId { get; private set; }
    public OrderStatus Status { get; private set; }
    public DateTime PickupTime { get; private set; }
    public OrderAddress PickupAddress { get; private set; }
    public OrderAddress DeliveryAddress { get; private set; }
    public bool LeaveAtDoor { get; private set; }
    public string? CourierInstructions { get; private set; }
    public decimal FinalPrice { get; private set; }
    public decimal DeliveryFee { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? DeliveredAt { get; private set; }
    public DateTime? AcceptedByLaundryAt { get; private set; }
    private readonly List<OrderItem> _items = new();
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();
    private readonly List<OrderStatusHistory> _statusHistory = new();
    public IReadOnlyCollection<OrderStatusHistory> StatusHistory => _statusHistory.AsReadOnly();

    protected Order() { }

    public Order(
        string customerId,
        OrderAddress pickupAddress,
        OrderAddress deliveryAddress,
        DateTime pickupTime,
        bool leaveAtDoor,
        string? courierInstructions)
    {
        Id = Guid.NewGuid().ToString();
        CustomerId = customerId;
        PickupTime = pickupTime;
        PickupAddress = pickupAddress;
        DeliveryAddress = deliveryAddress;
        LeaveAtDoor = leaveAtDoor;
        CourierInstructions = courierInstructions;
        Status = OrderStatus.PendingLaundryAssignment;
        CreatedAt = DateTime.UtcNow;
        DeliveryFee = CalculateDeliveryFee();
        AddStatusHistory(Status, "Order created and waiting for laundry assignment");
    }

    private decimal CalculateDeliveryFee()
    {
        // Base delivery fee
        decimal fee = 15.00m;

        // TODO: Add distance-based calculation when we implement proper distance calculation
        // For now, return the base fee
        return fee;
    }

    public void AssignLaundry(string laundryId)
    {
        LaundryId = laundryId;
        Status = OrderStatus.LaundryAssigned;
        AddStatusHistory(Status, $"Order assigned to laundry {laundryId}");
    }

    public void LaundryAccepted()
    {
        Status = OrderStatus.AcceptedByLaundry;
        AcceptedByLaundryAt = DateTime.UtcNow;
        AddStatusHistory(Status, $"Order accepted by laundry {LaundryId}");
    }

    public void LaundryRejected(string reason)
    {
        AddStatusHistory(OrderStatus.LaundryRejected, reason);
        Status = OrderStatus.LaundryRejected;
    }

    public void AwaitCourierPickup()
    {
        AddStatusHistory(OrderStatus.AwaitingPickup, "Order ready for courier pickup");
        Status = OrderStatus.AwaitingPickup;
    }

    public void AssignCourier(string courierId)
    {
        CourierId = courierId;
        if (Status == OrderStatus.ReadyForDelivery)
        {
            Status = OrderStatus.OutForDelivery;
            AddStatusHistory(Status, $"Courier {courierId} assigned to deliver order");
        }
        else
        {
            Status = OrderStatus.PickupInProgress;
            AddStatusHistory(Status, $"Courier {courierId} assigned to pick up order");
        }
    }

    public void PickupRejected(string reason)
    {
        CourierId = null;
        Status = OrderStatus.PickupRejected;
        AddStatusHistory(Status, reason);
    }

    public void PickedUp()
    {
        Status = OrderStatus.PickedUp;
        AddStatusHistory(Status, "Order picked up by courier");
    }

    public void InLaundry()
    {
        Status = OrderStatus.InLaundry;
        AddStatusHistory(Status, "Order received by laundry");
    }

    public void ReadyForDelivery()
    {
        Status = OrderStatus.ReadyForDelivery;
        AddStatusHistory(Status, "Order ready for delivery");
    }

    public void OutForDelivery()
    {
        Status = OrderStatus.OutForDelivery;
        AddStatusHistory(Status, "Order out for delivery");
    }

    public void DeliveryRejected(string reason)
    {
        Status = OrderStatus.DeliveryRejected;
        AddStatusHistory(Status, reason);
    }

    public void Delivered()
    {
        Status = OrderStatus.Delivered;
        DeliveredAt = DateTime.UtcNow;
        AddStatusHistory(Status, "Order delivered");
    }

    public void Cancel(string reason)
    {
        Status = OrderStatus.Cancelled;
        AddStatusHistory(Status, reason);
    }

    public void AddItem(string name, decimal price, decimal weight, string serviceId = "", bool isExtra = false)
    {
        var item = new OrderItem(name, price, weight, serviceId, isExtra);
        item.SetOrder(this);
        _items.Add(item);
        UpdateFinalPrice();
    }

    private void UpdateFinalPrice()
    {
        FinalPrice = _items.Sum(i => i.Price);
    }

    private void AddStatusHistory(OrderStatus status, string note)
    {
        var history = new OrderStatusHistory(status, note);
        history.SetOrder(this);
        _statusHistory.Add(history);
    }

    public void StartProcessing()
    {
        if (Status != OrderStatus.AcceptedByLaundry)
            throw new InvalidOperationException($"Cannot start processing order in {Status} status");

        Status = OrderStatus.InLaundry;
        AddStatusHistory(Status, "Order processing started");
    }

    public void MarkAsReady()
    {
        if (Status != OrderStatus.InLaundry)
            throw new InvalidOperationException($"Cannot mark as ready order in {Status} status");

        Status = OrderStatus.ReadyForDelivery;
        AddStatusHistory(Status, "Order ready for delivery");
    }

    public void UnassignCourier()
    {
        CourierId = null;
        AddStatusHistory(Status, "Courier unassigned after completing delivery");
    }
} 