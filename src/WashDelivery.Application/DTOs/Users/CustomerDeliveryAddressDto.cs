using WashDelivery.Application.DTOs.Common;

namespace WashDelivery.Application.DTOs.Users;

public class CustomerDeliveryAddressDto
{
    public string CustomerId { get; set; } = string.Empty;
    public AddressDto Address { get; set; } = null!;
    public bool IsDefault { get; set; }
} 