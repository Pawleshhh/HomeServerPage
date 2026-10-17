using HomeServerPage.Data.Astronomy;
using HomeServerPage.Data.Calories;
using HomeServerPage.Data.Fridge;
using Microsoft.EntityFrameworkCore;

namespace HomeServerPage.Data;

public static class DatabaseMigrationExtensions
{
    public static void MigrateApplicationDatabases(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var scopedServices = scope.ServiceProvider;

        MigrateDatabase<FridgeDbContext>(scopedServices);
        MigrateDatabase<AstronomyDbContext>(scopedServices);
        MigrateDatabase<CaloriesDbContext>(scopedServices);
    }

    private static void MigrateDatabase<TContext>(IServiceProvider serviceProvider)
        where TContext : DbContext
    {
        var contextFactory = serviceProvider.GetRequiredService<IDbContextFactory<TContext>>();
        using var dbContext = contextFactory.CreateDbContext();
        dbContext.Database.Migrate();
    }
}
