using System.Net.Http.Json;
using HomeServerPage.Data.Astronomy;
using HomeServerPage.Data.Locations;
using HomeServerPage.Tests.Integration.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace HomeServerPage.Tests.Integration;

public sealed class DatabaseMigrationTests
{
    [Test]
    public async Task LegacyObservationPointIsBackfilledOnceAndSurvivesRestart()
    {
        var databaseDirectory = Path.Combine(
            Path.GetTempPath(),
            "HomeServerPage.Tests",
            Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(databaseDirectory);

        try
        {
            await CreateLegacyAstronomyDatabaseAsync(databaseDirectory);
            int migratedLocationId;

            using (var firstFactory = new HomeServerPageWebApplicationFactory(
                       databaseDirectory,
                       deleteDatabaseDirectoryOnDispose: false))
            using (var firstClient = firstFactory.CreateClient())
            {
                var points = await firstClient.GetFromJsonAsync<List<ObservationPoint>>(
                    "/api/observation-points");
                var locations = await firstClient.GetFromJsonAsync<List<SavedLocation>>(
                    "/api/locations");
                var importedLocation = locations!.Single(location => location.Name == "Old ridge");

                Assert.That(points, Has.Count.EqualTo(1));
                Assert.That(points![0].Id, Is.EqualTo(7));
                Assert.That(points[0].LocationId, Is.EqualTo(importedLocation.Id));
                Assert.That(importedLocation.Name, Is.EqualTo("Old ridge"));
                Assert.That(importedLocation.Latitude, Is.EqualTo(53.2));
                Assert.That(importedLocation.Longitude, Is.EqualTo(14.6));
                migratedLocationId = importedLocation.Id;
            }

            using (var secondFactory = new HomeServerPageWebApplicationFactory(
                       databaseDirectory,
                       deleteDatabaseDirectoryOnDispose: false))
            using (var secondClient = secondFactory.CreateClient())
            {
                var points = await secondClient.GetFromJsonAsync<List<ObservationPoint>>(
                    "/api/observation-points");
                var locations = await secondClient.GetFromJsonAsync<List<SavedLocation>>(
                    "/api/locations");

                Assert.That(points, Has.Count.EqualTo(1));
                Assert.That(points![0].LocationId, Is.EqualTo(migratedLocationId));
                Assert.That(
                    locations!.Count(location => location.Name == "Old ridge"),
                    Is.EqualTo(1));
            }
        }
        finally
        {
            if (Directory.Exists(databaseDirectory))
            {
                Directory.Delete(databaseDirectory, recursive: true);
            }
        }
    }

    private static async Task CreateLegacyAstronomyDatabaseAsync(string databaseDirectory)
    {
        var connectionString =
            $"Data Source={Path.Combine(databaseDirectory, "astronomy.db")};Pooling=False";
        var options = new DbContextOptionsBuilder<AstronomyDbContext>()
            .UseSqlite(connectionString)
            .Options;

        await using var dbContext = new AstronomyDbContext(options);
        await dbContext.Database.MigrateAsync(ObservationPointLocationMigrationIds.AddLocationId);
        await dbContext.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO ObservationPoints (
                Id, Name, Latitude, Longitude, ElevationMeters,
                HorizonNorth, HorizonNorthEast, HorizonEast, HorizonSouthEast,
                HorizonSouth, HorizonSouthWest, HorizonWest, HorizonNorthWest, LocationId)
            VALUES (
                {7}, {"Old ridge"}, {53.2}, {14.6}, {25.0},
                {5.0}, {6.0}, {7.0}, {8.0},
                {9.0}, {10.0}, {11.0}, {12.0}, {null})
            """);
    }
}
