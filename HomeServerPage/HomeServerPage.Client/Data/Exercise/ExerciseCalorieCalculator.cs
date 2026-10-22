namespace HomeServerPage.Data.Exercise;

public static class ExerciseCalorieCalculator
{
    public const int GymCalories = 300;
    public const decimal MaxDistanceKm = 1000m;

    private const decimal RunningCaloriesPerKm = 250m / 3m;
    private const decimal BikeShortDistanceKm = 25m;
    private const decimal BikeLongDistanceKm = 40m;
    private const decimal BikeShortDistanceCalories = 500m;
    private const decimal BikeLongDistanceCalories = 1000m;

    public static bool RequiresDistance(ExerciseType type) => type is ExerciseType.Running or ExerciseType.Bike;

    public static string GetDisplayName(ExerciseType type) => type switch
    {
        ExerciseType.Gym => "Gym",
        ExerciseType.Running => "Running",
        ExerciseType.Bike => "Bike",
        _ => type.ToString()
    };

    /// <summary>
    /// Gym is a fixed session. Running is linear (250 kcal per 3 km). Bike is 20 kcal/km up to 25 km
    /// (500 kcal), interpolates linearly to 1000 kcal at 40 km, and continues at 25 kcal/km beyond that.
    /// </summary>
    public static int Calculate(ExerciseType type, decimal? distanceKm)
    {
        if (type == ExerciseType.Gym)
        {
            return GymCalories;
        }

        if (distanceKm is not decimal km || km <= 0m || km > MaxDistanceKm)
        {
            throw new ArgumentOutOfRangeException(nameof(distanceKm), "A positive distance is required.");
        }

        var calories = type switch
        {
            ExerciseType.Running => km * RunningCaloriesPerKm,
            ExerciseType.Bike => CalculateBike(km),
            _ => throw new ArgumentOutOfRangeException(nameof(type))
        };

        return Math.Max(1, (int)Math.Round(calories, MidpointRounding.AwayFromZero));
    }

    public static bool IsValid(ExerciseType type, decimal? distanceKm) => type switch
    {
        ExerciseType.Gym => distanceKm is null,
        ExerciseType.Running or ExerciseType.Bike =>
            distanceKm is decimal km && km > 0m && km <= MaxDistanceKm && km == decimal.Round(km, 2),
        _ => false
    };

    private static decimal CalculateBike(decimal km)
    {
        if (km <= BikeShortDistanceKm)
        {
            return km * BikeShortDistanceCalories / BikeShortDistanceKm;
        }

        if (km < BikeLongDistanceKm)
        {
            return BikeShortDistanceCalories +
                (km - BikeShortDistanceKm) *
                (BikeLongDistanceCalories - BikeShortDistanceCalories) /
                (BikeLongDistanceKm - BikeShortDistanceKm);
        }

        return km * BikeLongDistanceCalories / BikeLongDistanceKm;
    }
}
