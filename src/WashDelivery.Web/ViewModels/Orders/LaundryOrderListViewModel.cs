using WashDelivery.Application.DTOs.Orders;

namespace WashDelivery.Web.ViewModels.Orders;

public class LaundryOrderListViewModel
{
    public bool IsActive { get; set; }
    public List<OrderDto> PendingOrders { get; set; } = new();
    public List<OrderDto> InTransitOrders { get; set; } = new();
    public List<OrderDto> InProgressOrders { get; set; } = new();
    public List<OrderDto> CompletedOrders { get; set; } = new();
} 