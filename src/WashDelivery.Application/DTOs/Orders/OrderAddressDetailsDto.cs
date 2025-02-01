using System;

namespace WashDelivery.Application.DTOs.Orders;

public class OrderAddressDetailsDto
{
    public string PickupAddressId { get; set; } = string.Empty;
    public string DeliveryAddressId { get; set; } = string.Empty;
    public DateTime PickupTime { get; set; }
    public bool LeaveAtDoor { get; set; }
    public string? CourierInstructions { get; set; }
} 