using WashDelivery.Domain.Entities;

namespace WashDelivery.Application.Interfaces;

public interface IRepository<T> where T : IBaseEntity
{
    Task<T?> GetByIdAsync(string id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<T> AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(T entity);
} 