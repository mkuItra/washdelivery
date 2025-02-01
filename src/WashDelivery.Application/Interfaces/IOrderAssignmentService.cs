using System.Threading;
using System.Threading.Tasks;
using WashDelivery.Application.DTOs.Orders;

namespace WashDelivery.Application.Interfaces;

public interface IOrderAssignmentService
{
    Task ProcessPendingOrdersAsync(CancellationToken cancellationToken);
    Task ProcessOrderAsync(OrderDto order);
} 