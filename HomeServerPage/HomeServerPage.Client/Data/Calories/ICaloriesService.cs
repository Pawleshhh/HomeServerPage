namespace HomeServerPage.Data.Calories;

public interface ICaloriesService
{
    Task<List<CaloriesFood>> GetFoodsAsync(CancellationToken cancellationToken = default);

    Task<CaloriesFood> AddFoodAsync(CaloriesFood food, CancellationToken cancellationToken = default);

    Task<bool> UpdateFoodAsync(CaloriesFood food, CancellationToken cancellationToken = default);

    Task<bool> RemoveFoodAsync(int id, CancellationToken cancellationToken = default);

    Task<List<CaloriesLogEntry>> GetEntriesAsync(
        DateOnly from,
        DateOnly through,
        CancellationToken cancellationToken = default);

    Task<CaloriesLogEntry> AddEntryAsync(
        CaloriesLogEntry entry,
        CancellationToken cancellationToken = default);

    Task<bool> UpdateEntryAsync(
        CaloriesLogEntry entry,
        CancellationToken cancellationToken = default);

    Task<bool> RemoveEntryAsync(int id, CancellationToken cancellationToken = default);

    Task<int?> GetDailyGoalAsync(CancellationToken cancellationToken = default);

    Task<int> SetDailyGoalAsync(int calories, CancellationToken cancellationToken = default);
}
