namespace WashDelivery.Application.DTOs.Users;

public class CreateUserResult
{
    public bool Succeeded { get; set; }
    public List<string> Errors { get; set; } = new();
} 