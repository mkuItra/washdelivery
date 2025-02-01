using WashDelivery.Domain.Entities;

namespace WashDelivery.Application.Interfaces;

public interface ILaundryManagerRepository
{
    Task<LaundryManager?> GetByIdAsync(string id);
    Task<IEnumerable<LaundryManager>> GetAllAsync();
    Task<LaundryManager> AddAsync(LaundryManager manager);
    Task UpdateAsync(LaundryManager manager);
    Task DeleteAsync(LaundryManager manager);
} 