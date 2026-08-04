using AstroCalc.Coordinates;
using AstroCalc.Core;
using AstroCalc.Corrections;
using AstroCalc.Observation;
using AstroCalc.SolarSystem;
using AstroCalc.Time;
using HomeServerPage.Shared.Extensions;

namespace HomeServerPage.Data.Astronomy;

public class AstronomyService : IAstronomyService
{
    public Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(true);
    }

    public async Task<RiseTransitSetResult> GetPlanetRiseAndSetTime(DateTime dateTime, GeographicCoordinate location, Planet planet)
    {
        return await Task.Run(() =>
        {
            var jd = JulianDate.FromDateTime(dateTime);
            var obliquity = Nutation.TrueObliquity(jd.CenturiesFromJ2000);

            var localPos = dateTime
                .Pipe(d => JulianDate.FromDateTime(d))
                .Pipe(jd => PlanetaryPosition.Calculate(planet, jd))
                .Pipe(l => CoordinateTransform.EclipticToEquatorial(l.GeocentricPosition, obliquity))
                .Pipe(eq => RiseTransitSet.Calculate(eq, location, JulianDate.FromDateTime(dateTime.Date)));

            return localPos;
        });
    }
}
