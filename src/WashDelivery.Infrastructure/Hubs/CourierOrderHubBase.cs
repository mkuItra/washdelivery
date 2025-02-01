using Microsoft.AspNetCore.SignalR;
using WashDelivery.Application.Interfaces.Hubs;

namespace WashDelivery.Infrastructure.Hubs;

public abstract class CourierOrderHubBase : Hub<ICourierOrderHubClient>, ICourierOrderHubServer
{
    public abstract Task AcceptOrder(string orderId);
    public abstract Task DeclineOrder(string orderId);
    public abstract Task AcceptPickup(string orderId);
    public abstract Task RejectPickup(string orderId, string reason);
    public abstract Task JoinCourierGroup();
} 