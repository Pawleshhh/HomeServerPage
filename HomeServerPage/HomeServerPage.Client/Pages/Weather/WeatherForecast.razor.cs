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
using WeatherForecastModel = HomeServerPage.Data.Weather.WeatherForecast;

namespace HomeServerPage.Client.Pages.Weather;

public partial class WeatherForecast
{
    private WeatherForecastModel? forecast;
    private bool isLoading = true;
    private string? loadError;

    private SavedLocation? location;
    private int? loadedLocationId;

    [Parameter]
    public SavedLocation? Location { get; set; }

    private string LocationLabel => this.location?.Name ?? this.forecast?.Current.Location.Name ?? "Default";

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
        await this.LoadForecastAsync();
    }

    private async Task LoadForecastAsync()
    {
        this.isLoading = true;
        try
        {
            this.forecast = await this.WeatherService.GetForecastAsync(this.location!);
            this.loadError = null;
        }
        catch (Exception exception)
        {
            this.loadError = $"Could not load the weather forecast: {exception.Message}";
        }
        finally
        {
            this.isLoading = false;
        }
    }

    private static string GetIconClass(WeatherCondition condition) =>
        WeatherConditionIcons.GetIconClass(condition.Text);
}
