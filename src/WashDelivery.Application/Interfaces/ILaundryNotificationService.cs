using WashDelivery.Application.DTOs.Orders;
using WashDelivery.Application.DTOs.Laundries;

namespace WashDelivery.Application.Interfaces;

public interface ILaundryNotificationService
{
    Task NotifyLaundryAsync(OrderDto order, LaundryDto laundry);
    Task<bool> TryNotifyAvailableLaundryAsync(OrderDto order);
    bool CanLaundryAcceptOrder(string orderId, string laundryId);
    Task HandleLaundryDecline(string orderId, string laundryId);
} 