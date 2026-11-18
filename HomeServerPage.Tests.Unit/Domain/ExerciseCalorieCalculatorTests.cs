using HomeServerPage.Data.Exercise;

namespace HomeServerPage.Tests.Unit.Domain;

public sealed class ExerciseCalorieCalculatorTests
{
    [Test]
    public void GymUsesFixedCaloriesAndDoesNotRequireDistance()
    {
        Assert.That(ExerciseCalorieCalculator.Calculate(ExerciseType.Gym, null), Is.EqualTo(300));
        Assert.That(ExerciseCalorieCalculator.IsValid(ExerciseType.Gym, null), Is.True);
        Assert.That(ExerciseCalorieCalculator.IsValid(ExerciseType.Gym, 1m), Is.False);
    }

    [TestCase(3, 250)]
    [TestCase(1.01, 84)]
    public void RunningUsesLinearCaloriesAndRoundsMidpointsAwayFromZero(
        decimal distanceKm,
        int expectedCalories)
    {
        Assert.That(
            ExerciseCalorieCalculator.Calculate(ExerciseType.Running, distanceKm),
            Is.EqualTo(expectedCalories));
    }

    [Test]
    public void BikeRoundsExactHalfCaloriesAwayFromZero()
    {
        Assert.That(
            ExerciseCalorieCalculator.Calculate(ExerciseType.Bike, 0.125m),
            Is.EqualTo(3));
    }

    [TestCase(25, 500)]
    [TestCase(32.5, 750)]
    [TestCase(40, 1000)]
    [TestCase(50, 1250)]
    [TestCase(1000, 25000)]
    public void BikeUsesTheExpectedPiecewiseCalories(
        decimal distanceKm,
        int expectedCalories)
    {
        Assert.That(
            ExerciseCalorieCalculator.Calculate(ExerciseType.Bike, distanceKm),
            Is.EqualTo(expectedCalories));
    }

    [TestCase(ExerciseType.Running, 0)]
    [TestCase(ExerciseType.Running, -1)]
    [TestCase(ExerciseType.Bike, 1000.01)]
    [TestCase((ExerciseType)999, 1)]
    public void CalculateRejectsInvalidInputs(ExerciseType type, decimal distanceKm)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => ExerciseCalorieCalculator.Calculate(type, distanceKm));
    }

    [Test]
    public void IsValidAcceptsOnlyPositiveTwoDecimalDistancesForRunningAndBike()
    {
        Assert.That(ExerciseCalorieCalculator.IsValid(ExerciseType.Running, 0.01m), Is.True);
        Assert.That(ExerciseCalorieCalculator.IsValid(ExerciseType.Running, 0.001m), Is.False);
        Assert.That(ExerciseCalorieCalculator.IsValid(ExerciseType.Bike, 1000m), Is.True);
        Assert.That(ExerciseCalorieCalculator.IsValid(ExerciseType.Bike, 1000.01m), Is.False);
        Assert.That(ExerciseCalorieCalculator.IsValid((ExerciseType)999, 1m), Is.False);
    }
}
