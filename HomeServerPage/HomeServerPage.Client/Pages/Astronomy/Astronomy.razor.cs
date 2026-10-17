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
using HomeServerPage.Data.Astronomy;

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

    private GeographicCoordinate location = AstronomyDefaults.Location;
    private IReadOnlyList<ObservationPoint> observationPoints = [];
    private int? selectedObservationPointId;
    private DateTime selectedLocalDateTime;
    private DateTime requestDateUtc;
    private string activeTab = astronomyTabs[0].Key;
    private bool isLoadingObservationPoints;
    private string? observationPointLoadError;

    private IAstronomyTab CurrentTab => astronomyTabs.First(tab => tab.Key == activeTab);

    private ObservationPoint? SelectedObservationPoint => selectedObservationPointId is int id
        ? observationPoints.FirstOrDefault(observationPoint => observationPoint.Id == id)
        : null;

    private IDictionary<string, object?> CurrentTabParameters =>
        CurrentTab.ComponentType switch
        {
            _ when CurrentTab.ComponentType == typeof(AstronomyObservationsTab) => new Dictionary<string, object?>
            {
                [nameof(AstronomyObservationsTab.Location)] = location,
                [nameof(AstronomyObservationsTab.RequestDateUtc)] = requestDateUtc
            },
            _ when CurrentTab.ComponentType == typeof(AstronomyObservationPointsTab) => new Dictionary<string, object?>
            {
                [nameof(AstronomyObservationPointsTab.Location)] = location,
                [nameof(AstronomyObservationPointsTab.SelectedLocalDateTime)] = selectedLocalDateTime,
                [nameof(AstronomyObservationPointsTab.ObservationPoints)] = observationPoints,
                [nameof(AstronomyObservationPointsTab.SelectedObservationPoint)] = SelectedObservationPoint,
                [nameof(AstronomyObservationPointsTab.ObservationPointSaved)] =
                    EventCallback.Factory.Create<ObservationPoint>(this, OnObservationPointSaved),
                [nameof(AstronomyObservationPointsTab.ObservationPointDeleted)] =
                    EventCallback.Factory.Create<int>(this, OnObservationPointDeleted),
                [nameof(AstronomyObservationPointsTab.ObservationPointSelected)] =
                    EventCallback.Factory.Create<int?>(this, OnObservationPointSelectedFromTab)
            },
            _ => new Dictionary<string, object?>()
        };

    private bool IsActive(IAstronomyTab tab) => tab.Key == activeTab;

    private void ShowTab(string tabKey) => activeTab = tabKey;

    protected override async Task OnInitializedAsync()
    {
        var nowInPoland = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, AstronomyDefaults.LocalTimeZone);
        selectedLocalDateTime = nowInPoland.DateTime;
        UpdateRequestDate();

        await LoadObservationPointsAsync();
    }

    private Task OnLocationChanged(GeographicCoordinate newLocation)
    {
        location = newLocation;

        if (SelectedObservationPoint is not null && !CoordinatesMatch(SelectedObservationPoint, newLocation))
        {
            selectedObservationPointId = null;
        }

        return Task.CompletedTask;
    }

    private Task OnSelectedDateTimeChanged()
    {
        UpdateRequestDate();
        return Task.CompletedTask;
    }

    private void UpdateRequestDate()
    {
        requestDateUtc = TimeZoneInfo.ConvertTimeToUtc(
            DateTime.SpecifyKind(selectedLocalDateTime, DateTimeKind.Unspecified),
            AstronomyDefaults.LocalTimeZone);
    }

    private async Task LoadObservationPointsAsync()
    {
        isLoadingObservationPoints = true;
        observationPointLoadError = null;

        try
        {
            observationPoints = await ObservationPointService.GetObservationPointsAsync();
        }
        catch (Exception exception)
        {
            observationPoints = [];
            observationPointLoadError = $"Unable to load observation points: {exception.Message}";
        }
        finally
        {
            isLoadingObservationPoints = false;
        }
    }

    private Task OnObservationPointSelectedFromTab(int? observationPointId)
    {
        selectedObservationPointId = observationPointId;

        if (SelectedObservationPoint is not null)
        {
            location = SelectedObservationPoint.Coordinates;
        }

        return Task.CompletedTask;
    }

    private Task OnObservationPointSelected()
    {
        var selectedPoint = SelectedObservationPoint;
        if (selectedPoint is not null)
        {
            location = selectedPoint.Coordinates;
        }

        return Task.CompletedTask;
    }

    private async Task OnObservationPointSaved(ObservationPoint observationPoint)
    {
        await LoadObservationPointsAsync();
        selectedObservationPointId = observationPoint.Id;
        location = observationPoint.Coordinates;
    }

    private async Task OnObservationPointDeleted(int observationPointId)
    {
        var wasSelected = selectedObservationPointId == observationPointId;
        await LoadObservationPointsAsync();

        if (wasSelected)
        {
            selectedObservationPointId = null;
            location = AstronomyDefaults.Location;
        }
    }

    private static bool CoordinatesMatch(ObservationPoint observationPoint, GeographicCoordinate coordinate) =>
        observationPoint.Latitude == coordinate.Latitude.Degrees
        && observationPoint.Longitude == coordinate.Longitude.Degrees
        && (observationPoint.ElevationMeters ?? 0) == coordinate.ElevationMeters;
}
