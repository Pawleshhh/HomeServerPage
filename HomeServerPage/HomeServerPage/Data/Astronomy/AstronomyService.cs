using AstroCalc.Core;
using AstroCalc.Corrections;
using AstroCalc.Observation;
using AstroCalc.SolarSystem;
using AstroCalc.Time;
using HomeServerPage.Data.Astronomy;

namespace HomeServerPage.Data.Astronomy;

public class AstronomyService : IAstronomyService
{
    public Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(true);
    }

    public async Task<RiseTransitSetResult> GetPlanetRiseAndSetTime(GeographicCoordinate location, Planet planet)
    {
        return await Task.Run(() =>
        {
            var dt = DateTime.Now;
            var jd = JulianDate.FromDateTime(dt);
            var planetLocation = PlanetaryPosition.Calculate(planet, jd);
            var obliquity = Nutation.TrueObliquity(jd.CenturiesFromJ2000);
            var eqCoords = AstroCalc.Coordinates.CoordinateTransform.EclipticToEquatorial(planetLocation.GeocentricPosition, obliquity);
            var localPos = AstroCalc.Observation.RiseTransitSet.Calculate(
                eqCoords,
                location,
                JulianDate.FromDateTime(dt.Date));

            return localPos;
        });
    }
}
