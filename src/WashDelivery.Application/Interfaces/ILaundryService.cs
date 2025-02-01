using WashDelivery.Application.DTOs.Laundries;
using WashDelivery.Application.DTOs.Common;
using WashDelivery.Application.DTOs.Users;
using WashDelivery.Domain.Entities;

namespace WashDelivery.Application.Interfaces;

public interface ILaundryService
{
    Task<List<LaundryDto>> GetAvailableLaundriesAsync();
    Task<IEnumerable<LaundryDto>> GetAllAsync();
    Task<LaundryDto?> GetByIdAsync(string id);
    Task CreateAsync(CreateLaundryDto dto);
    Task UpdateAsync(string id, UpdateLaundryDto dto, string? updatedBy = null);
    Task DeleteAsync(string id);
    Task ActivateAsync(string id);
    Task DeactivateAsync(string id);
    Task<IEnumerable<LaundryWorkerDetailsDto>> GetWorkersAsync(string laundryId);
    Task RemoveWorkerAsync(string laundryId, string userId);
    Task AddWorkerAsync(string laundryId, string userId);
    Task AddWorkersAsync(string laundryId, List<string> userIds);
    Task RemoveWorkersAsync(string laundryId, List<string> userIds);
    Task<LaundryDetailsDto> GetLaundryDetailsAsync(string laundryId);
    Task<IEnumerable<LaundryListDto>> GetLaundriesAsync();
    Task<IEnumerable<Laundry>> GetNearbyLaundriesAsync(double latitude, double longitude);
} 