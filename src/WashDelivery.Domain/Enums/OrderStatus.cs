namespace WashDelivery.Domain.Enums;

public enum OrderStatus
{
    Draft = -1,
    Created = 0,
    PendingLaundryAssignment = 1,
    LaundryAssigned = 2,
    AcceptedByLaundry = 3,
    LaundryRejected = 4,
    AwaitingPickup = 5,
    PickupInProgress = 6,
    PickupRejected = 7,
    PickedUp = 8,
    InLaundry = 9,
    ReadyForDelivery = 10,
    OutForDelivery = 11,
    DeliveryRejected = 12,
    Delivered = 13,
    Cancelled = 14,
    Expired = 15
} 