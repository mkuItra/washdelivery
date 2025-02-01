using WashDelivery.Application.DTOs.Common;

namespace WashDelivery.Application.DTOs.Laundries;

public class LaundryDetailsDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;
    public string ContactPhone { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public decimal Rating { get; set; }
    public LocationAddressDto Address { get; set; } = null!;
    public List<LaundryServiceDto> Services { get; set; } = new();
    public List<LaundryWorkerDetailsDto> Workers { get; set; } = new();
} 