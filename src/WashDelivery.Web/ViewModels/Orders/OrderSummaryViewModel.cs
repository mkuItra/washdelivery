using WashDelivery.Application.DTOs.Common;

namespace WashDelivery.Web.ViewModels.Orders;

public class OrderSummaryViewModel
{
    public AddressDto PickupAddress { get; set; } = null!;
    public AddressDto DeliveryAddress { get; set; } = null!;
    public DateTime PickupTime { get; set; }
    public bool LeaveAtDoor { get; set; }
    public string? CourierInstructions { get; set; }
    public List<ServiceItemViewModel> Services { get; set; } = new();
} 