using System.ComponentModel.DataAnnotations;

namespace HomeServerPage.Client.Pages;

public sealed class CaloriesFoodDraft : IValidatableObject
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

    [Range(typeof(decimal), "0.1", "79228162514264337593543950335")]
    public decimal? GramsPerServing { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (GramsPerServing is decimal grams && grams != decimal.Round(grams, 1))
        {
            yield return new ValidationResult(
                "Grams per serving may have at most one decimal place.",
                [nameof(GramsPerServing)]);
        }
    }
}

public sealed class CaloriesEntryDraft : IValidatableObject
{
    [Required]
    public int? FoodId { get; set; }

    [Required]
    [Range(typeof(decimal), "0.01", "79228162514264337593543950335")]
    public decimal? Servings { get; set; } = 1m;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Servings is decimal servings && servings != decimal.Round(servings, 2))
        {
            yield return new ValidationResult(
                "Use at most two decimal places.",
                [nameof(Servings)]);
        }
    }
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

public sealed class CaloriesLoggedEntryDraft : IValidatableObject
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

    [Range(typeof(decimal), "0.01", "79228162514264337593543950335")]
    public decimal? Servings { get; set; }

    public decimal? GramsConsumed { get; set; }

    [Required]
    public string? Time { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Servings is decimal servings && servings != decimal.Round(servings, 2))
        {
            yield return new ValidationResult(
                "Use at most two decimal places.",
                [nameof(Servings)]);
        }

        if (GramsConsumed is decimal grams && grams != decimal.Round(grams, 3))
        {
            yield return new ValidationResult(
                "Consumed grams may have at most three decimal places.",
                [nameof(GramsConsumed)]);
        }
    }
}

public sealed class ExerciseDraft : IValidatableObject
{
    public HomeServerPage.Data.Exercise.ExerciseType Type { get; set; } = HomeServerPage.Data.Exercise.ExerciseType.Gym;

    [Range(typeof(decimal), "0.01", "1000")]
    public decimal? DistanceKm { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (HomeServerPage.Data.Exercise.ExerciseCalorieCalculator.RequiresDistance(Type))
        {
            if (DistanceKm is null)
            {
                yield return new ValidationResult("Enter a distance.", [nameof(DistanceKm)]);
            }
            else if (DistanceKm != decimal.Round(DistanceKm.Value, 2))
            {
                yield return new ValidationResult("Use at most two decimal places.", [nameof(DistanceKm)]);
            }
        }
    }
}

public sealed class CaloriesGoalDraft
{
    [Required]
    [Range(1, int.MaxValue)]
    public int? Calories { get; set; }
}
