using Microsoft.EntityFrameworkCore;
using WashDelivery.Application.DTOs.Orders;
using WashDelivery.Application.Interfaces;
using WashDelivery.Domain.Entities;
using WashDelivery.Domain.ValueObjects;
using WashDelivery.Infrastructure.Data;

namespace WashDelivery.Infrastructure.Services;

public class DraftOrderService : IDraftOrderService
{
    private readonly AppDbContext _dbContext;
    private readonly ILaundryService _laundryService;

    public DraftOrderService(AppDbContext dbContext, ILaundryService laundryService)
    {
        _dbContext = dbContext;
        _laundryService = laundryService;
    }

    public async Task<DraftOrder> GetOrCreateDraftOrderAsync(string customerId)
    {
        var draftOrder = await _dbContext.DraftOrders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.CustomerId == customerId);

        if (draftOrder == null)
        {
            draftOrder = new DraftOrder(customerId);
            _dbContext.DraftOrders.Add(draftOrder);
            await _dbContext.SaveChangesAsync();
        }

        return draftOrder;
    }

    public async Task UpdateAddressDetailsAsync(
        string draftOrderId,
        OrderAddress pickupAddress,
        OrderAddress deliveryAddress,
        DateTime? pickupTime,
        bool leaveAtDoor,
        string? courierInstructions)
    {
        var draftOrder = await _dbContext.DraftOrders.FindAsync(draftOrderId);
        if (draftOrder == null)
            throw new InvalidOperationException("Draft order not found");

        draftOrder.UpdateAddressDetails(pickupAddress, deliveryAddress, pickupTime, leaveAtDoor, courierInstructions);
        await _dbContext.SaveChangesAsync();
    }

    public async Task UpdateServicesAsync(string draftOrderId, List<DraftOrderItemDto> items)
    {
        var draftOrder = await _dbContext.DraftOrders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == draftOrderId);

        if (draftOrder == null)
            throw new InvalidOperationException("Draft order not found");

        var orderItems = items.Select(item => new OrderItem(
            item.Name,
            item.Price,
            item.Weight ?? 0,
            item.ServiceId,
            item.IsExtra
        )).ToList();

        draftOrder.UpdateServices(orderItems);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<Order> SubmitDraftOrderAsync(string draftOrderId)
    {
        var draftOrder = await _dbContext.DraftOrders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == draftOrderId);

        if (draftOrder == null)
            throw new InvalidOperationException("Draft order not found");

        var order = draftOrder.ToOrder();
        _dbContext.Orders.Add(order);
        _dbContext.DraftOrders.Remove(draftOrder);
        await _dbContext.SaveChangesAsync();

        return order;
    }
} 