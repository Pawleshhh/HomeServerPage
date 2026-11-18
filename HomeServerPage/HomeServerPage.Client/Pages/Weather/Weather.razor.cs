using HomeServerPage.Client.Data.Locations;
using HomeServerPage.Data.Locations;
using Microsoft.AspNetCore.Components;

namespace HomeServerPage.Client.Pages.Weather;

public partial class Weather
{
    [Inject]
    private ILocationService LocationService { get; set; } = default!;

    [Inject]
    private ILocationSelectionService LocationSelectionService { get; set; } = default!;

    private IReadOnlyList<SavedLocation> locations = [];
    private SavedLocation? selectedLocation;
    private bool isLoadingLocation = true;
    private string? locationError;
    private string WeatherWidgetKey => $"weather-widget-{this.selectedLocation?.Id}";
    private string WeatherForecastKey => $"weather-forecast-{this.selectedLocation?.Id}";

    protected override async Task OnInitializedAsync()
    {
        try
        {
            this.locations = await this.LocationService.GetLocationsAsync();
        }
        catch (Exception exception)
        {
            this.locationError = $"Unable to load saved locations: {exception.Message}";
        }

        if (this.locations.Count > 0)
        {
            try
            {
                this.selectedLocation = await this.LocationSelectionService.GetSelectedLocationAsync(
                    LocationFeature.Weather);
                this.locations = await this.LocationService.GetLocationsAsync();
            }
            catch (Exception exception)
            {
                this.locationError = $"Unable to load the weather location: {exception.Message}";
            }
        }

        this.isLoadingLocation = false;
    }

    private async Task OnLocationChangedAsync(ChangeEventArgs eventArgs)
    {
        if (!int.TryParse(eventArgs.Value?.ToString(), out var locationId))
        {
            this.locationError = "Select a valid weather location.";
            return;
        }

        this.isLoadingLocation = true;
        this.locationError = null;
        try
        {
            this.selectedLocation = await this.LocationSelectionService.SetSelectedLocationAsync(
                LocationFeature.Weather,
                locationId);
        }
        catch (Exception exception)
        {
            this.locationError = $"Unable to save the weather location: {exception.Message}";
        }
        finally
        {
            this.isLoadingLocation = false;
        }
    }
}
