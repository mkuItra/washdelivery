using WashDelivery.Domain.Enums;

namespace WashDelivery.Domain.Constants;

public static class OrderStatusDisplayNames
{
    private static readonly Dictionary<OrderStatus, string> _displayNames = new()
    {
        { OrderStatus.Draft, "Szkic" },
        { OrderStatus.Created, "Utworzone" },
        { OrderStatus.PendingLaundryAssignment, "Nowe zamówienie" },
        { OrderStatus.LaundryAssigned, "Przypisano do pralni" },
        { OrderStatus.AcceptedByLaundry, "Zaakceptowane przez pralnię" },
        { OrderStatus.LaundryRejected, "Odrzucone przez pralnię" },
        { OrderStatus.AwaitingPickup, "Szukamy kuriera" },
        { OrderStatus.PickupInProgress, "Kurier w drodze po odbiór" },
        { OrderStatus.PickupRejected, "Odrzucony odbiór" },
        { OrderStatus.PickedUp, "W drodze do pralni" },
        { OrderStatus.InLaundry, "W pralni" },
        { OrderStatus.ReadyForDelivery, "Gotowe do dostawy" },
        { OrderStatus.OutForDelivery, "Kurier w drodze z dostawą" },
        { OrderStatus.DeliveryRejected, "Odrzucona dostawa" },
        { OrderStatus.Delivered, "Dostarczone" },
        { OrderStatus.Cancelled, "Anulowane" }
    };

    public static string GetDisplayName(OrderStatus status)
    {
        return _displayNames.TryGetValue(status, out var displayName) 
            ? displayName 
            : status.ToString();
    }
} 