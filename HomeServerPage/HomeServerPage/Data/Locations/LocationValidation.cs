using HomeServerPage.Data.Locations;

namespace HomeServerPage.Data.Locations;

internal static class LocationValidation
{
    public static void Validate(SavedLocation location)
    {
        ArgumentNullException.ThrowIfNull(location);

        if (string.IsNullOrWhiteSpace(location.Name))
        {
            throw new ArgumentException("A location name is required.", nameof(location));
        }

        if (location.Name.Trim().Length > 100)
        {
            throw new ArgumentException("A location name cannot exceed 100 characters.", nameof(location));
        }

        if (!double.IsFinite(location.Latitude) || location.Latitude is < -90 or > 90)
        {
            throw new ArgumentException("Latitude must be a finite value between -90 and 90.", nameof(location));
        }

        if (!double.IsFinite(location.Longitude) || location.Longitude is < -180 or > 180)
        {
            throw new ArgumentException("Longitude must be a finite value between -180 and 180.", nameof(location));
        }

        if (!double.IsFinite(location.ElevationMeters))
        {
            throw new ArgumentException("Elevation must be a finite value.", nameof(location));
        }

        var timeZoneId = location.TimeZoneId?.Trim();
        if (string.IsNullOrWhiteSpace(timeZoneId) || timeZoneId.Length > 100)
        {
            throw new ArgumentException("A valid time-zone identifier is required.", nameof(location));
        }

        try
        {
            _ = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        }
        catch (TimeZoneNotFoundException exception)
        {
            throw new ArgumentException(
                $"Time zone '{timeZoneId}' is not supported by this server.",
                nameof(location),
                exception);
        }
        catch (InvalidTimeZoneException exception)
        {
            throw new ArgumentException(
                $"Time zone '{timeZoneId}' is invalid on this server.",
                nameof(location),
                exception);
        }
    }
}
