using Microsoft.EntityFrameworkCore;

namespace WashDelivery.Domain.ValueObjects;

[Owned]
public record Address
{
    private Address() { } // For EF Core

    public Address(
        string street,
        string city,
        string postalCode,
        double latitude,
        double longitude)
    {
        Street = street;
        City = city;
        PostalCode = postalCode;
        Latitude = latitude;
        Longitude = longitude;
    }

    public string Street { get; private set; }
    public string City { get; private set; }
    public string PostalCode { get; private set; }
    public double Latitude { get; private set; }
    public double Longitude { get; private set; }

    public override string ToString() => $"{Street}, {PostalCode} {City}";
} 