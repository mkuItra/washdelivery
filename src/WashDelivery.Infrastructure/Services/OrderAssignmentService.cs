using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using WashDelivery.Application.DTOs.Orders;
using WashDelivery.Application.Interfaces;
using WashDelivery.Domain.Enums;

namespace WashDelivery.Infrastructure.Services;

public class OrderAssignmentService : IOrderAssignmentService
{
    private readonly IOrderService _orderService;
    private readonly ILaundryNotificationService _laundryNotificationService;
    private readonly ILogger<OrderAssignmentService> _logger;

    public OrderAssignmentService(
        IOrderService orderService,
        ILaundryNotificationService laundryNotificationService,
        ILogger<OrderAssignmentService> logger)
    {
        _orderService = orderService;
        _laundryNotificationService = laundryNotificationService;
        _logger = logger;
    }

    public async Task ProcessPendingOrdersAsync(CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("[Notification Flow] Background service checking for pending orders");
            var pendingOrders = await _orderService.GetPendingOrdersAsync();
            _logger.LogInformation("[Notification Flow] Found {Count} pending orders", pendingOrders.Count);

            foreach (var order in pendingOrders)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                try
                {
                    var notified = await _laundryNotificationService.TryNotifyAvailableLaundryAsync(order);
                    if (!notified)
                    {
                        await _orderService.UpdateOrderStatusAsync(
                            order.Id,
                            OrderStatus.LaundryRejected,
                            "No active laundry available in the system");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[Notification Flow] Failed to process order {OrderId}", order.Id);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Notification Flow] Error in background service");
            throw;
        }
    }

    public async Task ProcessOrderAsync(OrderDto order)
    {
        try
        {
            _logger.LogInformation("[Notification Flow] Processing single order {OrderId}", order.Id);
            
            var notified = await _laundryNotificationService.TryNotifyAvailableLaundryAsync(order);
            if (!notified)
            {
                await _orderService.UpdateOrderStatusAsync(
                    order.Id,
                    OrderStatus.LaundryRejected,
                    "No active laundry available in the system");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Notification Flow] Error processing order {OrderId}", order.Id);
            throw;
        }
    }
} 