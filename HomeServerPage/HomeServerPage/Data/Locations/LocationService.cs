using HomeServerPage.Client.Data.Locations;
using HomeServerPage.Data.Locations;
using Microsoft.EntityFrameworkCore;

namespace HomeServerPage.Data.Locations;

public sealed class LocationService(
    IDbContextFactory<LocationsDbContext> dbContextFactory,
    ILocationReferenceChecker locationReferenceChecker,
    LocationOperationCoordinator operationCoordinator) : ILocationService
{
    public async Task<IReadOnlyList<SavedLocation>> GetLocationsAsync(
        CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        return await dbContext.Locations
            .AsNoTracking()
            .OrderByDescending(location => location.IsSystemDefault)
            .ThenBy(location => location.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<SavedLocation?> GetLocationAsync(
        int locationId,
        CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        return await dbContext.Locations
            .AsNoTracking()
            .SingleOrDefaultAsync(location => location.Id == locationId, cancellationToken);
    }

    public async Task<SavedLocation> SaveLocationAsync(
        SavedLocation location,
        CancellationToken cancellationToken = default)
    {
        LocationValidation.Validate(location);
        await operationCoordinator.WaitAsync(cancellationToken);

        try
        {
            await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

            SavedLocation entity;
            if (location.Id == 0)
            {
                entity = new SavedLocation();
                dbContext.Locations.Add(entity);
            }
            else
            {
                entity = await dbContext.Locations
                    .SingleOrDefaultAsync(item => item.Id == location.Id, cancellationToken)
                    ?? throw new KeyNotFoundException($"Location {location.Id} was not found.");

                if (entity.IsSystemDefault)
                {
                    throw new InvalidOperationException("The system Default location cannot be edited.");
                }
            }

            CopyValues(location, entity);
            entity.IsSystemDefault = false;
            await dbContext.SaveChangesAsync(cancellationToken);

            return entity;
        }
        finally
        {
            operationCoordinator.Release();
        }
    }

    public async Task DeleteLocationAsync(
        int locationId,
        CancellationToken cancellationToken = default)
    {
        await operationCoordinator.WaitAsync(cancellationToken);

        try
        {
            if (await locationReferenceChecker.IsReferencedByObservationPointAsync(
                    locationId,
                    cancellationToken))
            {
                throw new LocationInUseException(locationId);
            }

            await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

            var location = await dbContext.Locations
                .SingleOrDefaultAsync(item => item.Id == locationId, cancellationToken)
                ?? throw new KeyNotFoundException($"Location {locationId} was not found.");

            if (location.IsSystemDefault)
            {
                throw new InvalidOperationException("The system Default location cannot be deleted.");
            }

            var defaultLocationId = await dbContext.Locations
                .Where(item => item.IsSystemDefault)
                .Select(item => item.Id)
                .SingleAsync(cancellationToken);

            var selections = await dbContext.Selections
                .Where(selection => selection.LocationId == locationId)
                .ToListAsync(cancellationToken);
            foreach (var selection in selections)
            {
                selection.LocationId = defaultLocationId;
                selection.IsInitialized = true;
            }

            dbContext.Locations.Remove(location);
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        finally
        {
            operationCoordinator.Release();
        }
    }

    private static void CopyValues(SavedLocation source, SavedLocation target)
    {
        target.Name = source.Name.Trim();
        target.Latitude = source.Latitude;
        target.Longitude = source.Longitude;
        target.ElevationMeters = source.ElevationMeters;
        target.TimeZoneId = source.TimeZoneId.Trim();
    }
}
