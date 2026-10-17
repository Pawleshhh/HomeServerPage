using Microsoft.AspNetCore.Components;
using System.Net.Http;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.Web;
using static Microsoft.AspNetCore.Components.Web.RenderMode;
using Microsoft.AspNetCore.Components.Web.Virtualization;
using Microsoft.JSInterop;
using HomeServerPage.Client;
using HomeServerPage.Client.Layout;
using HomeServerPage.Client.Shared;
using HomeServerPage.Shared.Extensions;
using HomeServerPage.Data.Weather;
using System.Globalization;

namespace HomeServerPage.Client.Pages.Astronomy;

public partial class MoonPhase
{
    [Parameter]
    public WeatherAstronomy? Astronomy { get; set; }

    [Parameter]
    public bool Large { get; set; }

    [Parameter, EditorRequired]
    public DateTime ObservationDateUtc { get; set; }

    private string? failedImageUrl;

    private double Illumination => Math.Clamp(Astronomy?.MoonIlluminationPercent ?? 100, 0, 100) / 100d;

    private bool IsWaxing => Astronomy?.MoonPhase.Trim().ToLowerInvariant() switch
    {
        "waxing crescent" or "first quarter" or "waxing gibbous" => true,
        "waning crescent" or "last quarter" or "third quarter" or "waning gibbous" => false,
        _ => true
    };

    private string? LitAreaPath
    {
        get
        {
            if (Illumination <= 0)
            {
                return null;
            }

            var outerSweep = IsWaxing ? 1 : 0;
            var terminatorSweep = IsWaxing == (Illumination < 0.5) ? 0 : 1;
            var terminatorRadius = Math.Abs(1 - (2 * Illumination)) * 50;

            return $"M 50 1 A 49 49 0 0 {outerSweep} 50 99 A {terminatorRadius.ToString("0.###", CultureInfo.InvariantCulture)} 49 0 0 {terminatorSweep} 50 1 Z";
        }
    }

    private string? MoonImageUrl => GetMoonImageUrls().ImageUrl;

    private string? MoonVisualizationUrl => GetMoonImageUrls().VisualizationUrl;

    private bool ShouldDisplayImage =>
        MoonImageUrl is { } imageUrl && !string.Equals(this.failedImageUrl, imageUrl, StringComparison.Ordinal);

    private (string? ImageUrl, string? VisualizationUrl) GetMoonImageUrls()
    {
        var dateUtc = DateTime.SpecifyKind(ObservationDateUtc, DateTimeKind.Utc);
        var (groupId, visualizationId) = dateUtc.Year switch
        {
            2025 => (5400, 5415),
            2026 => (5500, 5587),
            _ => (0, 0)
        };

        if (visualizationId == 0)
        {
            return (null, null);
        }

        var frame = ((dateUtc.DayOfYear - 1) * 24) + dateUtc.Hour + 1;
        var frameName = frame.ToString("D4", CultureInfo.InvariantCulture);
        var baseUrl = $"https://svs.gsfc.nasa.gov/vis/a000000/a{groupId:D6}/a{visualizationId:D6}";

        return (
            $"{baseUrl}/frames/730x730_1x1_30p/moon.{frameName}.jpg",
            $"https://svs.gsfc.nasa.gov/{visualizationId}/");
    }

    private void HandleMoonImageError()
    {
        this.failedImageUrl = MoonImageUrl;
    }
}
