using WashDelivery.Domain.Entities;
using WashDelivery.Domain.Enums;

namespace WashDelivery.Application.Interfaces;

public interface IOrderRepository : IRepository<Order>
{
    Task<IEnumerable<Order>> GetByCustomerIdAsync(string customerId);
    Task<IEnumerable<Order>> GetByLaundryIdAsync(string laundryId);
    Task<IEnumerable<Order>> GetByCourierIdAsync(string courierId);
    Task<List<Order>> GetOrdersByStatusAsync(OrderStatus status);
    Task<List<Order>> GetOrdersByStatusesAsync(IEnumerable<OrderStatus> statuses);
    Task<List<Order>> GetCustomerOrdersAsync(string customerId);
    Task<List<Order>> GetLaundryOrdersAsync(string laundryId);
    Task<List<Order>> GetCourierOrdersAsync(string courierId);
    Task<List<Order>> GetPendingOrdersAsync();
} 