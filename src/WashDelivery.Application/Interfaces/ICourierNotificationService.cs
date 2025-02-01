using WashDelivery.Application.DTOs.Orders;

namespace WashDelivery.Application.Interfaces;

public interface ICourierNotificationService
{
    Task NotifyNewOrderAsync(OrderDto order);
} 