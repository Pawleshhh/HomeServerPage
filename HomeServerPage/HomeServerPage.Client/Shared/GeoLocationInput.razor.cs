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
using AstroCalc.Core;

namespace HomeServerPage.Client.Shared;

public partial class GeoLocationInput
{
    private static int idCounter;

    private readonly string _latitudeId = $"geo-latitude-{Interlocked.Increment(ref idCounter)}";
    private readonly string _longitudeId = $"geo-longitude-{Interlocked.Increment(ref idCounter)}";
    private readonly string _elevationId = $"geo-elevation-{Interlocked.Increment(ref idCounter)}";

    [Parameter]
    public GeographicCoordinate Value { get; set; }

    [Parameter]
    public EventCallback<GeographicCoordinate> ValueChanged { get; set; }

    [Parameter]
    public bool ShowElevation { get; set; } = true;

    private Task OnLatitudeChanged(ChangeEventArgs e) =>
        UpdateValue(latitude: ParseDouble(e.Value, Value.Latitude.Degrees));

    private Task OnLongitudeChanged(ChangeEventArgs e) =>
        UpdateValue(longitude: ParseDouble(e.Value, Value.Longitude.Degrees));

    private Task OnElevationChanged(ChangeEventArgs e) =>
        UpdateValue(elevation: ParseDouble(e.Value, Value.ElevationMeters));

    private Task UpdateValue(double? latitude = null, double? longitude = null, double? elevation = null)
    {
        var newValue = GeographicCoordinate.FromDegrees(
            latitude ?? Value.Latitude.Degrees,
            longitude ?? Value.Longitude.Degrees,
            elevation ?? Value.ElevationMeters);

        Value = newValue;

        return ValueChanged.InvokeAsync(newValue);
    }

    private static double ParseDouble(object? value, double fallback) =>
        double.TryParse(
            value?.ToString(),
            System.Globalization.NumberStyles.Float,
            System.Globalization.CultureInfo.InvariantCulture,
            out var result)
            ? result
            : fallback;
}
