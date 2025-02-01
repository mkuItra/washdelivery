namespace WashDelivery.Application.DTOs.Users;

public class UpdateUserResult
{
    public bool Succeeded { get; set; }
    public List<string> Errors { get; set; } = new();
} 