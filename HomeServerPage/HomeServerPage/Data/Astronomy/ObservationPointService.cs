using HomeServerPage.Data.Astronomy;
using HomeServerPage.Client.Data.Locations;
using HomeServerPage.Data.Locations;
using Microsoft.EntityFrameworkCore;

namespace HomeServerPage.Data.Astronomy;

public sealed class ObservationPointService(
    IDbContextFactory<AstronomyDbContext> dbContextFactory,
    ILocationService locationService,
    LocationOperationCoordinator operationCoordinator)
    : IObservationPointService
{
    public async Task<IReadOnlyList<ObservationPoint>> GetObservationPointsAsync(
        CancellationToken cancellationToken = default)
    {
        await operationCoordinator.WaitAsync(cancellationToken);

        try
        {
            await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
            var observationPoints = await dbContext.Set<ObservationPoint>()
                .AsNoTracking()
                .OrderBy(observationPoint => observationPoint.Name)
                .ToListAsync(cancellationToken);
            var locations = (await locationService.GetLocationsAsync(cancellationToken))
                .ToDictionary(location => location.Id);

            foreach (var observationPoint in observationPoints)
            {
                if (observationPoint.LocationId is not int locationId
                    || !locations.TryGetValue(locationId, out var location))
                {
                    throw new InvalidOperationException(
                        $"Observation point {observationPoint.Id} references a missing location.");
                }

                ApplyLocation(observationPoint, location);
            }

            return observationPoints;
        }
        finally
        {
            operationCoordinator.Release();
        }
    }

    public async Task<ObservationPoint> SaveObservationPointAsync(
        ObservationPoint observationPoint,
        CancellationToken cancellationToken = default)
    {
        Validate(observationPoint);
        var locationId = observationPoint.LocationId
            ?? throw new ArgumentException("An observation point location is required.", nameof(observationPoint));
        await operationCoordinator.WaitAsync(cancellationToken);

        try
        {
            var location = await locationService.GetLocationAsync(locationId, cancellationToken)
                ?? throw new KeyNotFoundException($"Location {locationId} was not found.");
            await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

            ObservationPoint entity;
            if (observationPoint.Id == 0)
            {
                entity = new ObservationPoint();
                dbContext.Set<ObservationPoint>().Add(entity);
            }
            else
            {
                entity = await dbContext.Set<ObservationPoint>()
                    .SingleOrDefaultAsync(
                        point => point.Id == observationPoint.Id,
                        cancellationToken)
                    ?? throw new KeyNotFoundException(
                        $"Observation point {observationPoint.Id} was not found.");
            }

            CopyValues(observationPoint, entity);
            ApplyLocation(entity, location);
            await dbContext.SaveChangesAsync(cancellationToken);

            return entity;
        }
        finally
        {
            operationCoordinator.Release();
        }
    }

    public async Task DeleteObservationPointAsync(
        int observationPointId,
        CancellationToken cancellationToken = default)
    {
        await operationCoordinator.WaitAsync(cancellationToken);

        try
        {
            await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

            var entity = await dbContext.Set<ObservationPoint>()
                .SingleOrDefaultAsync(
                    observationPoint => observationPoint.Id == observationPointId,
                    cancellationToken)
                ?? throw new KeyNotFoundException(
                    $"Observation point {observationPointId} was not found.");

            dbContext.Set<ObservationPoint>().Remove(entity);
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        finally
        {
            operationCoordinator.Release();
        }
    }

    private static void Validate(ObservationPoint observationPoint)
    {
        if (string.IsNullOrWhiteSpace(observationPoint.Name))
        {
            throw new ArgumentException("An observation point name is required.", nameof(observationPoint));
        }

        if (observationPoint.LocationId is null or <= 0)
        {
            throw new ArgumentException("An observation point location is required.", nameof(observationPoint));
        }
    }

    private static void CopyValues(ObservationPoint source, ObservationPoint target)
    {
        target.Name = source.Name.Trim();
        target.LocationId = source.LocationId;
        target.HorizonNorth = source.HorizonNorth;
        target.HorizonNorthEast = source.HorizonNorthEast;
        target.HorizonEast = source.HorizonEast;
        target.HorizonSouthEast = source.HorizonSouthEast;
        target.HorizonSouth = source.HorizonSouth;
        target.HorizonSouthWest = source.HorizonSouthWest;
        target.HorizonWest = source.HorizonWest;
        target.HorizonNorthWest = source.HorizonNorthWest;
    }

    private static void ApplyLocation(ObservationPoint observationPoint, SavedLocation location)
    {
        observationPoint.LocationId = location.Id;
        observationPoint.Location = location;
    }
}
