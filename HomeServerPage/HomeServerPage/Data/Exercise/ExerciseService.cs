using HomeServerPage.Data.Notifications;
using Microsoft.EntityFrameworkCore;

namespace HomeServerPage.Data.Exercise;

public sealed class ExerciseService(
    IDbContextFactory<ExerciseDbContext> dbContextFactory,
    IRealtimeNotificationPublisher notificationPublisher) : IExerciseService
{
    public async Task<List<ExerciseEntry>> GetEntriesAsync(
        DateOnly from,
        DateOnly through,
        CancellationToken cancellationToken = default)
    {
        if (through < from)
        {
            throw new ArgumentOutOfRangeException(nameof(through), "The end date cannot be earlier than the start date.");
        }

        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var entries = await dbContext.ExerciseEntries
            .Where(entry => entry.LocalDate >= from && entry.LocalDate <= through)
            .ToListAsync(cancellationToken);

        return entries
            .OrderBy(entry => entry.LocalDate)
            .ThenBy(entry => entry.LoggedAt.UtcDateTime)
            .ThenBy(entry => entry.Id)
            .ToList();
    }

    public async Task<ExerciseEntry> AddEntryAsync(
        ExerciseEntry entry,
        CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var added = await dbContext.ExerciseEntries.AddAsync(Normalize(entry) with { Id = 0 }, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        await this.PublishChangedAsync(cancellationToken);
        return added.Entity;
    }

    public async Task<bool> UpdateEntryAsync(
        ExerciseEntry entry,
        CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var entity = await dbContext.ExerciseEntries.FindAsync([entry.Id], cancellationToken);
        if (entity is null)
        {
            return false;
        }

        dbContext.Entry(entity).CurrentValues.SetValues(Normalize(entry));
        await dbContext.SaveChangesAsync(cancellationToken);
        await this.PublishChangedAsync(cancellationToken);
        return true;
    }

    public async Task<bool> RemoveEntryAsync(int id, CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var entity = await dbContext.ExerciseEntries.FindAsync([id], cancellationToken);
        if (entity is null)
        {
            return false;
        }

        dbContext.ExerciseEntries.Remove(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        await this.PublishChangedAsync(cancellationToken);
        return true;
    }

    // Burned calories are always derived server-side so every consumer stores consistent values.
    private static ExerciseEntry Normalize(ExerciseEntry entry)
    {
        var distance = ExerciseCalorieCalculator.RequiresDistance(entry.Type) ? entry.DistanceKm : null;
        return entry with
        {
            DistanceKm = distance,
            CaloriesBurned = ExerciseCalorieCalculator.Calculate(entry.Type, distance)
        };
    }

    private Task PublishChangedAsync(CancellationToken cancellationToken) =>
        notificationPublisher.PublishAsync(
            RealtimeNotificationTypes.CaloriesChanged,
            cancellationToken);
}
