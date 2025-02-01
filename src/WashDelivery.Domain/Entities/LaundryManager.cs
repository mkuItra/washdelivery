namespace WashDelivery.Domain.Entities;

public class LaundryManager : User
{
    protected LaundryManager() { }

    public LaundryManager(
        string email,
        string phoneNumber,
        string firstName,
        string lastName,
        Laundry laundry
    ) : base(email, phoneNumber, firstName, lastName)
    {
        AssignToLaundry(laundry);
    }
} 