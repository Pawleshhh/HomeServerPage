using HomeServerPage.Data.Notifications;
using Microsoft.EntityFrameworkCore;

namespace HomeServerPage.Data.Calories;

public sealed class CaloriesService(
    IDbContextFactory<CaloriesDbContext> dbContextFactory,
    IRealtimeNotificationPublisher notificationPublisher) : ICaloriesService
{
    public async Task<List<CaloriesFood>> GetFoodsAsync(CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        return await dbContext.Foods
            .OrderBy(food => food.Name)
            .ThenBy(food => food.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<CaloriesFood> AddFoodAsync(
        CaloriesFood food,
        CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var entry = await dbContext.Foods.AddAsync(food with
        {
            Id = 0,
            Name = NormalizeName(food.Name)
        }, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        await this.PublishChangedAsync(cancellationToken);
        return entry.Entity;
    }

    public async Task<bool> UpdateFoodAsync(
        CaloriesFood food,
        CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var entity = await dbContext.Foods.FindAsync([food.Id], cancellationToken);
        if (entity is null)
        {
            return false;
        }

        dbContext.Entry(entity).CurrentValues.SetValues(food with
        {
            Name = NormalizeName(food.Name)
        });
        await dbContext.SaveChangesAsync(cancellationToken);
        await this.PublishChangedAsync(cancellationToken);
        return true;
    }

    public async Task<bool> RemoveFoodAsync(int id, CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var entity = await dbContext.Foods.FindAsync([id], cancellationToken);
        if (entity is null)
        {
            return false;
        }

        dbContext.Foods.Remove(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        await this.PublishChangedAsync(cancellationToken);
        return true;
    }

    public async Task<List<CaloriesLogEntry>> GetEntriesAsync(
        DateOnly from,
        DateOnly through,
        CancellationToken cancellationToken = default)
    {
        if (through < from)
        {
            throw new ArgumentOutOfRangeException(nameof(through), "The end date cannot be earlier than the start date.");
        }

        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var entries = await dbContext.Entries
            .Where(entry => entry.LocalDate >= from && entry.LocalDate <= through)
            .ToListAsync(cancellationToken);

        return entries
            .OrderBy(entry => entry.LocalDate)
            .ThenBy(entry => entry.LoggedAt.UtcDateTime)
            .ThenBy(entry => entry.Id)
            .ToList();
    }

    public async Task<CaloriesLogEntry> AddEntryAsync(
        CaloriesLogEntry entry,
        CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var addedEntry = await dbContext.Entries.AddAsync(entry with
        {
            Id = 0,
            Name = NormalizeName(entry.Name)
        }, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        await this.PublishChangedAsync(cancellationToken);
        return addedEntry.Entity;
    }

    public async Task<bool> UpdateEntryAsync(
        CaloriesLogEntry entry,
        CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var entity = await dbContext.Entries.FindAsync([entry.Id], cancellationToken);
        if (entity is null)
        {
            return false;
        }

        dbContext.Entry(entity).CurrentValues.SetValues(entry with
        {
            Name = NormalizeName(entry.Name)
        });
        await dbContext.SaveChangesAsync(cancellationToken);
        await this.PublishChangedAsync(cancellationToken);
        return true;
    }

    public async Task<bool> RemoveEntryAsync(int id, CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var entity = await dbContext.Entries.FindAsync([id], cancellationToken);
        if (entity is null)
        {
            return false;
        }

        dbContext.Entries.Remove(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        await this.PublishChangedAsync(cancellationToken);
        return true;
    }

    public async Task<int?> GetDailyGoalAsync(CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        return await dbContext.Settings
            .Where(settings => settings.Id == 1)
            .Select(settings => settings.DailyCalorieGoal)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<int> SetDailyGoalAsync(
        int calories,
        CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var settings = await dbContext.Settings.FindAsync([1], cancellationToken);
        if (settings is null)
        {
            settings = new CaloriesTrackerSettings { Id = 1 };
            await dbContext.Settings.AddAsync(settings, cancellationToken);
        }

        settings.DailyCalorieGoal = calories;
        await dbContext.SaveChangesAsync(cancellationToken);
        await this.PublishChangedAsync(cancellationToken);
        return calories;
    }

    private Task PublishChangedAsync(CancellationToken cancellationToken) =>
        notificationPublisher.PublishAsync(
            RealtimeNotificationTypes.CaloriesChanged,
            cancellationToken);

    private static string NormalizeName(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return name.Trim();
    }
}
