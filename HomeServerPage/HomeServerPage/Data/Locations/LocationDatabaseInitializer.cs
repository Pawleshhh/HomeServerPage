using HomeServerPage.Data.Astronomy;
using HomeServerPage.Data.Locations;
using Microsoft.EntityFrameworkCore;

namespace HomeServerPage.Data.Locations;

public sealed class LocationDatabaseInitializer(
    IDbContextFactory<LocationsDbContext> dbContextFactory,
    bool weatherBootstrapEnabled)
{
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        var seedState = await dbContext.SeedStates
            .SingleOrDefaultAsync(state => state.Id == 1, cancellationToken);
        var seedInitialLocations = seedState is null || !seedState.InitialLocationsSeeded;

        var defaultLocation = await dbContext.Locations
            .SingleOrDefaultAsync(location => location.IsSystemDefault, cancellationToken);
        if (defaultLocation is null)
        {
            defaultLocation = new SavedLocation
            {
                Name = "Default",
                Latitude = 0,
                Longitude = 0,
                ElevationMeters = 0,
                TimeZoneId = "Etc/UTC",
                IsSystemDefault = true
            };
            dbContext.Locations.Add(defaultLocation);
        }

        var szczecin = seedInitialLocations
            ? await dbContext.Locations.SingleOrDefaultAsync(
                location => location.Name == "Szczecin",
                cancellationToken)
            : null;
        if (seedInitialLocations && szczecin is null)
        {
            szczecin = new SavedLocation
            {
                Name = "Szczecin",
                Latitude = AstronomyDefaults.Latitude,
                Longitude = AstronomyDefaults.Longitude,
                ElevationMeters = 0,
                TimeZoneId = "Europe/Warsaw"
            };
            dbContext.Locations.Add(szczecin);
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        if (seedState is null)
        {
            seedState = new LocationDatabaseSeedState { Id = 1 };
            dbContext.SeedStates.Add(seedState);
        }

        seedState.InitialLocationsSeeded = true;

        foreach (var feature in Enum.GetValues<LocationFeature>())
        {
            var selection = await dbContext.Selections
                .SingleOrDefaultAsync(item => item.Feature == feature, cancellationToken);
            if (selection is null)
            {
                dbContext.Selections.Add(new LocationSelection
                {
                    Feature = feature,
                    LocationId = (szczecin ?? defaultLocation).Id,
                    IsInitialized = feature == LocationFeature.Astronomy || !weatherBootstrapEnabled
                });
            }
            else if (!await dbContext.Locations.AnyAsync(
                         location => location.Id == selection.LocationId,
                         cancellationToken))
            {
                selection.LocationId = defaultLocation.Id;
                selection.IsInitialized = true;
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }
}
