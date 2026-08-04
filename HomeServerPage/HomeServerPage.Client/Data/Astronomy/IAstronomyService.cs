using AstroCalc.Core;
using AstroCalc.SolarSystem;

namespace HomeServerPage.Data.Astronomy;

public interface IAstronomyService
{
    Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default);

    Task GetPlanetRiseAndSetTime(GeographicCoordinate location, Planet planet);
}
