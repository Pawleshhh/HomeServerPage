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
using HomeServerPage.Data;
using HomeServerPage.Data.Astronomy;
using HomeServerPage.Data.Weather;
using HomeServerPage.Client.Data;
using HomeServerPage.Data.Locations;

namespace HomeServerPage.Client.Pages.Astronomy;

public partial class AstronomyWidget
{
    [Parameter]
    public string DetailsHref { get; set; } = "/astronomy";

    [Parameter]
    public bool CompactTablet { get; set; }

    [Parameter]
    public bool AutoRefresh { get; set; }

    private IReadOnlyList<AstronomyObservation> solarSystemObservations = [];
    private WeatherAstronomy? moonAstronomy;
    private DateTime requestDateUtc;
    private DateTimeOffset? observationTime;
    private SavedLocation? location;
    private TimeZoneInfo LocationTimeZone =>
        TimeZoneInfo.FindSystemTimeZoneById(this.location?.TimeZoneId ?? "Etc/UTC");
    private bool isLoading = true;
    private string? loadError;

    protected override async Task OnInitializedAsync()
    {
        if (this.AutoRefresh)
        {
            this.DashboardRefreshService.WidgetRefreshRequested += this.HandleWidgetRefreshRequestedAsync;
        }

        await this.LoadObservationsAsync();
    }

    private Task HandleRefreshRequestedAsync() => InvokeAsync(async () =>
    {
        this.isLoading = true;
        this.StateHasChanged();
        await this.LoadObservationsAsync();
        this.StateHasChanged();
    });

    private Task HandleWidgetRefreshRequestedAsync(string widgetId) =>
        widgetId == TabletDashboardWidgetRefreshOptions.Astronomy
            ? this.HandleRefreshRequestedAsync()
            : Task.CompletedTask;

    private async Task LoadObservationsAsync()
    {
        this.isLoading = true;
        this.loadError = null;
        this.solarSystemObservations = [];
        this.moonAstronomy = null;

        try
        {
            await this.DateTimeService.SyncAsync();
            this.location = await this.LocationSelectionService.GetSelectedLocationAsync(
                LocationFeature.Astronomy);
            var locationTimeZone = TimeZoneInfo.FindSystemTimeZoneById(this.location.TimeZoneId);
            var utcNow = DateTime.SpecifyKind(this.DateTimeService.UtcNow, DateTimeKind.Utc);
            this.requestDateUtc = utcNow;
            this.observationTime = TimeZoneInfo.ConvertTime(
                new DateTimeOffset(utcNow),
                locationTimeZone);
            var localDate = DateOnly.FromDateTime(this.observationTime.Value.DateTime);

            var solarSystemObservationsTask = AstronomyObservationLoader.LoadSolarSystemAsync(
                this.AstronomyService,
                this.requestDateUtc,
                this.location.Coordinates);
            var moonAstronomyTask = this.WeatherService.GetAstronomyAsync(
                this.location,
                localDate);
            await Task.WhenAll(solarSystemObservationsTask, moonAstronomyTask);
            var solarObservations = await solarSystemObservationsTask;
            this.solarSystemObservations = solarObservations
                .Where(observation => observation.Key != "uranus" && observation.Key != "neptune")
                .ToList();
            this.moonAstronomy = await moonAstronomyTask;
        }
        catch (Exception exception)
        {
            this.loadError = $"Unable to load celestial observations: {exception.Message}";
        }
        finally
        {
            this.isLoading = false;
        }
    }

    public void Dispose()
    {
        if (this.AutoRefresh)
        {
            this.DashboardRefreshService.WidgetRefreshRequested -= this.HandleWidgetRefreshRequestedAsync;
        }
    }
}
