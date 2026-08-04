using AstroCalc.Core;
using AstroCalc.Observation;
using AstroCalc.SolarSystem;

namespace HomeServerPage.Data.Astronomy;

public interface IAstronomyService
{
    Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default);

    Task<RiseTransitSetResult> GetPlanetRiseAndSetTime(DateTime dateTime, GeographicCoordinate location, Planet planet);
}
