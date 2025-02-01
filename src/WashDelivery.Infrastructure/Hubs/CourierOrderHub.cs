using Microsoft.AspNetCore.SignalR;
using WashDelivery.Application.DTOs.Orders;
using WashDelivery.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using WashDelivery.Application.Interfaces;
using WashDelivery.Application.Interfaces.Hubs;
using WashDelivery.Domain.Enums;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace WashDelivery.Infrastructure.Hubs;

[Authorize(Roles = Roles.Courier)]
public class CourierOrderHub : CourierOrderHubBase
{
    private static readonly Dictionary<string, string> _courierConnections = new();
    private readonly IOrderService _orderService;
    private readonly ICourierService _courierService;
    private readonly ILogger<CourierOrderHub> _logger;

    public CourierOrderHub(
        IOrderService orderService,
        ICourierService courierService,
        ILogger<CourierOrderHub> logger)
    {
        _orderService = orderService;
        _courierService = courierService;
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        try
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var isAuthenticated = Context.User?.Identity?.IsAuthenticated ?? false;
            var roles = Context.User?.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList() ?? new List<string>();

            _logger.LogInformation(
                "[SignalR] Courier connecting. ConnectionId: {ConnectionId}, UserId: {UserId}, IsAuthenticated: {IsAuthenticated}, Roles: {@Roles}",
                Context.ConnectionId,
                userId ?? "null",
                isAuthenticated,
                roles);

            if (userId != null)
            {
                _courierConnections[userId] = Context.ConnectionId;
                _logger.LogInformation(
                    "[SignalR] Added courier to connections dictionary. UserId: {UserId}, ConnectionId: {ConnectionId}, Total connections: {ConnectionCount}",
                    userId,
                    Context.ConnectionId,
                    _courierConnections.Count);
            }
            else
            {
                _logger.LogWarning("[SignalR] No user ID found for connection {ConnectionId}", Context.ConnectionId);
            }

            await base.OnConnectedAsync();
            _logger.LogInformation("[SignalR] Courier connection completed for {ConnectionId}", Context.ConnectionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[SignalR] Error in OnConnectedAsync for connection {ConnectionId}", Context.ConnectionId);
            throw;
        }
    }

