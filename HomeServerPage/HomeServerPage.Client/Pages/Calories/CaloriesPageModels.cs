using System.ComponentModel.DataAnnotations;

namespace HomeServerPage.Client.Pages;

public sealed class CaloriesFoodDraft
{
    [Required]
    [StringLength(160)]
    public string? Name { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int? CaloriesPerServing { get; set; }

    [Range(0, int.MaxValue)]
    public int? CarbohydratesGrams { get; set; }

    [Range(0, int.MaxValue)]
    public int? ProteinGrams { get; set; }

    [Range(0, int.MaxValue)]
    public int? FatGrams { get; set; }
}

public sealed class CaloriesEntryDraft
{
    [Required]
    public int? FoodId { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int? Servings { get; set; } = 1;
}

public sealed class CustomCaloriesEntryDraft
{
    [Required]
    [StringLength(160)]
    public string? Name { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int? Calories { get; set; }

    [Range(0, int.MaxValue)]
    public int? CarbohydratesGrams { get; set; }

    [Range(0, int.MaxValue)]
    public int? ProteinGrams { get; set; }

    [Range(0, int.MaxValue)]
    public int? FatGrams { get; set; }
}

public sealed class CaloriesGoalDraft
{
    [Required]
    [Range(1, int.MaxValue)]
    public int? Calories { get; set; }
}
