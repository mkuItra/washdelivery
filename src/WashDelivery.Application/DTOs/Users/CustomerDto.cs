namespace WashDelivery.Application.DTOs.Users;

public class CustomerDto : UserDto
{
    public List<CustomerDeliveryAddressDto> DeliveryAddresses { get; set; } = new();
} 