using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using WashDelivery.Application.DTOs.Orders;
using WashDelivery.Application.DTOs.Laundries;
using WashDelivery.Application.Interfaces;
using WashDelivery.Application.Interfaces.Hubs;
using WashDelivery.Domain.Enums;
using WashDelivery.Infrastructure.Hubs;

namespace WashDelivery.Infrastructure.Services;

public class LaundryNotificationService : ILaundryNotificationService
{
    private readonly ILaundryService _laundryService;
    private readonly ILogger<LaundryNotificationService> _logger;
    private readonly IHubContext<LaundryOrderHub, ILaundryOrderHubClient> _laundryHubContext;
    private readonly HashSet<string> _notifiedOrderIds = new();
    private readonly Dictionary<string, DateTime> _orderNotificationTimes = new();

    public LaundryNotificationService(
        ILaundryService laundryService,
        ILogger<LaundryNotificationService> logger,
        IHubContext<LaundryOrderHub, ILaundryOrderHubClient> laundryHubContext)
    {
        _laundryService = laundryService;
        _logger = logger;
        _laundryHubContext = laundryHubContext;
    }

    public async Task NotifyLaundryAsync(OrderDto order, LaundryDto laundry)
    {
        var groupName = $"laundry_{laundry.Id}";
        try
        {
            _logger.LogInformation("[Notification Flow] Sending SignalR notification. Group: {Group}, Order: {OrderId}, Laundry: {LaundryId}", 
                groupName, order.Id, laundry.Id);

            await _laundryHubContext.Clients.Group(groupName).ReceiveNewOrder(order);
            _notifiedOrderIds.Add(order.Id);
            _orderNotificationTimes[order.Id] = DateTime.UtcNow;
            _logger.LogInformation("[Notification Flow] SignalR notification sent successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Notification Flow] SignalR notification failed");
            throw;
        }
    }

    public async Task<bool> TryNotifyAvailableLaundryAsync(OrderDto order)
    {
        try
        {
            _logger.LogInformation("[Notification Flow] Processing order {OrderId}", order.Id);
            
            var laundries = await _laundryService.GetAvailableLaundriesAsync();
            var activeLaundry = laundries.FirstOrDefault(l => l.IsActive);

            if (activeLaundry == null)
            {
                _logger.LogWarning("[Notification Flow] Cannot process order {OrderId} - no active laundry found", order.Id);
                return false;
            }

            _logger.LogInformation("[Notification Flow] Active laundry found for order {OrderId}: {LaundryId}", 
                order.Id, activeLaundry.Id);

            if (!_notifiedOrderIds.Contains(order.Id))
            {
                await NotifyLaundryAsync(order, activeLaundry);
                return true;
            }
            else
            {
                _logger.LogInformation("[Notification Flow] Order {OrderId} was already notified, skipping", order.Id);
                return true;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Notification Flow] Error processing order {OrderId}", order.Id);
            throw;
        }
    }

    public bool CanLaundryAcceptOrder(string orderId, string laundryId)
    {
        return _orderNotificationTimes.TryGetValue(orderId, out var notificationTime) && 
               (DateTime.UtcNow - notificationTime).TotalMinutes < 3;
    }

    public async Task HandleLaundryDecline(string orderId, string laundryId)
    {
        _logger.LogInformation("[Notification Flow] Order {OrderId} declined by laundry {LaundryId}", orderId, laundryId);
        _notifiedOrderIds.Remove(orderId);
        _orderNotificationTimes.Remove(orderId);
    }
} 