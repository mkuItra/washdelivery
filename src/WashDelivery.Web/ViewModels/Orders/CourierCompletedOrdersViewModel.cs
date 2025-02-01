using WashDelivery.Application.DTOs.Orders;

namespace WashDelivery.Web.ViewModels.Orders;

public class CourierCompletedOrdersViewModel
{
    public List<OrderDto> CompletedOrders { get; set; } = new();
    public decimal TotalEarnings { get; set; }
    public int TotalDeliveries { get; set; }
    public TimeSpan AverageDeliveryTime { get; set; }
} 