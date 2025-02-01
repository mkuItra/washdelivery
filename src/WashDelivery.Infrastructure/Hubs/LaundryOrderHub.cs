using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using WashDelivery.Application.DTOs.Orders;
using WashDelivery.Application.Interfaces;
using WashDelivery.Application.Interfaces.Hubs;
using WashDelivery.Domain.Constants;
using System.Security.Claims;

namespace WashDelivery.Infrastructure.Hubs;

[Authorize(Roles = Roles.LaundryManager)]
public class LaundryOrderHub : Hub<ILaundryOrderHubClient>, ILaundryOrderHubServer
{
    private readonly IOrderService _orderService;
    private readonly ILaundryService _laundryService;
    private readonly ILogger<LaundryOrderHub> _logger;

    public LaundryOrderHub(
        IOrderService orderService,
        ILaundryService laundryService,
        ILogger<LaundryOrderHub> logger)
    {
        _orderService = orderService;
        _laundryService = laundryService;
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        try
        {
            _logger.LogInformation("[SignalR] Client connecting. ConnectionId: {ConnectionId}", Context.ConnectionId);
            
            var user = Context.User;
            if (user == null)
            {
                _logger.LogWarning("[SignalR] No user context found for connection {ConnectionId}", Context.ConnectionId);
                throw new HubException("No user context found");
            }

            _logger.LogInformation("[SignalR] User claims for connection {ConnectionId}:", Context.ConnectionId);
            foreach (var claim in user.Claims)
            {
                _logger.LogInformation("[SignalR] Claim: {Type} = {Value}", claim.Type, claim.Value);
            }

            var laundryId = user.FindFirst("LaundryId")?.Value;
            if (string.IsNullOrEmpty(laundryId))
            {
                _logger.LogWarning("[SignalR] No LaundryId claim found for user {UserId} on connection {ConnectionId}. Claims: {@Claims}", 
                    user.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                    Context.ConnectionId,
                    user.Claims.Select(c => new { c.Type, c.Value }));
                throw new HubException("No LaundryId claim found");
            }

            var groupName = $"laundry_{laundryId}";
            _logger.LogInformation("[SignalR] Adding client {ConnectionId} to group {GroupName}. User: {UserId}, LaundryId: {LaundryId}", 
                Context.ConnectionId, 
                groupName,
                user.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                laundryId);

            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            _logger.LogInformation("[SignalR] Successfully added client {ConnectionId} to group {GroupName}", 
                Context.ConnectionId, 
                groupName);

            await base.OnConnectedAsync();
            _logger.LogInformation("[SignalR] Client {ConnectionId} connected successfully", Context.ConnectionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[SignalR] Error connecting client {ConnectionId}", Context.ConnectionId);
            throw;
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        try
        {
            _logger.LogInformation("[SignalR] Client disconnecting. ConnectionId: {ConnectionId}", Context.ConnectionId);
            
            var user = Context.User;
            if (user != null)
            {
                _logger.LogInformation("[SignalR] User claims for disconnecting client {ConnectionId}:", Context.ConnectionId);
                foreach (var claim in user.Claims)
                {
                    _logger.LogInformation("[SignalR] Claim: {Type} = {Value}", claim.Type, claim.Value);
                }

                var laundryId = user.FindFirst("LaundryId")?.Value;
                if (!string.IsNullOrEmpty(laundryId))
                {
                    var groupName = $"laundry_{laundryId}";
                    _logger.LogInformation("[SignalR] Removing client {ConnectionId} from group {GroupName}", 
                        Context.ConnectionId, 
                        groupName);
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
                    _logger.LogInformation("[SignalR] Successfully removed client {ConnectionId} from group {GroupName}", 
                        Context.ConnectionId, 
                        groupName);
                }
                else
                {
                    _logger.LogWarning("[SignalR] No LaundryId claim found for disconnecting user {UserId}. Claims: {@Claims}", 
                        user.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                        user.Claims.Select(c => new { c.Type, c.Value }));
                }
            }

            await base.OnDisconnectedAsync(exception);
            _logger.LogInformation("[SignalR] Client {ConnectionId} disconnected successfully", Context.ConnectionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[SignalR] Error disconnecting client {ConnectionId}", Context.ConnectionId);
            throw;
        }
    }

    public async Task AcceptOrder(string orderId)
    {
        try
        {
            var laundryId = Context.User?.FindFirst("LaundryId")?.Value;
            if (string.IsNullOrEmpty(laundryId))
            {
                await Clients.Caller.OrderError("No laundry assigned to this user");
                return;
            }

            await _orderService.AcceptOrderAsync(orderId, laundryId);
            await Clients.All.OrderAccepted(orderId);
            _logger.LogInformation("[SignalR] Order {OrderId} accepted by laundry {LaundryId}", orderId, laundryId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[SignalR] Error accepting order {OrderId}", orderId);
            await Clients.Caller.OrderError($"Failed to accept order: {ex.Message}");
        }
    }

    public async Task DeclineOrder(string orderId)
    {
        try
        {
            var laundryId = Context.User?.FindFirst("LaundryId")?.Value;
            if (string.IsNullOrEmpty(laundryId))
            {
                await Clients.Caller.OrderError("No laundry assigned to this user");
                return;
            }

            await _orderService.DeclineOrderAsync(orderId, laundryId);
            await Clients.All.OrderDeclined(orderId);
            _logger.LogInformation("[SignalR] Order {OrderId} declined by laundry {LaundryId}", orderId, laundryId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[SignalR] Error declining order {OrderId}", orderId);
            await Clients.Caller.OrderError($"Failed to decline order: {ex.Message}");
        }
    }

    public async Task JoinLaundryGroup()
    {
        try
        {
            _logger.LogInformation("[SignalR] Client {ConnectionId} requesting to join laundry group", Context.ConnectionId);
            
            var user = Context.User;
            if (user == null)
            {
                _logger.LogWarning("[SignalR] No user context found for connection {ConnectionId} during JoinLaundryGroup", 
                    Context.ConnectionId);
                throw new HubException("No user context found");
            }

            var laundryId = user.FindFirst("LaundryId")?.Value;
            if (string.IsNullOrEmpty(laundryId))
            {
                _logger.LogWarning("[SignalR] No LaundryId claim found for user {UserId} during JoinLaundryGroup. Claims: {@Claims}", 
                    user.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                    user.Claims.Select(c => new { c.Type, c.Value }));
                throw new HubException("No LaundryId claim found");
            }

            var groupName = $"laundry_{laundryId}";
            _logger.LogInformation("[SignalR] Adding client {ConnectionId} to group {GroupName} via JoinLaundryGroup. User: {UserId}, LaundryId: {LaundryId}", 
                Context.ConnectionId, 
                groupName,
                user.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                laundryId);

            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            _logger.LogInformation("[SignalR] Successfully added client {ConnectionId} to group {GroupName} via JoinLaundryGroup", 
                Context.ConnectionId, 
                groupName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[SignalR] Error in JoinLaundryGroup for client {ConnectionId}", Context.ConnectionId);
            throw;
        }
    }

    public async Task SendNewOrder(OrderDto order)
    {
        try
        {
            var laundryId = Context.User?.FindFirst("LaundryId")?.Value;
            if (string.IsNullOrEmpty(laundryId))
            {
                await Clients.Caller.OrderError("No laundry assigned to this user");
                return;
            }

            await Clients.Group($"laundry_{laundryId}").ReceiveNewOrder(order);
            _logger.LogInformation("[SignalR] Order {OrderId} sent to laundry {LaundryId}", order.Id, laundryId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[SignalR] Error sending order {OrderId}", order?.Id);
            await Clients.Caller.OrderError($"Failed to send order: {ex.Message}");
        }
    }

    public async Task TestConnection()
    {
        try
        {
            _logger.LogInformation("[SignalR] Testing connection for client {ConnectionId}", Context.ConnectionId);
            
            var user = Context.User;
            if (user == null)
            {
                _logger.LogWarning("[SignalR] No user context found for connection {ConnectionId} during TestConnection", 
                    Context.ConnectionId);
                throw new HubException("No user context found");
            }

            var laundryId = user.FindFirst("LaundryId")?.Value;
            if (string.IsNullOrEmpty(laundryId))
            {
                _logger.LogWarning("[SignalR] No LaundryId claim found for user {UserId} during TestConnection. Claims: {@Claims}", 
                    user.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                    user.Claims.Select(c => new { c.Type, c.Value }));
                throw new HubException("No LaundryId claim found");
            }

            var groupName = $"laundry_{laundryId}";
            _logger.LogInformation("[SignalR] Testing connection for client {ConnectionId} in group {GroupName}", 
                Context.ConnectionId, 
                groupName);

            await Clients.Caller.OrderError("Connection test successful");
            _logger.LogInformation("[SignalR] Successfully sent test message to client {ConnectionId}", Context.ConnectionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[SignalR] Error in TestConnection for client {ConnectionId}", Context.ConnectionId);
            throw;
        }
    }
} 