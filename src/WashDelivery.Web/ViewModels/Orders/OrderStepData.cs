using WashDelivery.Application.DTOs.Common;

namespace WashDelivery.Web.ViewModels.Orders;

public class AddressStepData
{
    public string PickupAddressId { get; set; } = string.Empty;
    public string DeliveryAddressId { get; set; } = string.Empty;
    public string? PickupDate { get; set; }
    public string? PickupTime { get; set; }
    public bool LeaveAtDoor { get; set; }
    public string? CourierInstructions { get; set; }
}

public class ServicesStepData
{
    public string LaundryId { get; set; } = string.Empty;
    public List<ServiceItem> Items { get; set; } = new();
}

public class ServiceItem
{
    public string ServiceId { get; set; } = string.Empty;
}

public class CreateOrderData
{
    public AddressStepData AddressStepData { get; set; } = null!;
    public ServicesStepData ServicesStepData { get; set; } = null!;
} 