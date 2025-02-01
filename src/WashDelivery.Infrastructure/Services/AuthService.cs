using Microsoft.AspNetCore.Identity;
using WashDelivery.Application.DTOs.Auth;
using WashDelivery.Application.Interfaces;
using WashDelivery.Domain.Constants;
using WashDelivery.Domain.Entities;

namespace WashDelivery.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly UserManager<User> _userManager;
    private readonly ILaundryRepository _laundryRepository;

    public AuthService(
        IUserRepository userRepository,
        UserManager<User> userManager,
        ILaundryRepository laundryRepository)
    {
        _userRepository = userRepository;
        _userManager = userManager;
        _laundryRepository = laundryRepository;
    }

    public async Task<(bool success, string[] errors)> RegisterCustomerAsync(RegisterCustomerDto dto)
    {
        var customer = new Customer(
            email: dto.Email,
            phoneNumber: dto.PhoneNumber ?? "",
            firstName: dto.FirstName,
            lastName: dto.LastName
        );

        await _userRepository.AddAsync(customer);
        var roleResult = await AssignRoleAsync(customer, Roles.Customer);
        
        return roleResult ? 
            (true, Array.Empty<string>()) : 
            (false, new[] { "Failed to assign customer role" });
    }

    public async Task<(bool success, string[] errors)> RegisterCourierAsync(RegisterCourierDto dto)
    {
        var courier = new Courier(
            email: dto.Email,
            phoneNumber: dto.PhoneNumber ?? "",
            firstName: dto.FirstName,
            lastName: dto.LastName
        );

        await _userRepository.AddAsync(courier);
        var roleResult = await AssignRoleAsync(courier, Roles.Courier);
        return roleResult ? 
            (true, Array.Empty<string>()) : 
            (false, new[] { "Failed to assign courier role" });
    }

    public async Task<(bool success, string[] errors)> RegisterLaundryWorkerAsync(RegisterLaundryWorkerDto dto)
    {
        var laundry = await _laundryRepository.GetByIdAsync(dto.LaundryId);
        if (laundry == null)
            return (false, new[] { "Laundry not found" });

        var worker = new LaundryWorker(
            email: dto.Email,
            phoneNumber: dto.PhoneNumber ?? "",
            firstName: dto.FirstName,
            lastName: dto.LastName,
            laundry: laundry
        );

        await _userRepository.AddAsync(worker);
        var roleResult = await AssignRoleAsync(worker, Roles.LaundryWorker);
        return roleResult ? 
            (true, Array.Empty<string>()) : 
            (false, new[] { "Failed to assign worker role" });
    }

    public async Task<(bool success, string[] errors)> RegisterLaundryManagerAsync(RegisterLaundryWorkerDto dto)
    {
        var laundry = await _laundryRepository.GetByIdAsync(dto.LaundryId);
        if (laundry == null)
            return (false, new[] { "Laundry not found" });

        var manager = new LaundryManager(
            email: dto.Email,
            phoneNumber: dto.PhoneNumber ?? "",
            firstName: dto.FirstName,
            lastName: dto.LastName,
            laundry: laundry
        );

        await _userRepository.AddAsync(manager);
        var roleResult = await AssignRoleAsync(manager, Roles.LaundryManager);
        if (!roleResult)
            return (false, new[] { "Failed to assign manager role" });

        // Add LaundryId claim
        await _userManager.AddClaimAsync(manager, new System.Security.Claims.Claim("LaundryId", dto.LaundryId));

        return (true, Array.Empty<string>());
    }

    public async Task<bool> AssignRoleAsync(User user, string role)
    {
        var roleExists = await _userManager.IsInRoleAsync(user, role);
        if (!roleExists)
        {
            var result = await _userManager.AddToRoleAsync(user, role);
            return result.Succeeded;
        }
        return true;
    }
} 