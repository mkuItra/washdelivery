using WashDelivery.Domain.Entities;

namespace WashDelivery.Application.Interfaces;

public interface IUserRepository
{
    Task<T?> GetByIdAsync<T>(string id) where T : User;
    Task<T?> GetByEmailAsync<T>(string email) where T : User;
    Task<IEnumerable<T>> GetAllAsync<T>() where T : User;
    Task<bool> IsEmailUniqueAsync(string email);
    Task<T> AddAsync<T>(T user) where T : User;
    Task UpdateAsync<T>(T user) where T : User;
    Task DeleteAsync<T>(T user) where T : User;
    
    // Specific queries if needed
    Task<IEnumerable<LaundryWorker>> GetWorkersByLaundryIdAsync(int laundryId);
    Task<IEnumerable<Courier>> GetAvailableCouriersAsync();
} 