using WashDelivery.Application.DTOs.Laundries;

namespace WashDelivery.Application.DTOs.Users;

public class LaundryManagerDto : UserDto
{
    public LaundryDto Laundry { get; set; } = null!;
} 