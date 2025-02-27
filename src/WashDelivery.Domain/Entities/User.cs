using Microsoft.AspNetCore.Identity;

namespace WashDelivery.Domain.Entities;

public class User : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public bool IsActive { get; private set; } = true;
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public string? LaundryId { get; private set; }
    public Laundry? Laundry { get; private set; }
    public string UserType { get; set; } = "User";

    public User() 
    {
        CreatedAt = DateTime.UtcNow;
    }

    public User(string email, string phoneNumber, string firstName, string lastName)
    {
        Email = email;
        UserName = email;
        PhoneNumber = phoneNumber;
        FirstName = firstName;
        LastName = lastName;
        CreatedAt = DateTime.UtcNow;
        IsActive = false; // Will be set to true for admins in the service layer
        UserType = "User"; // Default value, will be updated in service layer
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;

    public void AssignToLaundry(Laundry laundry)
    {
        LaundryId = laundry.Id.ToString();
        Laundry = laundry;
    }

    public void RemoveFromLaundry()
    {
        LaundryId = null;
        Laundry = null;
    }

    public void SetUserType(string userType)
    {
        UserType = userType;
    }
} 