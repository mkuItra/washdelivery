using WashDelivery.Domain.Constants;

namespace WashDelivery.Web.ViewModels.Laundry;

public class LaundryWorkerDto
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    private string _role = string.Empty;
    public string Role 
    { 
        get => _role;
        set => _role = RoleDisplayNames.GetDisplayName(value);
    }
} 