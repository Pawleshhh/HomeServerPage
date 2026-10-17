namespace HomeServerPage.Data.Calories;

public record CaloriesLogEntry(
    int Id,
    string Name,
    int Calories,
    int? CarbohydratesGrams,
    int? ProteinGrams,
    int? FatGrams,
    decimal? Servings,
    DateOnly LocalDate,
    DateTimeOffset LoggedAt,
    string? Notes = null);
