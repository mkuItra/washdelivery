namespace WashDelivery.Application.DTOs.Orders;

public class OrderAddressDto
{
    public string Street { get; set; } = null!;
    public string BuildingNumber { get; set; } = null!;
    public string? ApartmentNumber { get; set; }
    public string City { get; set; } = null!;
    public string PostalCode { get; set; } = null!;
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public string? AdditionalInstructions { get; set; }
} 