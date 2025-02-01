using Microsoft.EntityFrameworkCore;
using WashDelivery.Application.Interfaces;
using WashDelivery.Domain.Entities;
using WashDelivery.Infrastructure.Repositories;

namespace WashDelivery.Infrastructure.Data.Repositories;

public class AddressRepository : Repository<CustomerDeliveryAddress>, IAddressRepository
{
    public AddressRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<List<CustomerDeliveryAddress>> GetUserAddressesAsync(string userId)
    {
        return await _dbSet
            .Where(a => a.CustomerId == userId)
            .ToListAsync();
    }

    public async Task<CustomerDeliveryAddress?> GetDefaultForCustomerAsync(string customerId)
    {
        return await _dbSet
            .FirstOrDefaultAsync(a => a.CustomerId == customerId && a.IsDefault);
    }

    public override async Task UpdateAsync(CustomerDeliveryAddress address)
    {
        if (address.IsDefault)
        {
            var currentDefault = await GetDefaultForCustomerAsync(address.CustomerId);
            if (currentDefault != null && currentDefault.Id != address.Id)
            {
                currentDefault.SetAsNotDefault();
                _dbSet.Update(currentDefault);
            }
        }

        await base.UpdateAsync(address);
    }
} 