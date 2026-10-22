namespace HomeServerPage.Data.Exercise;

public record ExerciseEntry(
    int Id,
    ExerciseType Type,
    decimal? DistanceKm,
    int CaloriesBurned,
    DateOnly LocalDate,
    DateTimeOffset LoggedAt);
