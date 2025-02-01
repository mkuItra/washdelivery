using Microsoft.EntityFrameworkCore;
using WashDelivery.Application.Interfaces;
using WashDelivery.Domain.Entities;
using WashDelivery.Infrastructure.Repositories;

namespace WashDelivery.Infrastructure.Data.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<T?> GetByIdAsync<T>(string id) where T : User
    {
        return await _context.Set<T>().FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<T?> GetByEmailAsync<T>(string email) where T : User
    {
        return await _context.Set<T>().FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<IEnumerable<T>> GetAllAsync<T>() where T : User
    {
        return await _context.Users
            .OfType<T>()
            .Where(u => u.IsActive)
            .ToListAsync();
    }

    public async Task<bool> IsEmailUniqueAsync(string email)
    {
        return !await _context.Users.AnyAsync(u => u.Email == email);
    }

    public async Task<T> AddAsync<T>(T entity) where T : User
    {
        await _context.Set<T>().AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateAsync<T>(T entity) where T : User
    {
        _context.Set<T>().Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync<T>(T entity) where T : User
    {
        _context.Set<T>().Remove(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<LaundryWorker>> GetWorkersByLaundryIdAsync(int laundryId)
    {
        return await _context.Set<LaundryWorker>()
            .Where(u => u.LaundryId == laundryId.ToString())
            .ToListAsync();
    }

    public async Task<IEnumerable<Courier>> GetAvailableCouriersAsync()
    {
        return await _context.Set<Courier>()
            .Where(c => c.IsActive && c.IsAvailable)
            .ToListAsync();
    }
} 