using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using WashDelivery.Application.DTOs.Orders;
using WashDelivery.Application.Interfaces;
using WashDelivery.Application.Interfaces.Hubs;
using WashDelivery.Infrastructure.Hubs;
using WashDelivery.Domain.Enums;

namespace WashDelivery.Infrastructure.Services;

public class CourierNotificationService : ICourierNotificationService
{
    private readonly IHubContext<CourierOrderHubBase, ICourierOrderHubClient> _hubContext;
    private readonly ILogger<CourierNotificationService> _logger;
    private readonly HashSet<string> _notifiedOrderIds = new();
    private readonly Dictionary<string, DateTime> _orderNotificationTimes = new();

    public CourierNotificationService(
        IHubContext<CourierOrderHubBase, ICourierOrderHubClient> hubContext,
        ILogger<CourierNotificationService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task NotifyNewOrderAsync(OrderDto order)
    {
        try
        {
            _logger.LogInformation(
                "[SignalR] Starting courier notification process for order {OrderId}. Status: {Status}, PickupTime: {PickupTime}", 
                order.Id, 
                order.Status,
                order.PickupTime);

            if (order.Status != OrderStatus.AwaitingPickup && order.Status != OrderStatus.ReadyForDelivery)
            {
                _logger.LogWarning(
                    "[SignalR] Cannot notify couriers - order {OrderId} is not in AwaitingPickup or ReadyForDelivery status. Current status: {Status}", 
                    order.Id, 
                    order.Status);
                return;
            }
            
            if (_notifiedOrderIds.Contains(order.Id))
            {
                _logger.LogInformation(
                    "[SignalR] Order {OrderId} was already notified. Checking expiration. Notification time: {NotificationTime}", 
                    order.Id,
                    _orderNotificationTimes.TryGetValue(order.Id, out var time) ? time.ToString() : "unknown");
                
                if (_orderNotificationTimes.TryGetValue(order.Id, out var notificationTime))
                {
                    var timeSinceNotification = DateTime.UtcNow - notificationTime;
                    if (timeSinceNotification.TotalMinutes >= 3)
                    {
                        _logger.LogInformation(
                            "[SignalR] Order {OrderId} notification expired after {Minutes:N1} minutes. Removing tracking", 
                            order.Id,
                            timeSinceNotification.TotalMinutes);
                        _notifiedOrderIds.Remove(order.Id);
                        _orderNotificationTimes.Remove(order.Id);
                    }
                    else
                    {
                        _logger.LogInformation(
                            "[SignalR] Order {OrderId} notification still active ({Minutes:N1} minutes old). Skipping", 
                            order.Id,
                            timeSinceNotification.TotalMinutes);
                        return;
                    }
                }
            }

            _logger.LogInformation(
                "[SignalR] Preparing to notify couriers about order {OrderId}. Details: {@OrderDetails}", 
                order.Id,
                new { 
                    order.Id, 
                    order.Status, 
                    order.PickupTime,
                    PickupAddress = order.PickupAddress,
                    DeliveryAddress = order.DeliveryAddress,
                    order.LeaveAtDoor,
                    order.CourierInstructions
                });

            // Send to all clients first as a test
            _logger.LogInformation("[SignalR] Sending test notification to all clients");
            await _hubContext.Clients.All.ReceiveNewOrder(order);
            _logger.LogInformation("[SignalR] Test notification sent successfully");

            // Send to courier group
            _logger.LogInformation("[SignalR] Sending notification to Courier group");
            await _hubContext.Clients.Group("Courier").ReceiveNewOrder(order);
            _logger.LogInformation("[SignalR] Notification sent to Courier group successfully");
            
            // Track the notification
            _notifiedOrderIds.Add(order.Id);
            _orderNotificationTimes[order.Id] = DateTime.UtcNow;
            
            _logger.LogInformation(
                "[SignalR] Successfully completed notification process for order {OrderId}. Active notifications: {NotificationCount}", 
                order.Id,
                _notifiedOrderIds.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex, 
                "[SignalR] Error notifying couriers about order {OrderId}. Error: {Error}", 
                order.Id,
                new { ex.Message, ex.StackTrace });
            throw;
        }
    }

    public void RemoveOrderNotification(string orderId)
    {
        _notifiedOrderIds.Remove(orderId);
        _orderNotificationTimes.Remove(orderId);
        _logger.LogInformation("[SignalR] Removed order {OrderId} from notification tracking", orderId);
    }
} 