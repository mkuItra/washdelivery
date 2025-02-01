using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WashDelivery.Application.Interfaces;
using WashDelivery.Domain.Entities;
using WashDelivery.Domain.Enums;
using WashDelivery.Infrastructure.Data;

namespace WashDelivery.Infrastructure.Data.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly AppDbContext _context;
    private readonly ILogger<OrderRepository> _logger;

    public OrderRepository(AppDbContext context, ILogger<OrderRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Order?> GetByIdAsync(string id)
    {
        return await _context.Orders
            .Include(o => o.Items)
            .Include(o => o.PickupAddress)
            .Include(o => o.StatusHistory)
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task<IEnumerable<Order>> GetAllAsync()
    {
        var orders = await _context.Orders
            .Include(o => o.Items)
            .Include(o => o.PickupAddress)
            .Include(o => o.StatusHistory)
            .ToListAsync();
        return orders;
    }

    public async Task<Order> AddAsync(Order entity)
    {
        await _context.Orders.AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateAsync(Order entity)
    {
        _context.Orders.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Order entity)
    {
        _context.Orders.Remove(entity);
        await _context.SaveChangesAsync();
    }

    // IOrderRepository specific methods
    public async Task<IEnumerable<Order>> GetByCustomerIdAsync(string customerId)
    {
        return await _context.Orders
            .Include(o => o.Items)
            .Include(o => o.PickupAddress)
            .Include(o => o.StatusHistory)
            .Where(o => o.CustomerId == customerId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Order>> GetByLaundryIdAsync(string laundryId)
    {
        return await _context.Orders
            .Include(o => o.Items)
            .Include(o => o.PickupAddress)
            .Include(o => o.StatusHistory)
            .Where(o => o.LaundryId == laundryId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Order>> GetByCourierIdAsync(string courierId)
    {
        return await _context.Orders
            .Include(o => o.Items)
            .Include(o => o.PickupAddress)
            .Include(o => o.StatusHistory)
            .Where(o => o.CourierId == courierId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Order>> GetOrdersByStatusAsync(OrderStatus status)
    {
        return await _context.Orders
            .Include(o => o.Items)
            .Include(o => o.PickupAddress)
            .Include(o => o.DeliveryAddress)
            .Include(o => o.StatusHistory)
            .Where(o => o.Status == status)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Order>> GetOrdersByStatusesAsync(IEnumerable<OrderStatus> statuses)
    {
        return await _context.Orders
            .Include(o => o.Items)
            .Include(o => o.PickupAddress)
            .Include(o => o.DeliveryAddress)
            .Include(o => o.StatusHistory)
            .Where(o => statuses.Contains(o.Status))
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Order>> GetCustomerOrdersAsync(string customerId)
    {
        return await _context.Orders
            .Include(o => o.Items)
            .Include(o => o.PickupAddress)
            .Include(o => o.StatusHistory)
            .Where(o => o.CustomerId == customerId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Order>> GetLaundryOrdersAsync(string laundryId)
    {
        // Get orders that are assigned to this laundry
        var assignedOrders = await _context.Orders
            .Include(o => o.Items)
            .Include(o => o.PickupAddress)
            .Include(o => o.StatusHistory)
            .Where(o => o.LaundryId == laundryId && 
                      (o.Status == OrderStatus.AcceptedByLaundry ||
                       o.Status == OrderStatus.AwaitingPickup ||
                       o.Status == OrderStatus.PickupInProgress ||
                       o.Status == OrderStatus.PickedUp ||
                       o.Status == OrderStatus.InLaundry ||
                       o.Status == OrderStatus.ReadyForDelivery ||
                       o.Status == OrderStatus.OutForDelivery ||
                       o.Status == OrderStatus.Delivered))
            .ToListAsync();

        // Get all pending orders if this is the active laundry
        var laundry = await _context.Laundries.FindAsync(laundryId);
        var pendingOrders = new List<Order>();
        
        if (laundry?.IsActive == true)
        {
            pendingOrders = await _context.Orders
                .Include(o => o.Items)
                .Include(o => o.PickupAddress)
                .Include(o => o.StatusHistory)
                .Where(o => o.Status == OrderStatus.PendingLaundryAssignment)
                .ToListAsync();
        }

        // Combine and order all orders
        return assignedOrders.Concat(pendingOrders)
            .OrderByDescending(o => o.CreatedAt)
            .ToList();
    }

    public async Task<List<Order>> GetCourierOrdersAsync(string courierId)
    {
        return await _context.Orders
            .Include(o => o.Items)
            .Include(o => o.PickupAddress)
            .Include(o => o.StatusHistory)
            .Where(o => o.CourierId == courierId && 
                       !_context.RejectedOrders.Any(ro => ro.OrderId == o.Id && ro.CourierId == courierId))
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Order>> GetPendingOrdersAsync()
    {
        _logger.LogInformation("Fetching pending orders");
        try
        {
            var orders = await _context.Orders
                .Include(o => o.Items)
                .Include(o => o.PickupAddress)
                .Include(o => o.StatusHistory)
                .Where(o => o.Status == OrderStatus.PendingLaundryAssignment)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            _logger.LogInformation("Found {Count} pending orders", orders.Count);
            return orders;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pending orders");
            throw;
        }
    }

    public async Task AcceptOrderAsync(string orderId, string laundryId)
    {
        var order = await _context.Orders.FindAsync(orderId);
        if (order == null)
        {
            throw new ArgumentException("Order not found", nameof(orderId));
        }

        var laundry = await _context.Laundries.FindAsync(laundryId);
        if (laundry?.IsActive != true)
        {
            throw new InvalidOperationException("Only active laundry can accept orders");
        }

        order.AssignLaundry(laundryId);
        order.LaundryAccepted();
        order.AwaitCourierPickup();
        await _context.SaveChangesAsync();
    }

    public async Task DeclineOrderAsync(string orderId, string laundryId)
    {
        var order = await _context.Orders.FindAsync(orderId);
        if (order == null)
        {
            throw new ArgumentException("Order not found", nameof(orderId));
        }

        var laundry = await _context.Laundries.FindAsync(laundryId);
        if (laundry?.IsActive != true)
        {
            throw new InvalidOperationException("Only active laundry can decline orders");
        }

        order.LaundryRejected("Order declined by laundry");
        await _context.SaveChangesAsync();
    }
} 