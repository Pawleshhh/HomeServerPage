using AstroCalc.Core;
using AstroCalc.SolarSystem;

namespace HomeServerPage.Data.Astronomy;

public static class AstronomyObservationLoader
{
    public static async Task<IReadOnlyList<AstronomyObservation>> LoadAsync(
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
            planetObservations.Append(
                LoadMoonAsync(astronomyService, requestDateUtc, location)));

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
}
