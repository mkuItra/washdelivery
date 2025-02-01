using WashDelivery.Application.DTOs.Orders;

namespace WashDelivery.Web.ViewModels.Orders;

public class CustomerOrderListViewModel
{
    public List<OrderDto> Orders { get; set; } = new();
} 