using WashDelivery.Application.DTOs.Users;
using WashDelivery.Application.DTOs.Common;

namespace WashDelivery.Application.Interfaces;

public interface IUserService
{
    Task<UserDto> GetUserByIdAsync(string userId);
    Task<bool> ChangePasswordAsync(string userId, string currentPassword, string newPassword);
    Task<UserProfileDto> GetUserProfileAsync(string userId);
    Task UpdateUserProfileAsync(string userId, UserProfileDto profile);
    Task<IEnumerable<UserListDto>> GetUsersAsync(string? role = null, string? excludeUserEmail = null);
    Task<IEnumerable<string>> GetUserRolesAsync(string userId);
    
    // Consolidated activation methods (removed duplicates)
    Task<bool> DeactivateUserAsync(string userId);
    Task<bool> ActivateUserAsync(string userId);
    Task<CreateUserResult> CreateUserAsync(CreateUserDto dto);
    Task<UpdateUserResult> UpdateUserAsync(UpdateUserDto dto);
    Task<IEnumerable<UserSearchResultDto>> SearchUsersAsync(string searchTerm, string[] roles, bool excludeAssigned = false);
    Task<List<AddressDto>> GetUserAddressesAsync(string userId);
    Task SetAddressAsDefaultAsync(string userId, string addressId);
    Task<bool> AssignLaundryAsync(string userId, string laundryId);
} 