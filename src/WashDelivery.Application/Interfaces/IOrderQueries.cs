using WashDelivery.Application.DTOs.Orders;

namespace WashDelivery.Application.Interfaces;

public interface IOrderQueries
{
    Task<OrderDto> GetOrderAsync(string orderId);
    Task<List<OrderDto>> GetCustomerOrdersAsync(string customerId);
    Task<List<OrderDto>> GetLaundryOrdersAsync(string laundryId);
    Task<List<OrderDto>> GetCourierOrdersAsync(string courierId);
    Task<List<OrderDto>> GetAllAsync();
    Task<List<OrderDto>> GetPendingOrdersAsync();
} 