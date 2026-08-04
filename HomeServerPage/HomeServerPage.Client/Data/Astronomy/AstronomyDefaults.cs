using AstroCalc.Core;

namespace HomeServerPage.Data.Astronomy;

public static class AstronomyDefaults
{
    public const double Latitude = 53.42890170607857;
    public const double Longitude = 14.552826366632289;

    public static readonly TimeZoneInfo LocalTimeZone =
        TimeZoneInfo.FindSystemTimeZoneById("Europe/Warsaw");

    public static GeographicCoordinate Location =>
        new(Angle.FromDegrees(Latitude), Angle.FromDegrees(Longitude), 0);
}
