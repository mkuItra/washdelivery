namespace WashDelivery.Domain.Entities;

public class Customer : User
{
    public ICollection<CustomerDeliveryAddress> Addresses { get; private set; } = new List<CustomerDeliveryAddress>();
    public decimal Rating { get; private set; }

    protected Customer() { }

    public Customer(
        string email,
        string phoneNumber,
        string firstName,
        string lastName
    ) : base(email, phoneNumber, firstName, lastName)
    {
    }

    public void AddAddress(CustomerDeliveryAddress address)
    {
        Addresses.Add(address);
        
        if (!Addresses.Any(a => a.IsDefault) || address.IsDefault)
        {
            foreach (var addr in Addresses)
            {
                addr.IsDefault = addr == address;
            }
        }
    }

    public void UpdateRating(decimal newRating)
    {
        if (newRating < 0 || newRating > 5)
            throw new ArgumentException("Rating must be between 0 and 5");

        Rating = newRating;
    }
} 