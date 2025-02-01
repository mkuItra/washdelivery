namespace WashDelivery.Application.DTOs.Users;

public class UpdateUserDto
{
    public string Id { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public string? Password { get; set; }
    public string Role { get; set; } = string.Empty;
    public string? LaundryId { get; set; }
} 