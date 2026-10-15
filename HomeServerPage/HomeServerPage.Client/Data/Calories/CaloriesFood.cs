namespace HomeServerPage.Data.Calories;

public record CaloriesFood(
    int Id,
    string Name,
    int CaloriesPerServing,
    int? CarbohydratesGrams,
    int? ProteinGrams,
    int? FatGrams);
