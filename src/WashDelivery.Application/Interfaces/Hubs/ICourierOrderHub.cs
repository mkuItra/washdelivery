using WashDelivery.Application.DTOs.Orders;

namespace WashDelivery.Application.Interfaces.Hubs;

public interface ICourierOrderHubClient
{
    Task ReceiveNewOrder(OrderDto order);
    Task ReceiveOrderUpdate(OrderDto order);
    Task ReceiveOrderCancellation(string orderId, string reason);
    Task OrderAccepted(string orderId);
    Task OrderDeclined(string orderId);
    Task OrderError(string message);
}

public interface ICourierOrderHubServer
{
    Task AcceptOrder(string orderId);
    Task DeclineOrder(string orderId);
    Task AcceptPickup(string orderId);
    Task RejectPickup(string orderId, string reason);
    Task JoinCourierGroup();
} 