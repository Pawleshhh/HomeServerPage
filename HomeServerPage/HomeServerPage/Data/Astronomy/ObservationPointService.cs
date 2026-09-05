using HomeServerPage.Data.Astronomy;
using Microsoft.EntityFrameworkCore;

namespace HomeServerPage.Data.Astronomy;

public sealed class ObservationPointService(IDbContextFactory<AstronomyDbContext> dbContextFactory)
    : IObservationPointService
{
    public async Task<IReadOnlyList<ObservationPoint>> GetObservationPointsAsync(
        CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        return await dbContext.Set<ObservationPoint>()
            .AsNoTracking()
            .OrderBy(observationPoint => observationPoint.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<ObservationPoint> SaveObservationPointAsync(
        ObservationPoint observationPoint,
        CancellationToken cancellationToken = default)
    {
        Validate(observationPoint);

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
        await dbContext.SaveChangesAsync(cancellationToken);

        return entity;
    }

    public async Task DeleteObservationPointAsync(
        int observationPointId,
        CancellationToken cancellationToken = default)
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

    private static void Validate(ObservationPoint observationPoint)
    {
        if (string.IsNullOrWhiteSpace(observationPoint.Name))
        {
            throw new ArgumentException("An observation point name is required.", nameof(observationPoint));
        }
    }

    private static void CopyValues(ObservationPoint source, ObservationPoint target)
    {
        target.Name = source.Name.Trim();
        target.Latitude = source.Latitude;
        target.Longitude = source.Longitude;
        target.ElevationMeters = source.ElevationMeters;
        target.HorizonNorth = source.HorizonNorth;
        target.HorizonNorthEast = source.HorizonNorthEast;
        target.HorizonEast = source.HorizonEast;
        target.HorizonSouthEast = source.HorizonSouthEast;
        target.HorizonSouth = source.HorizonSouth;
        target.HorizonSouthWest = source.HorizonSouthWest;
        target.HorizonWest = source.HorizonWest;
        target.HorizonNorthWest = source.HorizonNorthWest;
    }
}
