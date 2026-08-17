using AstroCalc.Catalogs;
using AstroCalc.Core;
using AstroCalc.Observation;
using AstroCalc.SolarSystem;
using AstroCalc.Time;
using HomeServerPage.Shared.Extensions;

namespace HomeServerPage.Data.Astronomy;

public static class AstronomyObservationLoader
{
    public static async Task<IReadOnlyList<AstronomyObservation>> LoadSolarSystemAsync(
        IAstronomyService astronomyService,
        DateTime requestDateUtc,
        GeographicCoordinate location)
    {
        var planets = Enum.GetValues<Planet>().Where(planet => planet != Planet.Earth);
        var planetObservations = planets.Select(async planet =>
            new AstronomyObservation(
                planet.ToString(),
                planet.ToString().ToLowerInvariant(),
                await astronomyService.GetPlanetRiseAndSetTime(requestDateUtc, location, planet)));

        var observations = await Task.WhenAll(
            planetObservations
                .Prepend(LoadSunAsync(astronomyService, requestDateUtc, location))
                .Append(LoadMoonAsync(astronomyService, requestDateUtc, location)));

        return observations;
    }

    private static async Task<AstronomyObservation> LoadMoonAsync(
        IAstronomyService astronomyService,
        DateTime requestDateUtc,
        GeographicCoordinate location)
    {
        return new AstronomyObservation(
            "Moon",
            "moon",
            await astronomyService.GetMoonRiseAndSetTime(requestDateUtc, location));
    }

    private static async Task<AstronomyObservation> LoadSunAsync(
        IAstronomyService astronomyService,
        DateTime requestDateUtc,
        GeographicCoordinate location)
    {
        return new AstronomyObservation(
            "Sun",
            "sun",
            await astronomyService.GetSunRiseAndSetTime(requestDateUtc, location));
    }

    public static async Task<IReadOnlyList<AstronomyObservation>> LoadMessierCatalogAsync(
        IAstronomyService astronomyService,
        DateTime dateTime,
        GeographicCoordinate location)
    {
        static string GetKey(MessierObject m) => $"M{m.Number}";
        static string GetName(MessierObject m) => $"{GetKey(m)} {m.CommonName}";

        RiseTransitSetResult GetRiseSetData(MessierObject m)
            => RiseTransitSet.Calculate(m.Position, location, JulianDate.FromDateTime(dateTime));

        var messierObjects = await astronomyService.GetAllMessierObjects();
        var messierObservations = messierObjects.Select(m =>
            new AstronomyObservation(
                GetName(m),
                GetKey(m),
                GetRiseSetData(m)));

        return [.. messierObservations];
    }
}
