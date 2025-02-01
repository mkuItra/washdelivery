using Microsoft.EntityFrameworkCore;
using WashDelivery.Domain.Common;

namespace WashDelivery.Domain.ValueObjects;

[Owned]
public class LocationAddress : ValueObject
{
    public string Street { get; init; } = string.Empty;
    public string City { get; init; } = string.Empty;
    public string PostalCode { get; init; } = string.Empty;
    public double Latitude { get; init; }
    public double Longitude { get; init; }

    private LocationAddress() { } // For EF Core

    public LocationAddress(
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

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Street;
        yield return City;
        yield return PostalCode;
        yield return Latitude;
        yield return Longitude;
    }

    public override string ToString() => $"{Street}, {PostalCode} {City}";
} 