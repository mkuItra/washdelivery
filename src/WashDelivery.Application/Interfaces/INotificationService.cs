using WashDelivery.Domain.Enums;

namespace WashDelivery.Application.Interfaces;

public interface INotificationService
{
    Task SendOrderStatusUpdateAsync(int orderId, OrderStatus newStatus);
    Task SendPickupConfirmationAsync(int orderId, DateTime estimatedPickupTime);
    Task SendDeliveryConfirmationAsync(int orderId, DateTime estimatedDeliveryTime);
    Task SendLaundryAssignmentAsync(int orderId, int laundryId);
} 