namespace HomeServerPage.Data.Exercise;

public interface IExerciseService
{
    Task<List<ExerciseEntry>> GetEntriesAsync(
        DateOnly from,
        DateOnly through,
        CancellationToken cancellationToken = default);

    Task<ExerciseEntry> AddEntryAsync(
        ExerciseEntry entry,
        CancellationToken cancellationToken = default);

    Task<bool> UpdateEntryAsync(
        ExerciseEntry entry,
        CancellationToken cancellationToken = default);

    Task<bool> RemoveEntryAsync(int id, CancellationToken cancellationToken = default);
}
