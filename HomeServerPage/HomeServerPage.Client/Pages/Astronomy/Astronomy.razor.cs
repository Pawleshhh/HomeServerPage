using AstroCalc.Core;
using HomeServerPage.Client.Data;
using HomeServerPage.Client.Data.Locations;
using HomeServerPage.Data.Astronomy;
using HomeServerPage.Data.Locations;
using Microsoft.AspNetCore.Components;

namespace HomeServerPage.Client.Pages.Astronomy;

public partial class Astronomy
{
    private static readonly IReadOnlyList<IAstronomyTab> astronomyTabs =
    [
        new AstronomyTab("observations", "Observations", "observations-panel", typeof(AstronomyObservationsTab)),
        new AstronomyTab("tools", "Astronomy tools", "tools-panel", typeof(AstronomyToolsTab)),
        new AstronomyTab("equipment", "Equipment", "equipment-panel", typeof(AstronomyEquipmentTab)),
        new AstronomyTab("observation-points", "Observation points", "observation-points-panel", typeof(AstronomyObservationPointsTab))
    ];

    [Inject]
    private ILocationService LocationService { get; set; } = default!;

    [Inject]
    private ILocationSelectionService LocationSelectionService { get; set; } = default!;

    private SavedLocation? selectedLocation;
    private IReadOnlyList<SavedLocation> locations = [];
    private IReadOnlyList<ObservationPoint> observationPoints = [];
    private int? selectedObservationPointId;
    private DateTime selectedLocalDateTime;
    private DateTime requestDateUtc;
    private string activeTab = astronomyTabs[0].Key;
    private bool isLoadingLocations = true;
    private bool isLoadingObservationPoints;
    private string? locationError;
    private string? observationPointLoadError;

    private IAstronomyTab CurrentTab => astronomyTabs.First(tab => tab.Key == this.activeTab);

    private ObservationPoint? SelectedObservationPoint => this.selectedObservationPointId is int id
        ? this.observationPoints.FirstOrDefault(observationPoint => observationPoint.Id == id)
        : null;

    private IDictionary<string, object?> CurrentTabParameters =>
        this.CurrentTab.ComponentType switch
        {
            _ when this.CurrentTab.ComponentType == typeof(AstronomyObservationsTab) => new Dictionary<string, object?>
            {
                [nameof(AstronomyObservationsTab.Location)] = this.selectedLocation,
                [nameof(AstronomyObservationsTab.RequestDateUtc)] = this.requestDateUtc
            },
            _ when this.CurrentTab.ComponentType == typeof(AstronomyObservationPointsTab) => new Dictionary<string, object?>
            {
                [nameof(AstronomyObservationPointsTab.Location)] = this.selectedLocation,
                [nameof(AstronomyObservationPointsTab.Locations)] = this.locations,
                [nameof(AstronomyObservationPointsTab.SelectedLocalDateTime)] = this.selectedLocalDateTime,
                [nameof(AstronomyObservationPointsTab.ObservationPoints)] = this.observationPoints,
                [nameof(AstronomyObservationPointsTab.SelectedObservationPoint)] = this.SelectedObservationPoint,
                [nameof(AstronomyObservationPointsTab.ObservationPointSaved)] =
                    EventCallback.Factory.Create<ObservationPoint>(this, this.OnObservationPointSaved),
                [nameof(AstronomyObservationPointsTab.ObservationPointDeleted)] =
                    EventCallback.Factory.Create<int>(this, this.OnObservationPointDeleted),
                [nameof(AstronomyObservationPointsTab.ObservationPointSelected)] =
                    EventCallback.Factory.Create<int?>(this, this.OnObservationPointSelectedFromTab)
            },
            _ => new Dictionary<string, object?>()
        };

    private bool IsActive(IAstronomyTab tab) => tab.Key == this.activeTab;

    private void ShowTab(string tabKey) => this.activeTab = tabKey;

