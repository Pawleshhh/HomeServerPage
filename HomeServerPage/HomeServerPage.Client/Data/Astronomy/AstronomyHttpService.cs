using System.Net.Http.Json;
using AstroCalc.Core;
using AstroCalc.Observation;
using AstroCalc.SolarSystem;

namespace HomeServerPage.Data.Astronomy;

public class AstronomyHttpService(HttpClient httpClient) : IAstronomyService
{
    public async Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        return await httpClient.GetFromJsonAsync<bool>("api/astronomy/status", cancellationToken);
    }

    public async Task<RiseTransitSetResult> GetPlanetRiseAndSetTime(DateTime dateTime, GeographicCoordinate location, Planet planet)
    {
        var requestUri = 
            $"api/astronomy/solarsystem/{(int)planet}?DateTime={dateTime:O}&Latitude={location.Latitude.Degrees}&Longitude={location.Longitude.Degrees}&ElevationMeters={location.ElevationMeters}";
        return await httpClient.GetFromJsonAsync<RiseTransitSetResult>(requestUri);
    }
}
