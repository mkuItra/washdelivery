using WashDelivery.Application.DTOs.Users;
using WashDelivery.Domain.Constants;

namespace WashDelivery.Web.ViewModels.Admin;

public class UsersViewModel
{
    public IEnumerable<UserListDto> Users { get; set; } = Array.Empty<UserListDto>();
    public string? SelectedRole { get; set; }
    public Dictionary<string, string> AvailableRoles { get; } = new()
    {
        { "", "Wszyscy użytkownicy" },
        { Roles.Admin, "Administrator" },
        { Roles.Customer, "Klient" },
        { Roles.Courier, "Kurier" },
        { Roles.LaundryWorker, "Pracownik pralni" },
        { Roles.LaundryManager, "Kierownik pralni" }
    };

    public string GetRoleDisplayName(string role)
    {
        return AvailableRoles.TryGetValue(role, out var displayName) 
            ? displayName 
            : role;
    }
} 