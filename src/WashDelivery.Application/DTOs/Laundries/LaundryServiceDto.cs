namespace WashDelivery.Application.DTOs.Laundries;

public class LaundryServiceDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Unit { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool IsExtraService { get; set; }
} 