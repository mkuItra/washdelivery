using Microsoft.EntityFrameworkCore;
using WashDelivery.Domain.Common;

namespace WashDelivery.Domain.ValueObjects;

[Owned]
public class OrderAddress : ValueObject
{
    public string Street { get; init; } = string.Empty;
    public string BuildingNumber { get; init; } = string.Empty;
    public string? ApartmentNumber { get; init; }
    public string City { get; init; } = string.Empty;
    public string PostalCode { get; init; } = string.Empty;
    public decimal Latitude { get; init; }
    public decimal Longitude { get; init; }
    public string? AdditionalInstructions { get; init; }

    private OrderAddress() { } // For EF Core

    public OrderAddress(
        string street,
        string buildingNumber,
        string? apartmentNumber,
        string city,
        string postalCode,
        decimal latitude,
        decimal longitude,
        string? additionalInstructions = null)
    {
        Street = street;
        BuildingNumber = buildingNumber;
        ApartmentNumber = apartmentNumber;
        City = city;
        PostalCode = postalCode;
        Latitude = latitude;
        Longitude = longitude;
        AdditionalInstructions = additionalInstructions;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Street;
        yield return BuildingNumber;
        if (ApartmentNumber != null) yield return ApartmentNumber;
        yield return City;
        yield return PostalCode;
        yield return Latitude;
        yield return Longitude;
        if (AdditionalInstructions != null) yield return AdditionalInstructions;
    }

    public override string ToString() => $"{Street} {BuildingNumber}{(ApartmentNumber != null ? $"/{ApartmentNumber}" : "")}, {PostalCode} {City}";
} 