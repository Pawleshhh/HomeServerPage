using AstroCalc.Coordinates;
using AstroCalc.Core;
using AstroCalc.Corrections;
using AstroCalc.Observation;
using AstroCalc.SolarSystem;
using AstroCalc.Time;
using HomeServerPage.Shared.Extensions;
using System.Numerics;

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
            var (jd, obliquity) = GetData(dateTime, location);

            var localPos = PlanetaryPosition.Calculate(planet, jd)
                .Pipe(l => CoordinateTransform.EclipticToEquatorial(l.GeocentricPosition, obliquity))
                .Pipe(eq => RiseTransitSet.Calculate(eq, location, JulianDate.FromDateTime(dateTime.Date)));

            return localPos;
        });
    }

    public async Task<RiseTransitSetResult> GetMoonRiseAndSetTime(DateTime dateTime, GeographicCoordinate location)
    {
        return await Task.Run(() =>
        {
            var (jd, obliquity) = GetData(dateTime, location);

            var localPos = LunarPosition.Calculate(jd)
                .Pipe(l => CoordinateTransform.EclipticToEquatorial(l.Position, obliquity))
                .Pipe(eq => RiseTransitSet.Calculate(eq, location, JulianDate.FromDateTime(dateTime.Date)));

            return localPos;
        });
    }

    public async Task<RiseTransitSetResult> GetSunRiseAndSetTime(DateTime dateTime, GeographicCoordinate location)
    {
        return await Task.Run(() =>
        {
            var (jd, obliquity) = GetData(dateTime, location);

            var localPos = SolarPosition.MediumPrecision(jd)
                .Pipe(s => (s.RightAscension, s.Declination))
                .Pipe(eq => RiseTransitSet.Calculate(
                    new EquatorialCoordinate(eq.RightAscension, eq.Declination),
                    location,
                    JulianDate.FromDateTime(dateTime.Date)));

            return localPos;    
        });
    }

    private static (JulianDate Jd, Angle Obl) GetData(DateTime dateTime, GeographicCoordinate location)
    {
        var jd = JulianDate.FromDateTime(dateTime);
        var obliquity = Nutation.TrueObliquity(jd.CenturiesFromJ2000);

        return (jd, obliquity);
    }
}
