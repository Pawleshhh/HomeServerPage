using System.Net.Http.Json;
using AstroCalc.Core;
using AstroCalc.SolarSystem;

namespace HomeServerPage.Data.Astronomy;

public class AstronomyHttpService(HttpClient httpClient) : IAstronomyService
{
    public async Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        return await httpClient.GetFromJsonAsync<bool>("api/astronomy/status", cancellationToken);
    }

    public async Task GetPlanetRiseAndSetTime(GeographicCoordinate location, Planet planet)
    {
        var requestUri = $"api/astronomy/solarsystem/{(int)planet}?Latitude={location.Latitude.Degrees}&Longitude={location.Longitude.Degrees}&ElevationMeters={location.ElevationMeters}";
        using var response = await httpClient.GetAsync(requestUri);
        response.EnsureSuccessStatusCode();
    }
}
