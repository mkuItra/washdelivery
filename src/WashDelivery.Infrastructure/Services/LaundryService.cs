using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WashDelivery.Application.DTOs.Laundries;
using WashDelivery.Application.DTOs.Common;
using WashDelivery.Application.DTOs.Users;
using WashDelivery.Application.Interfaces;
using WashDelivery.Domain.Entities;
using WashDelivery.Domain.Exceptions;
using WashDelivery.Domain.Constants;
using LocationAddress = WashDelivery.Domain.ValueObjects.LocationAddress;

namespace WashDelivery.Infrastructure.Services;

public class LaundryService : ILaundryService
{
    private readonly ILaundryRepository _laundryRepository;
    private readonly IGeocodingService _geocodingService;
    private readonly ILogger<LaundryService> _logger;
    private readonly UserManager<User> _userManager;

    public LaundryService(
        ILaundryRepository laundryRepository,
        IGeocodingService geocodingService,
        ILogger<LaundryService> logger,
        UserManager<User> userManager)
    {
        _laundryRepository = laundryRepository;
        _geocodingService = geocodingService;
        _logger = logger;
        _userManager = userManager;
    }

    public async Task<List<LaundryDto>> GetAvailableLaundriesAsync()
    {
        _logger.LogInformation("[LaundryService] Getting available laundries");
        var laundries = await _laundryRepository.GetAllAsync();
        _logger.LogInformation("[LaundryService] Found {Count} total laundries", laundries.Count());
        
        var activeLaundries = laundries.Where(l => l.IsActive).ToList();
        _logger.LogInformation("[LaundryService] Found {Count} active laundries. Active laundries: {@Laundries}", 
            activeLaundries.Count,
            activeLaundries.Select(l => new { l.Id, l.Name, l.IsActive }));
            
        return activeLaundries
            .Select(l => new LaundryDto
            {
                Id = l.Id,
                Name = l.Name,
                ContactEmail = l.ContactEmail,
                ContactPhone = l.ContactPhone,
                IsActive = l.IsActive,
                Rating = (decimal)l.Rating,
                Address = new LocationAddressDto
                {
                    Street = l.Address.Street,
                    City = l.Address.City,
                    PostalCode = l.Address.PostalCode,
                    Latitude = Convert.ToDecimal(l.Address.Latitude),
                    Longitude = Convert.ToDecimal(l.Address.Longitude)
                },
                Services = l.Services.Select(s => new LaundryServiceDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    Description = s.Description,
                    Price = s.Price,
                    Unit = "kg", // TODO: Get from service configuration
                    IsActive = s.IsActive,
                    IsExtraService = s.IsExtraService
                }).ToList()
            }).ToList();
    }

    public async Task<IEnumerable<LaundryDto>> GetAllAsync()
    {
        var laundries = await _laundryRepository.GetAllAsync();
        return laundries.Select(l => new LaundryDto
        {
            Id = l.Id,
            Name = l.Name,
            ContactEmail = l.ContactEmail,
            ContactPhone = l.ContactPhone,
            IsActive = l.IsActive,
            Rating = (decimal)l.Rating,
            Address = new LocationAddressDto
            {
                Street = l.Address.Street,
                City = l.Address.City,
                PostalCode = l.Address.PostalCode,
                Latitude = Convert.ToDecimal(l.Address.Latitude),
                Longitude = Convert.ToDecimal(l.Address.Longitude)
            }
        });
    }

    public async Task<LaundryDto?> GetByIdAsync(string id)
    {
        var laundry = await _laundryRepository.GetByIdAsync(id);
        if (laundry == null) return null;

        _logger.LogWarning("DEBUG SERVICE: Raw entity IsActive: {IsActive} ({Type})", 
            laundry.IsActive, laundry.IsActive.GetType());

        var dto = new LaundryDto
        {
            Id = laundry.Id,
            Name = laundry.Name,
            ContactEmail = laundry.ContactEmail,
            ContactPhone = laundry.ContactPhone,
            IsActive = laundry.IsActive,
            Rating = (decimal)laundry.Rating,
            Address = new LocationAddressDto
            {
                Street = laundry.Address.Street,
                City = laundry.Address.City,
                PostalCode = laundry.Address.PostalCode,
                Latitude = Convert.ToDecimal(laundry.Address.Latitude),
                Longitude = Convert.ToDecimal(laundry.Address.Longitude)
            }
        };

        _logger.LogWarning("DEBUG SERVICE: DTO IsActive: {IsActive} ({Type})", 
            dto.IsActive, dto.IsActive.GetType());

        return dto;
    }

    public async Task CreateAsync(CreateLaundryDto dto)
    {
        var coordinates = await _geocodingService.GetCoordinatesAsync(dto.Address);
        
        // Parse address components
        var addressParts = dto.Address.Split(',', StringSplitOptions.TrimEntries);
        var street = addressParts[0];
        var cityPostalCode = addressParts.Length > 1 ? addressParts[1].Split(' ', 2) : new[] { "", "" };
        var postalCode = cityPostalCode.Length > 0 ? cityPostalCode[0] : "";
        var city = cityPostalCode.Length > 1 ? cityPostalCode[1] : "";

        var address = new LocationAddress(
            street: street,
            city: city,
            postalCode: postalCode,
            latitude: coordinates?.Latitude ?? 0,
            longitude: coordinates?.Longitude ?? 0
        );

        var laundry = new Laundry(
            name: dto.Name,
            contactEmail: dto.ContactEmail,
            contactPhone: dto.ContactPhone,
            address: address
        );

        await _laundryRepository.AddAsync(laundry);
    }

    public async Task UpdateAsync(string id, UpdateLaundryDto dto, string? updatedBy = null)
    {
        var laundry = await _laundryRepository.GetByIdForUpdateAsync(id);
        if (laundry == null)
            throw new NotFoundException($"Pralnia o ID {id} nie została znaleziona.");

        if (laundry.Address.ToString() != dto.Address)
        {
            var coordinates = await _geocodingService.GetCoordinatesAsync(dto.Address);
            
            // Parse address components
            var addressParts = dto.Address.Split(',', StringSplitOptions.TrimEntries);
            var street = addressParts[0];
            var cityPostalCode = addressParts.Length > 1 ? addressParts[1].Split(' ', 2) : new[] { "", "" };
            var postalCode = cityPostalCode.Length > 0 ? cityPostalCode[0] : "";
            var city = cityPostalCode.Length > 1 ? cityPostalCode[1] : "";

            laundry.UpdateLocation(
                street,
                city,
                postalCode,
                coordinates?.Latitude ?? 0,
                coordinates?.Longitude ?? 0
            );
        }

        var address = new LocationAddress(
            street: laundry.Address.Street,
            city: laundry.Address.City,
            postalCode: laundry.Address.PostalCode,
            latitude: laundry.Address.Latitude,
            longitude: laundry.Address.Longitude
        );

        laundry.Update(
            name: dto.Name,
            contactEmail: dto.ContactEmail,
            contactPhone: dto.ContactPhone,
            address: address
        );

        await _laundryRepository.UpdateAsync(laundry);
    }

    public async Task DeleteAsync(string id)
    {
        var laundry = await _laundryRepository.GetByIdForUpdateAsync(id);
        if (laundry == null)
            throw new NotFoundException($"Pralnia o ID {id} nie została znaleziona.");

        await _laundryRepository.DeleteAsync(laundry);
    }

    public async Task ActivateAsync(string id)
    {
        var laundry = await _laundryRepository.GetByIdForUpdateAsync(id);
        if (laundry == null)
        {
            _logger.LogWarning("Attempted to activate non-existent laundry with ID {LaundryId}", id);
            throw new NotFoundException($"Pralnia o ID {id} nie została znaleziona.");
        }

        _logger.LogInformation("Activating laundry {LaundryId}. Current status: {IsActive}", id, laundry.IsActive);
        laundry.Activate();
        await _laundryRepository.UpdateAsync(laundry);
        _logger.LogInformation("Laundry {LaundryId} activated successfully. New status: {IsActive}", id, laundry.IsActive);
    }

    public async Task DeactivateAsync(string id)
    {
        var laundry = await _laundryRepository.GetByIdForUpdateAsync(id);
        if (laundry == null)
        {
            _logger.LogWarning("Attempted to deactivate non-existent laundry with ID {LaundryId}", id);
            throw new NotFoundException($"Pralnia o ID {id} nie została znaleziona.");
        }

        _logger.LogInformation("Deactivating laundry {LaundryId}. Current status: {IsActive}", id, laundry.IsActive);
        laundry.Deactivate();
        await _laundryRepository.UpdateAsync(laundry);
        _logger.LogInformation("Laundry {LaundryId} deactivated successfully. New status: {IsActive}", id, laundry.IsActive);
    }

    public async Task<IEnumerable<LaundryWorkerDetailsDto>> GetWorkersAsync(string laundryId)
    {
        var users = await _userManager.Users
            .Where(u => u.LaundryId == laundryId)
            .ToListAsync();

        var result = new List<LaundryWorkerDetailsDto>();
        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            _logger.LogInformation($"User {user.Email} has role: '{roles.FirstOrDefault()}'");
            
            result.Add(new LaundryWorkerDetailsDto
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber ?? string.Empty,
                Role = roles.FirstOrDefault() ?? string.Empty
            });
        }

        return result;
    }

    public async Task RemoveWorkerAsync(string laundryId, string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            throw new NotFoundException($"User with ID {userId} not found");

        if (user.LaundryId != laundryId)
            throw new InvalidOperationException($"User {userId} is not assigned to laundry {laundryId}");

        // Remove LaundryId claim
        var existingClaim = (await _userManager.GetClaimsAsync(user))
            .FirstOrDefault(c => c.Type == "LaundryId");
        if (existingClaim != null)
        {
            await _userManager.RemoveClaimAsync(user, existingClaim);
        }

        // Update user's LaundryId property
        user.RemoveFromLaundry();
        
        // Update security stamp to invalidate all existing sessions
        await _userManager.UpdateSecurityStampAsync(user);
        
        await _userManager.UpdateAsync(user);

        _logger.LogInformation("User {UserId} removed from laundry {LaundryId}", userId, laundryId);
    }

    public async Task AddWorkerAsync(string laundryId, string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            throw new NotFoundException($"User with ID {userId} not found");

        var laundry = await _laundryRepository.GetByIdAsync(laundryId);
        if (laundry == null)
            throw new NotFoundException($"Laundry with ID {laundryId} not found");

        var roles = await _userManager.GetRolesAsync(user);
        if (!roles.Any(r => r == Roles.LaundryWorker || r == Roles.LaundryManager))
        {
            _logger.LogWarning("User {UserId} cannot be assigned to laundry because they don't have the required role", userId);
            throw new InvalidOperationException("User must be a laundry worker or manager to be assigned to a laundry");
        }

        // Remove existing LaundryId claim if any
        var existingClaim = (await _userManager.GetClaimsAsync(user))
            .FirstOrDefault(c => c.Type == "LaundryId");
        if (existingClaim != null)
        {
            await _userManager.RemoveClaimAsync(user, existingClaim);
        }

        // Add new LaundryId claim
        await _userManager.AddClaimAsync(user, new System.Security.Claims.Claim("LaundryId", laundryId));

        // Update user's LaundryId property
        user.AssignToLaundry(laundry);
        await _userManager.UpdateAsync(user);

        _logger.LogInformation("User {UserId} assigned to laundry {LaundryId} with role {Role}", 
            userId, laundryId, roles.FirstOrDefault());
    }

    public async Task AddWorkersAsync(string laundryId, List<string> userIds)
    {
        var laundry = await _laundryRepository.GetByIdAsync(laundryId);
        if (laundry == null)
            throw new NotFoundException("Pralnia nie została znaleziona");

        foreach (var userId in userIds)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                var roles = await _userManager.GetRolesAsync(user);
                if (roles.Any(r => r == Roles.LaundryWorker || r == Roles.LaundryManager))
                {
                    // Remove existing LaundryId claim if any
                    var existingClaim = (await _userManager.GetClaimsAsync(user))
                        .FirstOrDefault(c => c.Type == "LaundryId");
                    if (existingClaim != null)
                    {
                        await _userManager.RemoveClaimAsync(user, existingClaim);
                    }

                    // Add new LaundryId claim
                    await _userManager.AddClaimAsync(user, new System.Security.Claims.Claim("LaundryId", laundryId));

                    // Update user's LaundryId property
                    user.AssignToLaundry(laundry);
                    await _userManager.UpdateAsync(user);

                    _logger.LogInformation("User {UserId} assigned to laundry {LaundryId} with role {Role}", 
                        userId, laundryId, roles.FirstOrDefault());
                }
            }
        }
    }

    public async Task RemoveWorkersAsync(string laundryId, List<string> userIds)
    {
        foreach (var userId in userIds)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null && user.LaundryId == laundryId)
            {
                // Remove LaundryId claim
                var existingClaim = (await _userManager.GetClaimsAsync(user))
                    .FirstOrDefault(c => c.Type == "LaundryId");
                if (existingClaim != null)
                {
                    await _userManager.RemoveClaimAsync(user, existingClaim);
                }

                // Update user's LaundryId property
                user.RemoveFromLaundry();
                
                // Update security stamp to invalidate all existing sessions
                await _userManager.UpdateSecurityStampAsync(user);
                
                await _userManager.UpdateAsync(user);

                _logger.LogInformation("User {UserId} removed from laundry {LaundryId}", userId, laundryId);
            }
        }
    }

    public async Task<LaundryDetailsDto> GetLaundryDetailsAsync(string laundryId)
    {
        var laundry = await _laundryRepository.GetByIdAsync(laundryId);
        if (laundry == null)
            throw new InvalidOperationException("Laundry not found");

        var workers = await GetWorkersAsync(laundryId);

        return new LaundryDetailsDto
        {
            Id = laundry.Id,
            Name = laundry.Name,
            ContactEmail = laundry.ContactEmail,
            ContactPhone = laundry.ContactPhone,
            IsActive = laundry.IsActive,
            Rating = (decimal)laundry.Rating,
            Address = new LocationAddressDto
            {
                Street = laundry.Address.Street,
                City = laundry.Address.City,
                PostalCode = laundry.Address.PostalCode,
                Latitude = (decimal)laundry.Address.Latitude,
                Longitude = (decimal)laundry.Address.Longitude
            },
            Services = laundry.Services.Select(s => new LaundryServiceDto
            {
                Id = s.Id,
                Name = s.Name,
                Description = s.Description,
                Price = s.Price,
                Unit = s.Unit,
                IsActive = s.IsActive,
                IsExtraService = s.IsExtraService
            }).ToList(),
            Workers = workers.ToList()
        };
    }

    public async Task<IEnumerable<LaundryListDto>> GetLaundriesAsync()
    {
        var laundries = await _laundryRepository.GetAllAsync();
        return laundries.Select(l => new LaundryListDto
        {
            Id = l.Id,
            Name = l.Name,
            ContactEmail = l.ContactEmail,
            ContactPhone = l.ContactPhone,
            IsActive = l.IsActive,
            Rating = (decimal)l.Rating,
            Address = new LocationAddressDto
            {
                Street = l.Address.Street,
                City = l.Address.City,
                PostalCode = l.Address.PostalCode,
                Latitude = (decimal)l.Address.Latitude,
                Longitude = (decimal)l.Address.Longitude
            }
        });
    }

    public async Task<IEnumerable<Laundry>> GetNearbyLaundriesAsync(double latitude, double longitude)
    {
        // First get all active laundries from the database
        var allLaundries = await _laundryRepository.GetAllAsync();
        var maxDistance = 10.0; // 10 kilometers radius

        _logger.LogInformation("Searching for laundries near ({Latitude}, {Longitude})", latitude, longitude);
        _logger.LogInformation("Found {Count} total laundries in database", allLaundries.Count());
        
        // Filter active laundries in memory
        var activeLaundries = allLaundries.Where(l => l.IsActive).ToList();
        _logger.LogInformation("Found {Count} active laundries", activeLaundries.Count);

        // Calculate distances in memory
        var laundriesWithDistances = activeLaundries
            .Select(l => new
            {
                Laundry = l,
                Distance = CalculateDistance(latitude, longitude, l.Address.Latitude, l.Address.Longitude)
            })
            .ToList();

        foreach (var l in laundriesWithDistances)
        {
            _logger.LogInformation(
                "Laundry {LaundryName} ({LaundryId}) at ({Latitude}, {Longitude}) is {Distance:F2}km away",
                l.Laundry.Name,
                l.Laundry.Id,
                l.Laundry.Address.Latitude,
                l.Laundry.Address.Longitude,
                l.Distance
            );
        }

        // Filter and order in memory
        var nearbyLaundries = laundriesWithDistances
            .Where(x => x.Distance <= maxDistance)
            .OrderBy(x => x.Distance)
            .Select(x => x.Laundry)
            .Take(5)
            .ToList();

        _logger.LogInformation("Found {Count} laundries within {MaxDistance}km radius", 
            nearbyLaundries.Count, maxDistance);

        return nearbyLaundries;
    }

    private static double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371; // Earth's radius in kilometers

        var dLat = ToRad(lat2 - lat1);
        var dLon = ToRad(lon2 - lon1);

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRad(lat1)) * Math.Cos(ToRad(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return R * c;
    }

    private static double ToRad(double degrees) => degrees * Math.PI / 180;

    private static LaundryDto MapToDto(Laundry laundry)
    {
        return new LaundryDto
        {
            Id = laundry.Id,
            Name = laundry.Name,
            ContactEmail = laundry.ContactEmail,
            ContactPhone = laundry.ContactPhone,
            IsActive = laundry.IsActive,
            Rating = Convert.ToDecimal(laundry.Rating),
            Address = new LocationAddressDto
            {
                Street = laundry.Address.Street,
                City = laundry.Address.City,
                PostalCode = laundry.Address.PostalCode,
                Latitude = Convert.ToDecimal(laundry.Address.Latitude),
                Longitude = Convert.ToDecimal(laundry.Address.Longitude)
            },
            Services = laundry.Services.Select(s => new LaundryServiceDto
            {
                Id = s.Id,
                Name = s.Name,
                Description = s.Description,
                Price = s.Price,
                Unit = s.Unit,
                IsActive = s.IsActive,
                IsExtraService = s.IsExtraService
            }).ToList()
        };
    }
} 