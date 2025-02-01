using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WashDelivery.Application.DTOs.Users;
using WashDelivery.Application.DTOs.Common;
using WashDelivery.Application.Interfaces;
using WashDelivery.Domain.Entities;
using WashDelivery.Domain.Exceptions;
using WashDelivery.Domain.Constants;
using WashDelivery.Infrastructure.Data;

namespace WashDelivery.Infrastructure.Services;

public class UserService : IUserService
{
    private readonly UserManager<User> _userManager;
    private readonly ILogger<UserService> _logger;
    private readonly ILaundryRepository _laundryRepository;
    private readonly AppDbContext _dbContext;
    private readonly IAddressRepository _addressRepository;
    private readonly IUserRepository _userRepository;

    public UserService(
        UserManager<User> userManager,
        ILogger<UserService> logger,
        ILaundryRepository laundryRepository,
        AppDbContext dbContext,
        IAddressRepository addressRepository,
        IUserRepository userRepository)
    {
        _userManager = userManager;
        _logger = logger;
        _laundryRepository = laundryRepository;
        _dbContext = dbContext;
        _addressRepository = addressRepository;
        _userRepository = userRepository;
    }

    public async Task<UserProfileDto> GetUserProfileAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            throw new NotFoundException("User not found");

        return new UserProfileDto
        {
            Email = user.Email ?? string.Empty,
            FirstName = user.FirstName,
            LastName = user.LastName,
            PhoneNumber = user.PhoneNumber ?? string.Empty
        };
    }

    // Implementacja pozostałych metod z interfejsu
    public async Task<UpdateUserResult> UpdateUserAsync(UpdateUserDto dto)
    {
        var user = await _userManager.FindByIdAsync(dto.Id);
        if (user == null)
        {
            return new UpdateUserResult 
            { 
                Succeeded = false,
                Errors = new List<string> { "Użytkownik nie został znaleziony." }
            };
        }

        user.Email = dto.Email;
        user.UserName = dto.Email;
        user.FirstName = dto.FirstName;
        user.LastName = dto.LastName;
        user.PhoneNumber = dto.PhoneNumber;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            return new UpdateUserResult 
            { 
                Succeeded = false,
                Errors = result.Errors.Select(e => e.Description).ToList()
            };
        }

        // Update role if changed
        var currentRoles = await _userManager.GetRolesAsync(user);
        var currentRole = currentRoles.FirstOrDefault();
        
        if (currentRole != dto.Role)
        {
            if (currentRole != null)
            {
                await _userManager.RemoveFromRoleAsync(user, currentRole);
            }
            
            result = await _userManager.AddToRoleAsync(user, dto.Role);
            if (!result.Succeeded)
            {
                return new UpdateUserResult 
                { 
                    Succeeded = false,
                    Errors = result.Errors.Select(e => e.Description).ToList()
                };
            }
        }

        // Update laundry assignment if role requires it
        if (dto.Role == Roles.LaundryWorker || dto.Role == Roles.LaundryManager)
        {
            if (dto.LaundryId != user.LaundryId)
            {
                if (dto.LaundryId != null)
                {
                    var laundry = await _laundryRepository.GetByIdAsync(dto.LaundryId);
                    if (laundry != null)
                    {
                        user.AssignToLaundry(laundry);
                    }
                }
                else
                {
                    user.RemoveFromLaundry();
                }
                await _userManager.UpdateAsync(user);
            }
        }
        else
        {
            if (user.LaundryId != null)
            {
                user.RemoveFromLaundry();
                await _userManager.UpdateAsync(user);
            }
        }

        return new UpdateUserResult { Succeeded = true };
    }

    public async Task<bool> DeactivateUserAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            _logger.LogWarning("User not found: {UserId}", userId);
            return false;
        }

        user.Deactivate();
        
        // Update security stamp to invalidate all existing sessions
        await _userManager.UpdateSecurityStampAsync(user);
        
        var result = await _userManager.UpdateAsync(user);
        return result.Succeeded;
    }

    public async Task<bool> ActivateUserAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            _logger.LogWarning("User not found: {UserId}", userId);
            return false;
        }

        user.Activate();
        var result = await _userManager.UpdateAsync(user);
        return result.Succeeded;
    }

    public async Task<bool> ChangePasswordAsync(string userId, string currentPassword, string newPassword)
    {
        throw new NotImplementedException();
    }

    public async Task UpdateUserProfileAsync(string userId, UserProfileDto profile)
    {
        throw new NotImplementedException();
    }

    public async Task<IEnumerable<UserListDto>> GetUsersAsync(string? role = null, string? excludeUserEmail = null)
    {
        IQueryable<User> query = _userManager.Users;

        // Wyklucz określonego użytkownika
        if (!string.IsNullOrEmpty(excludeUserEmail))
        {
            query = query.Where(u => u.Email != excludeUserEmail);
        }

        var users = await query.ToListAsync();
        var userDtos = new List<UserListDto>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            
            if (!string.IsNullOrEmpty(role) && !roles.Contains(role))
                continue;

            userDtos.Add(new UserListDto
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber ?? string.Empty,
                IsActive = user.IsActive,
                Roles = roles.ToList(),
                CreatedAt = user.CreatedAt
            });
        }

        return userDtos;
    }

    public async Task<IEnumerable<string>> GetUserRolesAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            _logger.LogWarning("User not found: {UserId}", userId);
            return Array.Empty<string>();
        }

        return await _userManager.GetRolesAsync(user);
    }

    public async Task<CreateUserResult> CreateUserAsync(CreateUserDto dto)
    {
        var user = new User(dto.Email, dto.PhoneNumber, dto.FirstName, dto.LastName);
        
        // Set user type based on role
        switch (dto.Role)
        {
            case Roles.Admin:
                user.SetUserType("Admin");
                user.Activate(); // Admins are active by default
                break;
            case Roles.Customer:
                user.SetUserType("Customer");
                break;
            case Roles.Courier:
                user.SetUserType("Courier");
                break;
            case Roles.LaundryWorker:
                user.SetUserType("LaundryWorker");
                break;
            case Roles.LaundryManager:
                user.SetUserType("LaundryManager");
                break;
        }

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
        {
            return new CreateUserResult 
            { 
                Succeeded = false,
                Errors = result.Errors.Select(e => e.Description).ToList()
            };
        }

        result = await _userManager.AddToRoleAsync(user, dto.Role);
        if (!result.Succeeded)
        {
            // If role assignment fails, delete the user
            await _userManager.DeleteAsync(user);
            return new CreateUserResult 
            { 
                Succeeded = false,
                Errors = result.Errors.Select(e => e.Description).ToList()
            };
        }

        return new CreateUserResult { Succeeded = true };
    }

    public async Task<UserDto> GetUserByIdAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            throw new NotFoundException("User not found");

        return new UserDto
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            FirstName = user.FirstName,
            LastName = user.LastName,
            PhoneNumber = user.PhoneNumber ?? string.Empty,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt,
            LaundryId = user.LaundryId?.ToString()
        };
    }

    public async Task<IEnumerable<UserSearchResultDto>> SearchUsersAsync(string searchTerm, string[] roles, bool excludeAssigned = false)
    {
        // Debugowanie
        _logger.LogInformation("Searching users with roles: {Roles}, excludeAssigned: {ExcludeAssigned}, searchTerm: {SearchTerm}", 
            string.Join(", ", roles), excludeAssigned, searchTerm);

        var query = _userManager.Users
            .Where(u => u.IsActive);

        // Filtruj po przypisaniu do pralni
        if (excludeAssigned)
        {
            query = query.Where(u => u.LaundryId == null);
        }

        var users = await query.ToListAsync();
        var result = new List<UserSearchResultDto>();

        // Debugowanie
        _logger.LogInformation("Found {Count} active users before role filtering", users.Count);

        foreach (var user in users)
        {
            var userRoles = await _userManager.GetRolesAsync(user);
            if (userRoles?.Any() != true) continue;
            
            // Debugowanie
            _logger.LogInformation("User {Email} has roles: {Roles}", user.Email, string.Join(", ", userRoles));

            // Sprawdź czy użytkownik ma którąkolwiek z wymaganych ról
            if (userRoles.Any(userRole => roles.Contains(userRole)))
            {
                // Filtruj po wyszukiwaniu tylko jeśli podano termin
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    searchTerm = searchTerm.ToLower();
                    if (!user.Email.ToLower().Contains(searchTerm) && 
                        !user.FirstName.ToLower().Contains(searchTerm) && 
                        !user.LastName.ToLower().Contains(searchTerm))
                    {
                        continue;
                    }
                }

                var role = userRoles.FirstOrDefault() ?? string.Empty;
                result.Add(new UserSearchResultDto
                {
                    Id = user.Id,
                    Email = user.Email ?? string.Empty,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Role = role,
                    RoleDisplay = RoleDisplayNames.GetDisplayName(role)
                });
            }
        }

        // Debugowanie
        _logger.LogInformation("Returning {Count} users after all filtering", result.Count);

        return result;
    }

    public async Task<List<AddressDto>> GetUserAddressesAsync(string userId)
    {
        var addresses = await _addressRepository.GetUserAddressesAsync(userId);
        return addresses.Select(a => new AddressDto
        {
            Id = a.Id,
            Name = a.Name,
            Street = a.Street,
            BuildingNumber = a.BuildingNumber,
            ApartmentNumber = a.ApartmentNumber,
            PostalCode = a.PostalCode,
            City = a.City,
            AdditionalInstructions = a.AdditionalInstructions,
            IsDefault = a.IsDefault,
            Latitude = (double)a.Latitude,
            Longitude = (double)a.Longitude
        }).ToList();
    }

    public async Task SetAddressAsDefaultAsync(string userId, string addressId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            throw new NotFoundException("User not found");

        var addresses = await _addressRepository.GetUserAddressesAsync(userId);
        
        // Remove default flag from all addresses
        foreach (var address in addresses.Where(a => a.IsDefault))
        {
            address.SetAsNotDefault();
            await _addressRepository.UpdateAsync(address);
        }

        // Set new default address
        var newDefaultAddress = addresses.FirstOrDefault(a => a.Id == addressId);
        if (newDefaultAddress != null)
        {
            newDefaultAddress.SetAsDefault();
            await _addressRepository.UpdateAsync(newDefaultAddress);
        }
    }

    public async Task<bool> AssignLaundryAsync(string userId, string laundryId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return false;

        var laundry = await _laundryRepository.GetByIdAsync(laundryId);
        if (laundry == null)
            return false;

        user.AssignToLaundry(laundry);
        await _userManager.UpdateAsync(user);
        return true;
    }
} 