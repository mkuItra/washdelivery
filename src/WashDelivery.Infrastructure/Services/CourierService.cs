using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using WashDelivery.Application.DTOs.Couriers;
using WashDelivery.Application.Interfaces;
using WashDelivery.Domain.Constants;
using WashDelivery.Domain.Entities;
using WashDelivery.Domain.Enums;
using WashDelivery.Infrastructure.Data;

namespace WashDelivery.Infrastructure.Services;

public class CourierService : ICourierService
{
    private readonly IUserRepository _userRepository;
    private readonly UserManager<User> _userManager;
    private readonly ILogger<CourierService> _logger;
    private readonly AppDbContext _dbContext;

    public CourierService(
        IUserRepository userRepository,
        UserManager<User> userManager,
        ILogger<CourierService> logger,
        AppDbContext dbContext)
    {
        _userRepository = userRepository;
        _userManager = userManager;
        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task<CourierDto> AssignToCourierAsync(int orderId, int courierId)
    {
        throw new NotImplementedException("Legacy method - use OrderService.AssignCourierAsync instead");
    }

    public async Task<IEnumerable<CourierDto>> GetAvailableCouriersAsync(decimal latitude, decimal longitude)
    {
        try
        {
            var couriers = await _userRepository.GetAllAsync<Courier>();
            var availableCouriers = new List<CourierDto>();

            foreach (var courier in couriers)
            {
                if (await _userManager.IsInRoleAsync(courier, Roles.Courier) && 
                    courier.IsActive)
                {
                    availableCouriers.Add(new CourierDto
                    {
                        Id = courier.Id,
                        Email = courier.Email,
                        PhoneNumber = courier.PhoneNumber,
                        FirstName = courier.FirstName,
                        LastName = courier.LastName,
                        IsActive = courier.IsActive,
                        IsAvailable = courier.IsAvailable
                    });
                }
            }

            _logger.LogInformation("Found {Count} available couriers near coordinates ({Latitude}, {Longitude})", 
                availableCouriers.Count, latitude, longitude);

            return availableCouriers;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting available couriers");
            throw;
        }
    }

    public async Task<CourierDto> UpdateCourierLocationAsync(int courierId, decimal latitude, decimal longitude)
    {
        throw new NotImplementedException("Location tracking not implemented yet");
    }

    public async Task<CourierDto> UpdateCourierAvailabilityAsync(int courierId, bool isAvailable)
    {
        throw new NotImplementedException("Availability updates not implemented yet");
    }

    public async Task<IEnumerable<PendingCourierDto>> GetPendingCouriersAsync()
    {
        var couriers = await _userRepository.GetAllAsync<Courier>();
        var pendingCouriers = new List<PendingCourierDto>();

        foreach (var courier in couriers)
        {
            if (await _userManager.IsInRoleAsync(courier, Roles.Courier) && 
                !courier.IsActive)
            {
                pendingCouriers.Add(new PendingCourierDto
                {
                    Id = courier.Id,
                    Email = courier.Email,
                    PhoneNumber = courier.PhoneNumber,
                    FirstName = courier.FirstName,
                    LastName = courier.LastName,
                    CreatedAt = courier.CreatedAt
                });
            }
        }

        return pendingCouriers;
    }

    public async Task VerifyCourierAsync(string courierId, string adminId)
    {
        var courier = await _userRepository.GetByIdAsync<Courier>(courierId);
        if (courier == null)
            throw new InvalidOperationException("Courier not found");

        courier.Activate();
        await _userRepository.UpdateAsync(courier);
        _logger.LogInformation("Courier {CourierId} verified by admin {AdminId}", courierId, adminId);
    }

    public async Task RejectCourierAsync(string courierId, string adminId)
    {
        var courier = await _userRepository.GetByIdAsync<Courier>(courierId);
        if (courier == null)
            throw new InvalidOperationException("Courier not found");

        courier.Deactivate();
        await _userRepository.UpdateAsync(courier);
        _logger.LogInformation("Courier {CourierId} rejected by admin {AdminId}", courierId, adminId);
    }

    public async Task<CourierDto> GetByIdAsync(string id)
    {
        try
        {
            var courier = await _userRepository.GetByIdAsync<Courier>(id);
            if (courier == null)
                return null;

            return new CourierDto
            {
                Id = courier.Id,
                Email = courier.Email,
                PhoneNumber = courier.PhoneNumber,
                FirstName = courier.FirstName,
                LastName = courier.LastName,
                IsActive = courier.IsActive,
                IsAvailable = courier.IsAvailable
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting courier by ID {CourierId}", id);
            throw;
        }
    }

    public async Task<bool> AddRejectedOrderAsync(string courierId, string orderId)
    {
        try
        {
            var rejectedOrder = new RejectedOrder(courierId, orderId);
            await _dbContext.RejectedOrders.AddAsync(rejectedOrder);
            await _dbContext.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding rejected order {OrderId} for courier {CourierId}", orderId, courierId);
            return false;
        }
    }

    public async Task<List<string>> GetRejectedOrdersAsync(string courierId)
    {
        return await _dbContext.RejectedOrders
            .Where(ro => ro.CourierId == courierId)
            .Select(ro => ro.OrderId)
            .ToListAsync();
    }

    public async Task<bool> AddCompletedOrderAsync(string courierId, string orderId, string comment)
    {
        try
        {
            var courier = await _userManager.FindByIdAsync(courierId);
            if (courier == null || !await _userManager.IsInRoleAsync(courier, Roles.Courier))
            {
                _logger.LogWarning("Courier {CourierId} not found or not a courier", courierId);
                return false;
            }

            var completedOrder = new CourierCompletedOrder
            {
                CourierId = courierId,
                OrderId = orderId,
                CompletedAt = DateTime.UtcNow,
                Comment = comment
            };

            _dbContext.CourierCompletedOrders.Add(completedOrder);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Added completed order {OrderId} for courier {CourierId}", orderId, courierId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding completed order {OrderId} for courier {CourierId}", orderId, courierId);
            return false;
        }
    }
} 