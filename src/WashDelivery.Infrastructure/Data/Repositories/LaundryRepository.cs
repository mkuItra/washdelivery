using Microsoft.EntityFrameworkCore;
using WashDelivery.Application.Interfaces;
using WashDelivery.Domain.Entities;
using WashDelivery.Infrastructure.Repositories;

namespace WashDelivery.Infrastructure.Data.Repositories;

public class LaundryRepository : Repository<Laundry>, ILaundryRepository
{
    private readonly AppDbContext _context;

    public LaundryRepository(AppDbContext context) : base(context)
    {
        _context = context;
    }

    public override async Task<IEnumerable<Laundry>> GetAllAsync()
    {
        return await _dbSet
            .AsNoTracking()
            .Include(l => l.Services)
            .Include(l => l.Address)
            .ToListAsync();
    }

    public override async Task<Laundry?> GetByIdAsync(string id)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(l => l.Services)
            .Include(l => l.Address)
            .FirstOrDefaultAsync(l => l.Id == id);
    }

    public async Task<Laundry?> GetByIdForUpdateAsync(string id)
    {
        return await _dbSet
            .Include(l => l.Services)
            .Include(l => l.Address)
            .FirstOrDefaultAsync(l => l.Id == id);
    }

    public override async Task UpdateAsync(Laundry entity)
    {
        var entry = _context.Entry(entity);
        entry.State = EntityState.Modified;
        
        // Also mark the Address as modified
        var addressEntry = _context.Entry(entity.Address);
        addressEntry.State = EntityState.Modified;

        await _context.SaveChangesAsync();
    }
} 