namespace WashDelivery.Application.DTOs.Common;

public class CreateAddressDto
{
    public string Name { get; set; } = string.Empty;
    public string Street { get; set; } = string.Empty;
    public string BuildingNumber { get; set; } = string.Empty;
    public string? ApartmentNumber { get; set; }
    public string City { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string? AdditionalInstructions { get; set; }
    public bool IsDefault { get; set; }
} 