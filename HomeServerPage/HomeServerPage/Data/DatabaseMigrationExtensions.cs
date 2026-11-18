using HomeServerPage.Data.Astronomy;
using HomeServerPage.Data.Calories;
using HomeServerPage.Data.Exercise;
using HomeServerPage.Data.Fridge;
using HomeServerPage.Data.Locations;
using Microsoft.EntityFrameworkCore;

namespace HomeServerPage.Data;

public static class DatabaseMigrationExtensions
{
    public static async Task MigrateApplicationDatabasesAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var scopedServices = scope.ServiceProvider;

        await MigrateDatabaseAsync<LocationsDbContext>(scopedServices);
        await scopedServices.GetRequiredService<LocationDatabaseInitializer>().InitializeAsync();
        if (!await HasAppliedMigrationAsync<AstronomyDbContext>(
                scopedServices,
                ObservationPointLocationMigrationIds.RemoveLegacyCoordinates))
        {
            await MigrateDatabaseToAsync<AstronomyDbContext>(
                scopedServices,
                ObservationPointLocationMigrationIds.AddLocationId);
            await scopedServices.GetRequiredService<LegacyObservationPointLocationBackfill>().RunAsync();
        }

        await MigrateDatabaseAsync<AstronomyDbContext>(scopedServices);
        await MigrateDatabaseAsync<FridgeDbContext>(scopedServices);
        await MigrateDatabaseAsync<CaloriesDbContext>(scopedServices);
        await MigrateDatabaseAsync<ExerciseDbContext>(scopedServices);
    }

    private static async Task MigrateDatabaseAsync<TContext>(IServiceProvider serviceProvider)
        where TContext : DbContext
    {
        var contextFactory = serviceProvider.GetRequiredService<IDbContextFactory<TContext>>();
        await using var dbContext = await contextFactory.CreateDbContextAsync();
        await dbContext.Database.MigrateAsync();
    }

    private static async Task MigrateDatabaseToAsync<TContext>(
        IServiceProvider serviceProvider,
        string targetMigration)
        where TContext : DbContext
    {
        var contextFactory = serviceProvider.GetRequiredService<IDbContextFactory<TContext>>();
        await using var dbContext = await contextFactory.CreateDbContextAsync();
        await dbContext.Database.MigrateAsync(targetMigration);
    }

    private static async Task<bool> HasAppliedMigrationAsync<TContext>(
        IServiceProvider serviceProvider,
        string migrationId)
        where TContext : DbContext
    {
        var contextFactory = serviceProvider.GetRequiredService<IDbContextFactory<TContext>>();
        await using var dbContext = await contextFactory.CreateDbContextAsync();
        var appliedMigrations = await dbContext.Database.GetAppliedMigrationsAsync();
        return appliedMigrations.Contains(migrationId);
    }
}
