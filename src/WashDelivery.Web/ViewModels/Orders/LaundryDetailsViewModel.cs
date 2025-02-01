using WashDelivery.Application.DTOs.Laundries;
using WashDelivery.Web.Controllers;

namespace WashDelivery.Web.ViewModels.Orders;

public class LaundryDetailsViewModel
{
    public List<LaundryDto> AvailableLaundries { get; set; } = new();
} 