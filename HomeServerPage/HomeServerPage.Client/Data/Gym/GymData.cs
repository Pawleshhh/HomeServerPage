namespace HomeServerPage.Data.Gym;

public record GymMeasurement(
    DateTime DateTime,
    double Weight);

public enum GymExerciseKind
{
    FreeWeight,
    Bodyweight,
    Time
}

public record GymSet(
    int SetNumber,
    int Reps,
    double Weight,
    TimeSpan Time);

public record GymExercise(
    string Name,
    GymExerciseKind WeightKind,
    IReadOnlyCollection<GymSet> Sets);

public record GymSession(
    DateTime StartDateTime,
    DateTime FinishDateTime,
    IReadOnlyCollection<GymExercise> Exercises);
