using WashDelivery.Domain.Common;
using WashDelivery.Domain.ValueObjects;

namespace WashDelivery.Domain.Entities;

public class LaundryService : BaseEntity
{
    public string Id { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public decimal Price { get; private set; }
    public string Unit { get; private set; }
    public bool IsActive { get; private set; }
    public bool IsExtraService { get; private set; }

    protected LaundryService() { }

    public LaundryService(
        string name,
        string description,
        decimal price,
        string unit,
        bool isActive = true,
        bool isExtraService = false)
    {
        Id = Guid.NewGuid().ToString();
        Name = name;
        Description = description;
        Price = price;
        Unit = unit;
        IsActive = isActive;
        IsExtraService = isExtraService;
    }

    public void Update(string name, string description, decimal price)
    {
        Name = name;
        Description = description;
        Price = price;
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
} 