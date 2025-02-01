namespace WashDelivery.Domain.Entities;

public class Admin : BaseEntity
{
    public string UserId { get; private set; }
    public User User { get; private set; }

    protected Admin() { }

    public Admin(User user)
    {
        User = user;
        UserId = user.Id.ToString();
    }
} 