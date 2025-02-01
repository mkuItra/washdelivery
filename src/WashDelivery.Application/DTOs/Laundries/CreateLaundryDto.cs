using WashDelivery.Application.DTOs.Common;

namespace WashDelivery.Application.DTOs.Laundries;

public class CreateLaundryDto
{
    public string Name { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;
    public string ContactPhone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
} 