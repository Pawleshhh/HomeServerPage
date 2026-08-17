using AstroCalc.Catalogs;
using AstroCalc.Core;
using AstroCalc.Observation;
using AstroCalc.SolarSystem;

namespace HomeServerPage.Data.Astronomy;

public interface IAstronomyService
{
    Task<IEnumerable<MessierObject>> GetAllMessierObjects();

    Task<MessierObject> GetMessierObject(int messierId);

    Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default);

    Task<RiseTransitSetResult> GetPlanetRiseAndSetTime(DateTime dateTime, GeographicCoordinate location, Planet planet);

    Task<RiseTransitSetResult> GetMoonRiseAndSetTime(DateTime dateTime, GeographicCoordinate location);

    Task<RiseTransitSetResult> GetSunRiseAndSetTime(DateTime dateTime, GeographicCoordinate location);
}
