namespace WashDelivery.Domain.Entities;

public abstract class BaseEntity
{
    public string Id { get; protected set; } = null!;
}
