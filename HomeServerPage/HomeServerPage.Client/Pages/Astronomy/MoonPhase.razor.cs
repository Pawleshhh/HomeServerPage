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

namespace HomeServerPage.Client.Pages.Astronomy;

public partial class MoonPhase
{
    [Parameter]
    public WeatherAstronomy? Astronomy { get; set; }

    [Parameter]
    public bool Large { get; set; }

    private string PhaseClass => Astronomy?.MoonPhase.Trim().ToLowerInvariant() switch
    {
        "new moon" => "moon-phase-new",
        "waxing crescent" => "moon-phase-waxing-crescent",
        "first quarter" => "moon-phase-first-quarter",
        "waxing gibbous" => "moon-phase-waxing-gibbous",
        "full moon" => "moon-phase-full",
        "waning gibbous" => "moon-phase-waning-gibbous",
        "last quarter" or "third quarter" => "moon-phase-last-quarter",
        "waning crescent" => "moon-phase-waning-crescent",
        _ => "moon-phase-full"
    };
}
