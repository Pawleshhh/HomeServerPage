using AstroCalc.Catalogs;
using AstroCalc.Core;
using AstroCalc.Observation;
using AstroCalc.SolarSystem;
using System.Net.Http.Json;
using System.Numerics;

namespace HomeServerPage.Data.Astronomy;

public class AstronomyHttpService(HttpClient httpClient) : IAstronomyService
{
    public async Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        return await httpClient.GetFromJsonAsync<bool>("api/astronomy/status", cancellationToken);
    }

    public async Task<IEnumerable<MessierObject>> GetAllMessierObjects()
    {
        var requestUri =
            "api/astronomy/messier/";
        return await httpClient.GetFromJsonAsync<IEnumerable<MessierObject>>(requestUri) ?? [];
    }

    public async Task<MessierObject> GetMessierObject(int messierId)
    {
        var requestUri =
            $"api/astronomy/messier/{messierId}";
        return (await httpClient.GetFromJsonAsync<MessierObject>(requestUri))!;
    }

    public async Task<RiseTransitSetResult> GetPlanetRiseAndSetTime(DateTime dateTime, GeographicCoordinate location, Planet planet)
    {
        var requestUri = 
            $"api/astronomy/solarsystem/{(int)planet}?DateTime={dateTime:O}&Latitude={location.Latitude.Degrees}&Longitude={location.Longitude.Degrees}&ElevationMeters={location.ElevationMeters}";
        return await httpClient.GetFromJsonAsync<RiseTransitSetResult>(requestUri);
    }
    public async Task<RiseTransitSetResult> GetMoonRiseAndSetTime(DateTime dateTime, GeographicCoordinate location)
    {
        var requestUri =
            $"api/astronomy/solarsystem/moon?DateTime={dateTime:O}&Latitude={location.Latitude.Degrees}&Longitude={location.Longitude.Degrees}&ElevationMeters={location.ElevationMeters}";
        return await httpClient.GetFromJsonAsync<RiseTransitSetResult>(requestUri);
    }
    public async Task<RiseTransitSetResult> GetSunRiseAndSetTime(DateTime dateTime, GeographicCoordinate location)
    {
        var requestUri =
            $"api/astronomy/solarsystem/sun?DateTime={dateTime:O}&Latitude={location.Latitude.Degrees}&Longitude={location.Longitude.Degrees}&ElevationMeters={location.ElevationMeters}";
        return await httpClient.GetFromJsonAsync<RiseTransitSetResult>(requestUri);
    }
}
