using WashDelivery.Domain.Entities;

namespace WashDelivery.Application.Interfaces;

public interface IDraftOrderRepository
{
    Task<DraftOrder?> GetByIdAsync(string id);
    Task<DraftOrder?> GetByCustomerIdAsync(string customerId);
    Task<DraftOrder> AddAsync(DraftOrder draftOrder);
    Task UpdateAsync(DraftOrder draftOrder);
    Task DeleteAsync(DraftOrder draftOrder);
} 