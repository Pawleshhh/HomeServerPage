using Microsoft.EntityFrameworkCore;
using HomeServerPage.Data.Astronomy;

namespace HomeServerPage.Data.Locations;

public sealed class LocationReferenceChecker(
    IDbContextFactory<AstronomyDbContext> astronomyContextFactory) : ILocationReferenceChecker
{
    public async Task<bool> IsReferencedByObservationPointAsync(
        int locationId,
        CancellationToken cancellationToken = default)
    {
        await using var dbContext = await astronomyContextFactory.CreateDbContextAsync(cancellationToken);

        return await dbContext.ObservationPoints
            .AnyAsync(point => point.LocationId == locationId, cancellationToken);
    }
}
