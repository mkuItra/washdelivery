using WashDelivery.Application.DTOs.Orders;

namespace WashDelivery.Web.ViewModels.Orders;

public class OrderListViewModel
{
    public List<OrderDto> Orders { get; set; } = new();
} 