using HomeServerPage.Client.Data.Astronomy.Telescopes;
using Microsoft.EntityFrameworkCore;

namespace HomeServerPage.Data.Astronomy.Telescopes;

public class TelescopeService(IDbContextFactory<AstronomyDbContext> dbContextFactory) : ITelescopeService
{
    public async Task<IEnumerable<TelescopeItem>> GetTelescopesAsync(CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        return await dbContext.Telescopes.AsNoTracking().ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<TelescopeEyepiece>> GetEyepiecesAsync(CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        return await dbContext.Eyepieces.AsNoTracking().ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<TelescopeLens>> GetLensesAsync(CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        return await dbContext.Lenses.AsNoTracking().ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<SensorItem>> GetSensorsAsync(CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        return await dbContext.Sensors.AsNoTracking().ToListAsync(cancellationToken);
    }

    public Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(true);
    }

}
