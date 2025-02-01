namespace WashDelivery.Application.Interfaces;

public interface IGeocodingService
{
    Task<GeoCoordinates?> GetCoordinatesAsync(string address);
}

public record GeoCoordinates(double Latitude, double Longitude); 