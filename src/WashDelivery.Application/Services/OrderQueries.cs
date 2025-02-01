using System.Linq;
using WashDelivery.Application.DTOs.Orders;
using WashDelivery.Application.Interfaces;
using WashDelivery.Domain.Enums;

namespace WashDelivery.Application.Services;

public class OrderQueries : IOrderQueries
{
    protected readonly IOrderRepository _orderRepository;

    public OrderQueries(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<OrderDto> GetOrderAsync(string orderId)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null)
        {
            throw new InvalidOperationException("Order not found");
        }

        return OrderDto.FromOrder(order);
    }

    public async Task<List<OrderDto>> GetCustomerOrdersAsync(string customerId)
    {
        var orders = await _orderRepository.GetCustomerOrdersAsync(customerId);
        return orders.Select(OrderDto.FromOrder).ToList();
    }

    public async Task<List<OrderDto>> GetLaundryOrdersAsync(string laundryId)
    {
        var orders = await _orderRepository.GetLaundryOrdersAsync(laundryId);
        return orders.Select(OrderDto.FromOrder).ToList();
    }

    public async Task<List<OrderDto>> GetCourierOrdersAsync(string courierId)
    {
        var orders = await _orderRepository.GetCourierOrdersAsync(courierId);
        return orders.Select(OrderDto.FromOrder).ToList();
    }

    public async Task<List<OrderDto>> GetAllAsync()
    {
        var orders = await _orderRepository.GetAllAsync();
        return orders.Select(OrderDto.FromOrder).ToList();
    }

    public async Task<List<OrderDto>> GetPendingOrdersAsync()
    {
        var orders = await _orderRepository.GetPendingOrdersAsync();
        return orders.Select(OrderDto.FromOrder).ToList();
    }
} 