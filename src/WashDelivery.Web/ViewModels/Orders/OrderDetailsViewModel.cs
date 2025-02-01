using WashDelivery.Application.DTOs.Orders;
using WashDelivery.Domain.ValueObjects;

namespace WashDelivery.Web.ViewModels.Orders;

public class OrderDetailsViewModel
{
    public string Id { get; set; } = string.Empty;
    public string CustomerId { get; set; } = string.Empty;
    public string? LaundryId { get; set; }
    public string? CourierId { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime PickupTime { get; set; }
    public OrderAddressDto PickupAddress { get; set; } = null!;
    public OrderAddressDto DeliveryAddress { get; set; } = null!;
    public List<OrderItemDetailsDto> Items { get; set; } = new();
    public List<OrderStatusHistoryDto> StatusHistory { get; set; } = new();
    public DateTime CreatedAt { get; set; }

    public OrderDetailsViewModel(OrderDto order)
    {
        Id = order.Id;
        CustomerId = order.CustomerId;
        LaundryId = order.LaundryId;
        CourierId = order.CourierId;
        Status = order.Status.ToString();
        PickupTime = order.PickupTime;
        PickupAddress = new OrderAddressDto
        {
            Street = order.PickupAddress.Street,
            BuildingNumber = order.PickupAddress.BuildingNumber,
            ApartmentNumber = order.PickupAddress.ApartmentNumber,
            City = order.PickupAddress.City,
            PostalCode = order.PickupAddress.PostalCode,
            Latitude = (decimal)order.PickupAddress.Latitude,
            Longitude = (decimal)order.PickupAddress.Longitude,
            AdditionalInstructions = order.PickupAddress.AdditionalInstructions
        };
        DeliveryAddress = new OrderAddressDto
        {
            Street = order.DeliveryAddress.Street,
            BuildingNumber = order.DeliveryAddress.BuildingNumber,
            ApartmentNumber = order.DeliveryAddress.ApartmentNumber,
            City = order.DeliveryAddress.City,
            PostalCode = order.DeliveryAddress.PostalCode,
            Latitude = (decimal)order.DeliveryAddress.Latitude,
            Longitude = (decimal)order.DeliveryAddress.Longitude,
            AdditionalInstructions = order.DeliveryAddress.AdditionalInstructions
        };
        Items = order.Items;
        StatusHistory = order.StatusHistory;
        CreatedAt = order.CreatedAt;
    }
} 