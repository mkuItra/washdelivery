using WashDelivery.Application.DTOs.Orders;
using WashDelivery.Domain.Entities;
using WashDelivery.Domain.Enums;

namespace WashDelivery.Application.Interfaces;

public interface IOrderService
{
    Task<OrderDto> CreateOrderAsync(string customerId, CreateOrderDto dto);
    Task<OrderDto> GetOrderAsync(string orderId);
    Task<List<OrderDto>> GetCustomerOrdersAsync(string customerId);
    Task<List<OrderDto>> GetLaundryOrdersAsync(string laundryId);
    Task<List<OrderDto>> GetCourierOrdersAsync(string courierId);
    Task<List<OrderDto>> GetOrdersByStatusAsync(OrderStatus status);
    Task<List<OrderDto>> GetOrdersByStatusesAsync(IEnumerable<OrderStatus> statuses);
    Task<OrderDto> UpdateOrderStatusAsync(string orderId, OrderStatus newStatus, string? comment = null);
    Task<bool> AssignLaundryAsync(string orderId, string laundryId);
    Task<OrderDto> AcceptOrderAsync(string orderId, string laundryId);
    Task<OrderDto> DeclineOrderAsync(string orderId, string laundryId);
    Task<OrderDto> AssignCourierAsync(string orderId, string courierId);
    Task<List<OrderDto>> GetAllAsync();
    Task<List<OrderDto>> GetPendingOrdersAsync();
    Task<OrderDto> StartProcessingAsync(string orderId, string laundryId);
    Task<OrderDto> MarkAsReadyAsync(string orderId, string laundryId);
} 