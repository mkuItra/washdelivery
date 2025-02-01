using WashDelivery.Application.DTOs.Orders;

namespace WashDelivery.Web.ViewModels.Orders;

public class CourierCompletedOrdersViewModel
{
    public List<OrderDto> CompletedOrders { get; set; } = new();
} 