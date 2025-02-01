namespace WashDelivery.Domain.Entities;

public class CustomerDeliveryAddress : IBaseEntity
{
    public string Id { get; private set; }
    public string CustomerId { get; private set; }
    public string Name { get; private set; }
    public string Street { get; private set; }
    public string BuildingNumber { get; private set; }
    public string? ApartmentNumber { get; private set; }
    public string City { get; private set; }
    public string PostalCode { get; private set; }
    public decimal Latitude { get; private set; }
    public decimal Longitude { get; private set; }
    public string? AdditionalInstructions { get; private set; }
    public bool IsDefault { get; set; }

    protected CustomerDeliveryAddress() { }

    public CustomerDeliveryAddress(
        string customerId,
        string name,
        string street,
        string buildingNumber,
        string? apartmentNumber,
        string city,
        string postalCode,
        decimal latitude,
        decimal longitude,
        string? additionalInstructions = null,
        bool isDefault = false)
    {
        Id = Guid.NewGuid().ToString();
        CustomerId = customerId;
        Name = name;
        Street = street;
        BuildingNumber = buildingNumber;
        ApartmentNumber = apartmentNumber;
        City = city;
        PostalCode = postalCode;
        Latitude = latitude;
        Longitude = longitude;
        AdditionalInstructions = additionalInstructions;
        IsDefault = isDefault;
    }

    public void SetAsDefault() => IsDefault = true;
    public void SetAsNotDefault() => IsDefault = false;

    public void Update(
        string name,
        string street,
        string buildingNumber,
        string? apartmentNumber,
        string city,
        string postalCode,
        decimal latitude,
        decimal longitude,
        string? additionalInstructions,
        bool isDefault)
    {
        Name = name;
        Street = street;
        BuildingNumber = buildingNumber;
        ApartmentNumber = apartmentNumber;
        City = city;
        PostalCode = postalCode;
        Latitude = latitude;
        Longitude = longitude;
        AdditionalInstructions = additionalInstructions;
        IsDefault = isDefault;
    }
} 