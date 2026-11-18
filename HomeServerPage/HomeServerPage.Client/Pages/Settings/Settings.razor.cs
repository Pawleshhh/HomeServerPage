using HomeServerPage.Client.Data.Locations;
using HomeServerPage.Data.Locations;
using Microsoft.AspNetCore.Components;

namespace HomeServerPage.Client.Pages.Settings;

public partial class Settings
{
    [Inject]
    private ILocationService LocationService { get; set; } = default!;

    private IReadOnlyList<SavedLocation> locations = [];
    private SavedLocation editLocation = NewLocation();
    private bool isLoading = true;
    private bool isSaving;
    private bool isDeleting;
    private string? loadError;
    private string? saveError;

    protected override async Task OnInitializedAsync()
    {
        await this.LoadLocationsAsync();
    }

    private void StartNewLocation()
    {
        this.editLocation = NewLocation();
        this.saveError = null;
    }

    private void SelectLocation(SavedLocation location)
    {
        this.editLocation = Clone(location);
        this.saveError = null;
    }

    private async Task SaveLocationAsync()
    {
        this.isSaving = true;
        this.saveError = null;

        try
        {
            var savedLocation = await this.LocationService.SaveLocationAsync(this.editLocation);
            await this.LoadLocationsAsync();
            this.editLocation = Clone(savedLocation);
        }
        catch (Exception exception)
        {
            this.saveError = $"Unable to save location: {exception.Message}";
        }
        finally
        {
            this.isSaving = false;
        }
    }

    private async Task DeleteLocationAsync()
    {
        this.isDeleting = true;
        this.saveError = null;

        try
        {
            await this.LocationService.DeleteLocationAsync(this.editLocation.Id);
            await this.LoadLocationsAsync();
            this.editLocation = NewLocation();
        }
        catch (Exception exception)
        {
            this.saveError = $"Unable to delete location: {exception.Message}";
        }
        finally
        {
            this.isDeleting = false;
        }
    }

    private async Task LoadLocationsAsync()
    {
        this.isLoading = true;
        this.loadError = null;

        try
        {
            this.locations = await this.LocationService.GetLocationsAsync();
        }
        catch (Exception exception)
        {
            this.locations = [];
            this.loadError = $"Unable to load locations: {exception.Message}";
        }
        finally
        {
            this.isLoading = false;
        }
    }

    private static SavedLocation NewLocation() => new()
    {
        TimeZoneId = "Europe/Warsaw"
    };

    private static SavedLocation Clone(SavedLocation location) => new()
    {
        Id = location.Id,
        Name = location.Name,
        Latitude = location.Latitude,
        Longitude = location.Longitude,
        ElevationMeters = location.ElevationMeters,
        TimeZoneId = location.TimeZoneId,
        IsSystemDefault = location.IsSystemDefault
    };
}
