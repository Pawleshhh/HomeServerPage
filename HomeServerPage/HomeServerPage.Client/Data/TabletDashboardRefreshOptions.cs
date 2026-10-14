using System.Globalization;
using System.Text.Json.Serialization;

namespace HomeServerPage.Client.Data;

public sealed class TabletDashboardRefreshOptions
{
    public const string SectionName = "TabletDashboardRefresh";

    public int IntervalMinutes { get; set; } = 60;

    public string ActiveFrom { get; set; } = "07:00";

    public string ActiveUntil { get; set; } = "23:00";

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
        IntervalMinutes is > 0 and <= 1440
        && TryGetActiveRange(out _, out _);
}
