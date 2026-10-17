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
using AstroCalc.Observation;
using HomeServerPage.Data.Astronomy;

namespace HomeServerPage.Client.Pages.Astronomy;

public partial class PlanetObservationList
{
    [Parameter, EditorRequired]
    public IReadOnlyList<AstronomyObservation> Observations { get; set; } = [];

    [Parameter]
    public DateTime RequestDateUtc { get; set; }

    [Parameter]
    public bool Compact { get; set; }

    [Parameter]
    public bool TableLayout { get; set; }

    private IEnumerable<AstronomyObservation> VisibleObservations =>
        Observations;

    private static string GetObservationImageUrl(string key) =>
        $"images/astronomy/solarsystem/{key}.png";

    private IEnumerable<(string Label, string Value)> GetResultValues(RiseTransitSetResult result)
    {
        yield return ("Rise", result.RiseUT.FormatUtHours(RequestDateUtc, AstronomyDefaults.LocalTimeZone));

        if (!Compact)
        {
            yield return ("Transit", result.TransitUT.FormatUtHours(RequestDateUtc, AstronomyDefaults.LocalTimeZone));
        }

        yield return ("Set", result.SetUT.FormatUtHours(RequestDateUtc, AstronomyDefaults.LocalTimeZone));

        if (!Compact && result.RisesAndSets)
        {
            yield return ("Above horizon", FormatDuration(result.HoursAboveHorizon));
        }
    }

    private static string FormatDuration(double hours)
    {
        if (double.IsNaN(hours))
        {
            return "—";
        }

        var timeSpan = TimeSpan.FromHours(hours);
        return $"{(int)timeSpan.TotalHours}h {timeSpan.Minutes:D2}m";
    }
}
