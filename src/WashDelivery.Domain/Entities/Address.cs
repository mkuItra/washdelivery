namespace WashDelivery.Domain.Entities;

public class Address : BaseEntity
{
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
    public bool IsDefault { get; private set; }
    public User Customer { get; private set; }

    protected Address() { }

    public Address(
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

    public void SetDefault(bool isDefault)
    {
        IsDefault = isDefault;
    }
} 