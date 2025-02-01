namespace WashDelivery.Web.ViewModels.Laundry;

public class LaundryDetailsViewModel
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;
    public string ContactPhone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;  // String representation of address
    public bool IsActive { get; set; }
    public double Rating { get; set; }
    public int CurrentLoad { get; set; }
} 