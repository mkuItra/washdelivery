namespace WashDelivery.Domain.Entities;

public class LaundryWorker : User
{
    protected LaundryWorker() { }

    public LaundryWorker(
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