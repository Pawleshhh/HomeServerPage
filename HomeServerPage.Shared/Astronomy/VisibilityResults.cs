namespace HomeServerPage.Data.Astronomy;

public enum AstronomyObjectType
{
    Sun,
    Moon,
    Planet,
    DeepSky
}

public sealed record VisibilityWindow(
    string ObjectKey,
    string ObjectName,
    AstronomyObjectType ObjectType,
    HorizonDirection Direction,
    DateTimeOffset StartUtc,
    DateTimeOffset EndUtc,
    double MaximumAltitudeDegrees)
{
    public TimeSpan Duration => EndUtc - StartUtc;
}

public sealed record VisibleAstronomyObject(
    string Key,
    string Name,
    AstronomyObjectType ObjectType,
    IReadOnlyList<VisibilityWindow> Windows)
{
    public TimeSpan TotalVisibleDuration => TimeSpan.FromTicks(
        Windows.Sum(window => window.Duration.Ticks));
}
