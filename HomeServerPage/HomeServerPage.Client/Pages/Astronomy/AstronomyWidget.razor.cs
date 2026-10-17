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
    private bool isLoading = true;
    private string? loadError;

    protected override async Task OnInitializedAsync()
    {
        if (AutoRefresh)
        {
            DashboardRefreshService.WidgetRefreshRequested += this.HandleWidgetRefreshRequestedAsync;
        }

        await this.LoadObservationsAsync();
    }

    private Task HandleRefreshRequestedAsync() => InvokeAsync(async () =>
    {
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
            await DateTimeService.SyncAsync();
            var nowInPoland = DateTimeService.Now;
            this.requestDateUtc = DateTimeService.UtcNow;
            this.observationTime = nowInPoland;

            var solarSystemObservationsTask = AstronomyObservationLoader.LoadSolarSystemAsync(
                AstronomyService,
                this.requestDateUtc,
                AstronomyDefaults.Location);
            var moonAstronomyTask = WeatherService.GetAstronomyAsync(
                DateOnly.FromDateTime(this.requestDateUtc));
            await Task.WhenAll(solarSystemObservationsTask, moonAstronomyTask);
            var solarObservations = await solarSystemObservationsTask;
            this.solarSystemObservations = solarObservations.Where(p => p.Key != "uranus" && p.Key != "neptune").ToList();
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
        if (AutoRefresh)
        {
            DashboardRefreshService.WidgetRefreshRequested -= this.HandleWidgetRefreshRequestedAsync;
        }
    }
}
