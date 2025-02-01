using WashDelivery.Domain.Enums;

namespace WashDelivery.Domain.Entities;

public class Courier : User
{
    public bool IsAvailable { get; set; }

    public Courier(
        string email,
        string phoneNumber,
        string firstName,
        string lastName
    ) : base(email, phoneNumber, firstName, lastName)
    {
        IsAvailable = false;
    }

    protected Courier() { }

    public void SetAvailable() => IsAvailable = true;
    public void SetUnavailable() => IsAvailable = false;
} 