    protected override async Task OnInitializedAsync()
    {
        this.isLoadingLocations = true;
        this.locationError = null;

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
                await this.DateTimeService.SyncAsync();
                this.selectedLocation = await this.LocationSelectionService.GetSelectedLocationAsync(
                    LocationFeature.Astronomy);
                this.InitializeLocalDateTime(this.selectedLocation, null);
            }
            catch (Exception exception)
            {
                this.locationError = $"Unable to load the astronomy location: {exception.Message}";
            }
        }

        await this.LoadObservationPointsAsync();
        this.isLoadingLocations = false;
    }

    private async Task OnLocationChangedAsync(ChangeEventArgs eventArgs)
    {
        if (!int.TryParse(eventArgs.Value?.ToString(), out var locationId))
        {
            this.locationError = "Select a valid astronomy location.";
            return;
        }

        this.locationError = null;

        try
        {
            if (this.requestDateUtc == default)
            {
                await this.DateTimeService.SyncAsync();
            }

            var currentInstant = this.requestDateUtc == default
                ? new DateTimeOffset(
                    DateTime.SpecifyKind(this.DateTimeService.UtcNow, DateTimeKind.Utc))
                : new DateTimeOffset(
                    DateTime.SpecifyKind(this.requestDateUtc, DateTimeKind.Utc));
            this.selectedLocation = await this.LocationSelectionService.SetSelectedLocationAsync(
                LocationFeature.Astronomy,
                locationId);
            if (this.SelectedObservationPoint?.LocationId != locationId)
            {
                this.selectedObservationPointId = null;
            }

            this.InitializeLocalDateTime(this.selectedLocation, currentInstant);
        }
        catch (Exception exception)
        {
            this.locationError = $"Unable to save the astronomy location: {exception.Message}";
        }
    }

    private Task OnSelectedDateTimeChanged()
    {
        this.UpdateRequestDate();
        return Task.CompletedTask;
    }

    private void InitializeLocalDateTime(SavedLocation location, DateTimeOffset? instant)
    {
        var timeZone = TimeZoneInfo.FindSystemTimeZoneById(location.TimeZoneId);
        var selectedInstant = instant
            ?? new DateTimeOffset(
                DateTime.SpecifyKind(this.DateTimeService.UtcNow, DateTimeKind.Utc));
        this.selectedLocalDateTime = TimeZoneInfo.ConvertTime(selectedInstant, timeZone).DateTime;
        this.UpdateRequestDate();
    }

    private void UpdateRequestDate()
    {
        if (this.selectedLocation is null)
        {
            return;
        }

        var timeZone = TimeZoneInfo.FindSystemTimeZoneById(this.selectedLocation.TimeZoneId);
        this.requestDateUtc = TimeZoneInfo.ConvertTimeToUtc(
            DateTime.SpecifyKind(this.selectedLocalDateTime, DateTimeKind.Unspecified),
            timeZone);
    }

    private async Task LoadObservationPointsAsync()
    {
        this.isLoadingObservationPoints = true;
        this.observationPointLoadError = null;

        try
        {
            this.observationPoints = await this.ObservationPointService.GetObservationPointsAsync();
        }
        catch (Exception exception)
        {
            this.observationPoints = [];
            this.observationPointLoadError = $"Unable to load observation points: {exception.Message}";
        }
        finally
        {
            this.isLoadingObservationPoints = false;
        }
    }

    private async Task OnObservationPointSelectedFromTab(int? observationPointId)
    {
        this.selectedObservationPointId = observationPointId;
        if (this.SelectedObservationPoint?.LocationId is int locationId)
        {
            await this.SelectLocationAsync(locationId);
        }
    }

    private async Task OnObservationPointSaved(ObservationPoint observationPoint)
    {
        await this.LoadObservationPointsAsync();
        this.selectedObservationPointId = observationPoint.Id;

        if (observationPoint.LocationId is int locationId)
        {
            await this.SelectLocationAsync(locationId);
        }
    }

    private async Task OnObservationPointDeleted(int observationPointId)
    {
        if (this.selectedObservationPointId == observationPointId)
        {
            this.selectedObservationPointId = null;
        }

        await this.LoadObservationPointsAsync();
    }

    private async Task SelectLocationAsync(int locationId)
    {
        if (this.selectedLocation?.Id == locationId)
        {
            return;
        }

        var currentInstant = new DateTimeOffset(
            DateTime.SpecifyKind(this.requestDateUtc, DateTimeKind.Utc));
        this.selectedLocation = await this.LocationSelectionService.SetSelectedLocationAsync(
            LocationFeature.Astronomy,
            locationId);
        this.InitializeLocalDateTime(this.selectedLocation, currentInstant);
    }
}
