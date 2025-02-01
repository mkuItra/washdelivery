using WashDelivery.Domain.Entities;

namespace WashDelivery.Application.Interfaces;

public interface ILaundryRepository : IRepository<Laundry>
{
    Task<IEnumerable<Laundry>> GetAllAsync();
    Task<Laundry?> GetByIdForUpdateAsync(string id);
} 