using Microsoft.EntityFrameworkCore;
using WashDelivery.Application.Interfaces;
using WashDelivery.Domain.Entities;

namespace WashDelivery.Infrastructure.Data.Repositories;

public class DraftOrderRepository : IDraftOrderRepository
{
    private readonly AppDbContext _dbContext;

    public DraftOrderRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<DraftOrder?> GetByIdAsync(string id)
    {
        return await _dbContext.DraftOrders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id && o.ExpiresAt > DateTime.UtcNow);
    }

    public async Task<DraftOrder?> GetByCustomerIdAsync(string customerId)
    {
        return await _dbContext.DraftOrders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.CustomerId == customerId && o.ExpiresAt > DateTime.UtcNow);
    }

    public async Task<DraftOrder> AddAsync(DraftOrder draftOrder)
    {
        _dbContext.DraftOrders.Add(draftOrder);
        await _dbContext.SaveChangesAsync();
        return draftOrder;
    }

    public async Task UpdateAsync(DraftOrder draftOrder)
    {
        _dbContext.Entry(draftOrder).State = EntityState.Modified;
        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(DraftOrder draftOrder)
    {
        _dbContext.DraftOrders.Remove(draftOrder);
        await _dbContext.SaveChangesAsync();
    }
} 