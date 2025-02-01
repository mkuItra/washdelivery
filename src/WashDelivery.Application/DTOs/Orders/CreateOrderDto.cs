namespace WashDelivery.Application.DTOs.Orders;

public class CreateOrderDto
{
    public string PickupAddressId { get; set; } = string.Empty;
    public string DeliveryAddressId { get; set; } = string.Empty;
    public bool LeaveAtDoor { get; set; }
    public string? CourierInstructions { get; set; }
    public DateTime ScheduledDateTime { get; set; }
    public List<CreateOrderItemDto> Items { get; set; } = new();

    public CreateOrderDto()
    {
    }

    public CreateOrderDto(DateTime pickupTime, string pickupAddressId, string deliveryAddressId, List<CreateOrderItemDto> items)
    {
        ScheduledDateTime = pickupTime;
        PickupAddressId = pickupAddressId;
        DeliveryAddressId = deliveryAddressId;
        Items = items;
    }
}

public class CreateOrderItemDto
{
    public string ServiceId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal? Weight { get; set; }

    public CreateOrderItemDto()
    {
    }

    public CreateOrderItemDto(string serviceId, string name, decimal price)
    {
        ServiceId = serviceId;
        Name = name;
        Price = price;
    }
} 