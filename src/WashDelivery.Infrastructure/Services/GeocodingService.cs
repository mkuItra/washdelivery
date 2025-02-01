using WashDelivery.Application.Interfaces;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace WashDelivery.Infrastructure.Services;

public class GeocodingService : IGeocodingService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<GeocodingService> _logger;

    public GeocodingService(HttpClient httpClient, ILogger<GeocodingService> logger)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("https://nominatim.openstreetmap.org/");
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "WashDelivery/1.0");
        _logger = logger;
    }

    public async Task<GeoCoordinates?> GetCoordinatesAsync(string address)
    {
        try
        {
            var encodedAddress = Uri.EscapeDataString(address);
            var response = await _httpClient.GetFromJsonAsync<NominatimResponse[]>(
                $"search?format=json&q={encodedAddress}"
            );

            var location = response?.FirstOrDefault();
            if (location == null)
            {
                _logger.LogWarning("No coordinates found for address: {Address}", address);
                return null;
            }

            return new GeoCoordinates(
                Convert.ToDouble(location.Lat), 
                Convert.ToDouble(location.Lon)
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting coordinates for address: {Address}", address);
            return null;
        }
    }
}

public class NominatimResponse
{
    public decimal Lat { get; set; }
    public decimal Lon { get; set; }
} 