    public override async Task JoinCourierGroup()
    {
        try
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var isAuthenticated = Context.User?.Identity?.IsAuthenticated ?? false;
            var isCourier = Context.User?.IsInRole(Roles.Courier) ?? false;

            _logger.LogInformation(
                "[SignalR] Courier attempting to join group. ConnectionId: {ConnectionId}, UserId: {UserId}, IsAuthenticated: {IsAuthenticated}, IsCourier: {IsCourier}",
                Context.ConnectionId,
                userId ?? "null",
                isAuthenticated,
                isCourier);

            if (!isAuthenticated || !isCourier)
            {
                _logger.LogWarning(
                    "[SignalR] Unauthorized join attempt. ConnectionId: {ConnectionId}, IsAuthenticated: {IsAuthenticated}, IsCourier: {IsCourier}",
                    Context.ConnectionId,
                    isAuthenticated,
                    isCourier);
                throw new HubException("Unauthorized");
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, "Courier");
            _logger.LogInformation(
                "[SignalR] Successfully added courier to group. ConnectionId: {ConnectionId}, UserId: {UserId}",
                Context.ConnectionId,
                userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[SignalR] Error in JoinCourierGroup for connection {ConnectionId}", Context.ConnectionId);
            throw;
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        try
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            _logger.LogInformation(
                "[SignalR] Courier disconnecting. ConnectionId: {ConnectionId}, UserId: {UserId}",
                Context.ConnectionId,
                userId ?? "null");

            if (userId != null)
            {
                _courierConnections.Remove(userId);
                _logger.LogInformation(
                    "[SignalR] Removed courier from connections dictionary. UserId: {UserId}, Remaining connections: {ConnectionCount}",
                    userId,
                    _courierConnections.Count);
            }

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "Courier");
            _logger.LogInformation("[SignalR] Removed from Courier group. ConnectionId: {ConnectionId}", Context.ConnectionId);

            await base.OnDisconnectedAsync(exception);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[SignalR] Error in OnDisconnectedAsync for connection {ConnectionId}", Context.ConnectionId);
            throw;
        }
    }

    public override async Task AcceptOrder(string orderId)
    {
        // This method is temporarily disabled as we're using MVC forms for order acceptance
        _logger.LogInformation("[SignalR] AcceptOrder called but disabled - using MVC forms instead");
        return;
    }

    public override async Task DeclineOrder(string orderId)
    {
        try
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("[SignalR] Attempt to decline order without user ID");
                throw new InvalidOperationException("User ID not found");
            }

            // Check if the order is still pending
            var order = await _orderService.GetOrderAsync(orderId);
            if (order.Status != OrderStatus.AwaitingPickup)
            {
                _logger.LogWarning("[SignalR] Courier {UserId} attempted to decline order {OrderId} that is not awaiting pickup", 
                    userId, orderId);
                await Clients.Caller.OrderError("Order is no longer available");
                return;
            }

            await _orderService.UpdateOrderStatusAsync(orderId, OrderStatus.PickupRejected, "Order declined by courier");
            await Clients.Caller.OrderDeclined(orderId);
            await Clients.Group("Courier").ReceiveOrderUpdate(order); // Notify other couriers
            _logger.LogInformation("[SignalR] Courier {UserId} declined order {OrderId}", userId, orderId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[SignalR] Error declining order {OrderId}", orderId);
            await Clients.Caller.OrderError("Failed to decline order");
            throw;
        }
    }

    public override async Task AcceptPickup(string orderId)
    {
        try
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("[SignalR] Attempt to accept pickup without user ID");
                throw new InvalidOperationException("User ID not found");
            }

            _logger.LogInformation("[SignalR] Courier {UserId} accepting pickup for order {OrderId}", userId, orderId);
            
            var order = await _orderService.GetOrderAsync(orderId);
            if (order.CourierId != userId)
            {
                _logger.LogWarning("[SignalR] Courier {UserId} attempted to accept pickup for order {OrderId} assigned to different courier", 
                    userId, orderId);
                throw new InvalidOperationException("Order is not assigned to this courier");
            }

            await _orderService.UpdateOrderStatusAsync(orderId, OrderStatus.PickedUp);
            _logger.LogInformation("[SignalR] Courier {UserId} successfully accepted pickup for order {OrderId}", userId, orderId);
            
            await Clients.Caller.ReceiveOrderUpdate(order);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[SignalR] Error accepting pickup for order {OrderId}", orderId);
            await Clients.Caller.OrderError("Failed to accept pickup");
            throw;
        }
    }

    public override async Task RejectPickup(string orderId, string reason)
    {
        try
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("[SignalR] Attempt to reject pickup without user ID");
                throw new InvalidOperationException("User ID not found");
            }

            _logger.LogInformation("[SignalR] Courier {UserId} rejecting pickup for order {OrderId}. Reason: {Reason}", 
                userId, orderId, reason);
            
            var order = await _orderService.GetOrderAsync(orderId);
            if (order.CourierId != userId)
            {
                _logger.LogWarning("[SignalR] Courier {UserId} attempted to reject pickup for order {OrderId} assigned to different courier", 
                    userId, orderId);
                throw new InvalidOperationException("Order is not assigned to this courier");
            }

            // Remove courier assignment but keep the order status
            order = await _orderService.UpdateOrderStatusAsync(orderId, OrderStatus.AwaitingPickup);
            await _courierService.AddRejectedOrderAsync(userId, orderId);
            _logger.LogInformation("[SignalR] Courier {UserId} successfully rejected pickup for order {OrderId}", userId, orderId);
            
            await Clients.Caller.ReceiveOrderUpdate(order);
            await Clients.Group("Courier").ReceiveOrderUpdate(order); // Notify other couriers that the order is available again
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[SignalR] Error rejecting pickup for order {OrderId}", orderId);
            await Clients.Caller.OrderError("Failed to reject pickup");
            throw;
        }
    }

    public static IEnumerable<string> GetConnectedCourierIds()
    {
        return _courierConnections.Keys;
    }

    public static string? GetConnectionId(string courierId)
    {
        return _courierConnections.TryGetValue(courierId, out var connectionId) ? connectionId : null;
    }
} 