using Microsoft.EntityFrameworkCore;
using WashDelivery.Application.Interfaces;
using WashDelivery.Domain.Entities;

namespace WashDelivery.Infrastructure.Data.Repositories;

public class CustomerAddressRepository : ICustomerAddressRepository
{
    private readonly AppDbContext _context;

    public CustomerAddressRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<CustomerDeliveryAddress?> GetByIdAsync(string id)
    {
        return await _context.CustomerDeliveryAddresses.FindAsync(id);
    }

    public async Task<List<CustomerDeliveryAddress>> GetByCustomerIdAsync(string customerId)
    {
        return await _context.CustomerDeliveryAddresses
            .Where(a => a.CustomerId == customerId)
            .ToListAsync();
    }

    public async Task<CustomerDeliveryAddress> AddAsync(CustomerDeliveryAddress address)
    {
        await _context.CustomerDeliveryAddresses.AddAsync(address);
        await _context.SaveChangesAsync();
        return address;
    }

    public async Task<CustomerDeliveryAddress> UpdateAsync(CustomerDeliveryAddress address)
    {
        _context.CustomerDeliveryAddresses.Update(address);
        await _context.SaveChangesAsync();
        return address;
    }

    public async Task DeleteAsync(string id)
    {
        var address = await GetByIdAsync(id);
        if (address != null)
        {
            _context.CustomerDeliveryAddresses.Remove(address);
            await _context.SaveChangesAsync();
        }
    }
} 