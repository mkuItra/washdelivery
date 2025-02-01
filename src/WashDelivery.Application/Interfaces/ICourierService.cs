using WashDelivery.Application.DTOs.Couriers;
using WashDelivery.Application.DTOs.Orders;
using WashDelivery.Application.DTOs.Users;
using WashDelivery.Domain.Entities;

namespace WashDelivery.Application.Interfaces;

public interface ICourierService
{
    Task<CourierDto> GetByIdAsync(string id);
    Task<CourierDto> AssignToCourierAsync(int orderId, int courierId);
    Task<IEnumerable<CourierDto>> GetAvailableCouriersAsync(decimal latitude, decimal longitude);
    Task<CourierDto> UpdateCourierLocationAsync(int courierId, decimal latitude, decimal longitude);
    Task<CourierDto> UpdateCourierAvailabilityAsync(int courierId, bool isAvailable);
    Task<IEnumerable<PendingCourierDto>> GetPendingCouriersAsync();
    Task VerifyCourierAsync(string courierId, string adminId);
    Task RejectCourierAsync(string courierId, string adminId);
    Task<bool> AddRejectedOrderAsync(string courierId, string orderId);
    Task<bool> AddCompletedOrderAsync(string courierId, string orderId, string comment);
    Task<List<string>> GetRejectedOrdersAsync(string courierId);
} 