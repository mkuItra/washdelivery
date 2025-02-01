namespace WashDelivery.Domain.Common;

public abstract class Entity
{
    public string Id { get; protected set; } = string.Empty;
} 