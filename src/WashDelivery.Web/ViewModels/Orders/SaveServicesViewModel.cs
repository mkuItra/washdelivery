namespace WashDelivery.Web.ViewModels.Orders;

public class SaveServicesViewModel
{
    public List<ServiceItemViewModel> Items { get; set; } = new();
}

public class ServiceItemViewModel
{
    public string ServiceId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public bool IsExtra { get; set; }
} 