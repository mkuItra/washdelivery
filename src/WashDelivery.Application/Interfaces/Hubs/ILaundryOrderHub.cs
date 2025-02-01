using WashDelivery.Application.DTOs.Orders;

namespace WashDelivery.Application.Interfaces.Hubs;

/// <summary>
/// Defines methods that can be called from the server to connected clients
/// </summary>
public interface ILaundryOrderHubClient
{
    /// <summary>
    /// Notifies clients about a new order
    /// </summary>
    Task ReceiveNewOrder(OrderDto order);
    
    /// <summary>
    /// Notifies clients that an order has been accepted
    /// </summary>
    Task OrderAccepted(string orderId);
    
    /// <summary>
    /// Notifies clients that an order has been declined
    /// </summary>
    Task OrderDeclined(string orderId);
    
    /// <summary>
    /// Sends error message to clients
    /// </summary>
    Task OrderError(string message);
}

/// <summary>
/// Defines methods that can be called from clients to the server
/// </summary>
public interface ILaundryOrderHubServer
{
    /// <summary>
    /// Called by clients to accept an order
    /// </summary>
    Task AcceptOrder(string orderId);

    /// <summary>
    /// Called by clients to decline an order
    /// </summary>
    Task DeclineOrder(string orderId);

    /// <summary>
    /// Called by clients to join their laundry group
    /// </summary>
    Task JoinLaundryGroup();

    /// <summary>
    /// Called by clients to test the connection
    /// </summary>
    Task TestConnection();
} 