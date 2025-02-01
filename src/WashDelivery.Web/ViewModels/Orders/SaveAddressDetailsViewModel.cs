namespace WashDelivery.Web.ViewModels.Orders;

public class SaveAddressDetailsViewModel
{
    public string PickupAddressId { get; set; } = string.Empty;
    public string DeliveryAddressId { get; set; } = string.Empty;
    public string PickupTimeOption { get; set; } = string.Empty;
    public string? PickupTime { get; set; }
    public bool LeaveAtDoor { get; set; }
    public string? CourierInstructions { get; set; }
} 