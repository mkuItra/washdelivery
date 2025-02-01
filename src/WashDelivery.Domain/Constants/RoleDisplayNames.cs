namespace WashDelivery.Domain.Constants;

public static class RoleDisplayNames
{
    private static readonly Dictionary<string, string> _displayNames = new()
    {
        { Roles.Admin, "Administrator" },
        { Roles.Customer, "Klient" },
        { Roles.Courier, "Kurier" },
        { Roles.LaundryWorker, "Pracownik pralni" },
        { Roles.LaundryManager, "Kierownik pralni" }
    };

    public static string GetDisplayName(string role)
    {
        return _displayNames.TryGetValue(role, out var displayName) 
            ? displayName 
            : role;
    }
} 