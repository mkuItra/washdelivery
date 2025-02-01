using WashDelivery.Application.DTOs.Auth;
using WashDelivery.Domain.Entities;

namespace WashDelivery.Application.Interfaces;

public interface IAuthService
{
    Task<(bool success, string[] errors)> RegisterCustomerAsync(RegisterCustomerDto dto);
    Task<(bool success, string[] errors)> RegisterCourierAsync(RegisterCourierDto dto);
    Task<(bool success, string[] errors)> RegisterLaundryWorkerAsync(RegisterLaundryWorkerDto dto);
    Task<(bool success, string[] errors)> RegisterLaundryManagerAsync(RegisterLaundryWorkerDto dto);
    Task<bool> AssignRoleAsync(User user, string role);
} 