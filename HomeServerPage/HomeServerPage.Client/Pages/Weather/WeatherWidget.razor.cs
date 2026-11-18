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
using HomeServerPage.Data.Locations;
using HomeServerPage.Client.Data;

namespace HomeServerPage.Client.Pages.Weather;

public partial class WeatherWidget
{
    private WeatherSnapshot? snapshot;
    private bool isLoading = true;
    private string? loadError;
    private WeatherForecastDay? forecastDay;
    private WeatherForecastDay? nextDayForecast;
    private SavedLocation? location;
    private int? loadedLocationId;

    [Parameter]
    public SavedLocation? Location { get; set; }

    [Parameter]
    public bool AutoRefresh { get; set; }

    private string LocationLabel => this.location?.Name ?? this.snapshot?.Location.Name ?? "Default";
    private string WeatherIconClass => WeatherConditionIcons.GetIconClass(this.snapshot?.Condition.Text);
    private int RainChancePercent => this.forecastDay?.ChanceOfRainPercent ?? 0;

    protected override Task OnInitializedAsync()
    {
        if (this.AutoRefresh)
        {
            this.DashboardRefreshService.WidgetRefreshRequested += this.HandleWidgetRefreshRequestedAsync;
        }

        return Task.CompletedTask;
    }

    protected override async Task OnParametersSetAsync()
    {
        var selectedLocation = this.Location ?? await this.LocationSelectionService.GetSelectedLocationAsync(
            LocationFeature.Weather);
        if (this.loadedLocationId == selectedLocation.Id)
        {
            return;
        }

        this.location = selectedLocation;
        this.loadedLocationId = selectedLocation.Id;
        await this.LoadWeatherAsync();
    }

    private async Task LoadWeatherAsync()
    {
        this.isLoading = true;
        this.loadError = null;

        try
        {
            var selectedLocation = this.Location
                ?? await this.LocationSelectionService.GetSelectedLocationAsync(LocationFeature.Weather);
            this.location = selectedLocation;
            this.loadedLocationId = selectedLocation.Id;
            var forecast = await this.WeatherService.GetForecastAsync(selectedLocation, 2);
            this.snapshot = forecast.Current;
            this.forecastDay = forecast.Days.FirstOrDefault();
            this.nextDayForecast = forecast.Days.Skip(1).FirstOrDefault();
        }
        catch (Exception exception)
        {
            this.loadError = $"Could not load weather: {exception.Message}";
        }
        finally
        {
            this.isLoading = false;
        }
    }

    private Task HandleRefreshRequestedAsync() => InvokeAsync(async () =>
    {
        this.isLoading = true;
        this.StateHasChanged();
        await this.LoadWeatherAsync();
        this.StateHasChanged();
    });

    private Task HandleWidgetRefreshRequestedAsync(string widgetId) =>
        widgetId == TabletDashboardWidgetRefreshOptions.Weather
            ? this.HandleRefreshRequestedAsync()
            : Task.CompletedTask;

    public void Dispose()
    {
        if (this.AutoRefresh)
        {
            this.DashboardRefreshService.WidgetRefreshRequested -= this.HandleWidgetRefreshRequestedAsync;
        }
    }
}
