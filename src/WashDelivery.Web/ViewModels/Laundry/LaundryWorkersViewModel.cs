namespace WashDelivery.Web.ViewModels.Laundry;

public class LaundryWorkersViewModel
{
    public string LaundryId { get; set; } = string.Empty;
    public string LaundryName { get; set; } = string.Empty;
    public IEnumerable<LaundryWorkerDto> Workers { get; set; } = Array.Empty<LaundryWorkerDto>();
} 