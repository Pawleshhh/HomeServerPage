using System.ComponentModel.DataAnnotations;
using HomeServerPage.Client.Pages;
using HomeServerPage.Data.Exercise;

namespace HomeServerPage.Tests.Unit.Domain;

public sealed class CaloriesDraftValidationTests
{
    [Test]
    public void FoodDraftAcceptsOneDecimalGramServingAndRejectsMorePrecision()
    {
        var valid = new CaloriesFoodDraft
        {
            Name = "Oats",
            CaloriesPerServing = 150,
            GramsPerServing = 40.1m
        };
        var invalid = new CaloriesFoodDraft
        {
            Name = "Oats",
            CaloriesPerServing = 150,
            GramsPerServing = 40.12m
        };

        Assert.That(Validate(valid), Is.Empty);
        Assert.That(Validate(invalid).Single().MemberNames, Does.Contain(nameof(invalid.GramsPerServing)));
    }

    [Test]
    public void EntryDraftAcceptsTwoDecimalServingsAndRejectsMorePrecision()
    {
        var valid = new CaloriesEntryDraft { FoodId = 1, Servings = 1.25m };
        var invalid = new CaloriesEntryDraft { FoodId = 1, Servings = 1.251m };

        Assert.That(Validate(valid), Is.Empty);
        Assert.That(Validate(invalid).Single().MemberNames, Does.Contain(nameof(invalid.Servings)));
    }

    [Test]
    public void LoggedEntryDraftValidatesServingAndConsumedGramPrecision()
    {
        var draft = new CaloriesLoggedEntryDraft
        {
            Name = "Rice",
            Calories = 100,
            Time = "12:00",
            Servings = 1.234m,
            GramsConsumed = 50.1234m
        };

        var errors = Validate(draft);

        Assert.That(errors.SelectMany(error => error.MemberNames), Does.Contain(nameof(draft.Servings)));
        Assert.That(errors.SelectMany(error => error.MemberNames), Does.Contain(nameof(draft.GramsConsumed)));
    }

    [Test]
    public void ExerciseDraftRequiresDistanceOnlyForRunningAndBike()
    {
        var gym = new ExerciseDraft { Type = ExerciseType.Gym };
        var runningWithoutDistance = new ExerciseDraft { Type = ExerciseType.Running };
        var runningAtMaximum = new ExerciseDraft { Type = ExerciseType.Running, DistanceKm = 1000m };

        Assert.That(Validate(gym), Is.Empty);
        Assert.That(
            Validate(runningWithoutDistance).SelectMany(error => error.MemberNames),
            Does.Contain(nameof(runningWithoutDistance.DistanceKm)));
        Assert.That(Validate(runningAtMaximum), Is.Empty);
    }

    private static List<ValidationResult> Validate(object instance)
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(
            instance,
            new ValidationContext(instance),
            results,
            validateAllProperties: true);
        return results;
    }
}
