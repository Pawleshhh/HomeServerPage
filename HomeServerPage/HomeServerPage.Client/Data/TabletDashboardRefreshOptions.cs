using System.Globalization;
using System.Text.Json.Serialization;

namespace HomeServerPage.Client.Data;

public sealed class TabletDashboardRefreshOptions
{
    public const string SectionName = "TabletDashboardRefresh";

    public string ActiveFrom { get; set; } = "07:00";

    public string ActiveUntil { get; set; } = "23:00";

    public TabletDashboardWidgetRefreshOptions Widgets { get; set; } = new();

    public bool TryGetActiveRange(out TimeOnly activeFrom, out TimeOnly activeUntil)
    {
        var validFrom = TimeOnly.TryParseExact(
            ActiveFrom,
            "HH:mm",
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out activeFrom);
        var validUntil = TimeOnly.TryParseExact(
            ActiveUntil,
            "HH:mm",
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out activeUntil);

        return validFrom
            && validUntil
            && activeFrom < activeUntil;
    }

    [JsonIgnore]
    public bool IsValid =>
        Widgets is not null
        && Widgets.FridgeIntervalMinutes is > 0 and <= 1440
        && Widgets.PublicTransportIntervalMinutes is > 0 and <= 1440
        && Widgets.WeatherIntervalMinutes is > 0 and <= 1440
        && Widgets.AstronomyIntervalMinutes is > 0 and <= 1440
        && TryGetActiveRange(out _, out _);
}

public sealed class TabletDashboardWidgetRefreshOptions
{
    public const string Fridge = "fridge";
    public const string PublicTransport = "public-transport";
    public const string Weather = "weather";
    public const string Astronomy = "astronomy";

    public int FridgeIntervalMinutes { get; set; } = 60;

    public int PublicTransportIntervalMinutes { get; set; } = 20;

    public int WeatherIntervalMinutes { get; set; } = 60;

    public int AstronomyIntervalMinutes { get; set; } = 1440;
}
