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

public partial class AstronomyObservationPointsTab
{
    private static readonly HorizonDirection[] Directions = Enum.GetValues<HorizonDirection>();
    private static readonly AstronomyObjectType[] ObjectTypes = Enum.GetValues<AstronomyObjectType>();
    private static readonly ObservationPointVisibilityCalculator VisibilityCalculator = new();

    [Parameter, EditorRequired]
    public GeographicCoordinate Location { get; set; }

    [Parameter, EditorRequired]
    public DateTime SelectedLocalDateTime { get; set; }

    [Parameter]
    public IReadOnlyList<ObservationPoint> ObservationPoints { get; set; } = [];

    [Parameter]
    public ObservationPoint? SelectedObservationPoint { get; set; }

    [Parameter]
    public EventCallback<ObservationPoint> ObservationPointSaved { get; set; }

    [Parameter]
    public EventCallback<int> ObservationPointDeleted { get; set; }

    [Parameter]
    public EventCallback<int?> ObservationPointSelected { get; set; }

    private readonly HashSet<HorizonDirection> selectedDirections = Enum.GetValues<HorizonDirection>().ToHashSet();
    private readonly HashSet<AstronomyObjectType> selectedObjectTypes = Enum.GetValues<AstronomyObjectType>().ToHashSet();

    private readonly ObservationPointVisibilityCalculator visibilityCalculator = VisibilityCalculator;
    private ObservationPoint editPoint = new();
    private GeographicCoordinate editLocation = AstronomyDefaults.Location;
    private int? editorPointId;
    private DateTime rangeStartLocal;
    private DateTime rangeEndLocal;
    private DateTime rangeDate;
    private bool hasEditorState;
    private bool hasRange;
    private bool isSaving;
    private bool isDeleting;
    private bool isCalculating;
    private string? editorError;
    private string? calculationError;
    private IReadOnlyList<VisibleAstronomyObject> visibleObjects = [];
    private CancellationTokenSource? calculationCancellation;

    private IReadOnlyList<VisibleAstronomyObject> FilteredVisibleObjects =>
        visibleObjects
            .Select(observation => new VisibleAstronomyObject(
                observation.Key,
                observation.Name,
                observation.ObjectType,
                observation.Windows
                    .Where(window => selectedDirections.Contains(window.Direction))
                    .ToList()))
            .Where(observation => selectedObjectTypes.Contains(observation.ObjectType))
            .Where(observation => observation.Windows.Count > 0)
            .ToList();

    protected override async Task OnParametersSetAsync()
    {
        var selectedId = SelectedObservationPoint?.Id;
        if (!hasEditorState || editorPointId != selectedId)
        {
            LoadEditor(SelectedObservationPoint);
            hasEditorState = true;
        }

        var selectedDate = SelectedLocalDateTime.Date;
        if (!hasRange || rangeDate != selectedDate)
        {
            rangeStartLocal = selectedDate;
            rangeEndLocal = selectedDate.AddDays(1);
            rangeDate = selectedDate;
            hasRange = true;
        }

        await CalculateVisibilityAsync();
    }

    private async Task SaveObservationPointAsync()
    {
        isSaving = true;
        editorError = null;

        try
        {
            var savedPoint = await ObservationPointService.SaveObservationPointAsync(editPoint);
            await ObservationPointSaved.InvokeAsync(savedPoint);
            LoadEditor(savedPoint);
            hasEditorState = true;
        }
        catch (Exception exception)
        {
            editorError = $"Unable to save observation point: {exception.Message}";
        }
        finally
        {
            isSaving = false;
        }
    }

    private async Task DeleteObservationPointAsync()
    {
        if (editPoint.Id == 0)
        {
            return;
        }

        isDeleting = true;
        editorError = null;
        var deletedId = editPoint.Id;

        try
        {
            await ObservationPointService.DeleteObservationPointAsync(deletedId);
            await ObservationPointDeleted.InvokeAsync(deletedId);
            await StartNewPoint();
        }
        catch (Exception exception)
        {
            editorError = $"Unable to delete observation point: {exception.Message}";
        }
        finally
        {
            isDeleting = false;
        }
    }

    private async Task OnEditorPointSelected()
    {
        await ObservationPointSelected.InvokeAsync(editorPointId);

        var selectedPoint = ObservationPoints.FirstOrDefault(point => point.Id == editorPointId);
        LoadEditor(selectedPoint);
        hasEditorState = true;
    }

    private async Task StartNewPoint()
    {
        editorPointId = null;
        await ObservationPointSelected.InvokeAsync(null);
        LoadEditor(null);
        hasEditorState = true;
        editorError = null;
    }

    private void LoadEditor(ObservationPoint? observationPoint)
    {
        editPoint = observationPoint is null
            ? CreateNewPoint(Location)
            : Clone(observationPoint);
        editLocation = editPoint.Coordinates;
        editorPointId = observationPoint?.Id;
    }

    private void OnEditLocationChanged(GeographicCoordinate coordinate)
    {
        editLocation = coordinate;
        editPoint.Latitude = coordinate.Latitude.Degrees;
        editPoint.Longitude = coordinate.Longitude.Degrees;
    }

    private Task OnRangeChanged() => CalculateVisibilityAsync();

    private async Task CalculateVisibilityAsync()
    {
        calculationCancellation?.Cancel();
        calculationCancellation?.Dispose();
        calculationCancellation = new CancellationTokenSource();
        var cancellationToken = calculationCancellation.Token;

        isCalculating = true;
        calculationError = null;

        try
        {
            var (fromUtc, toUtc) = ConvertRangeToUtc();
            var messierObjects = await AstronomyService.GetAllMessierObjects();
            await Task.Yield();

            var point = SelectedObservationPoint ?? CreateNewPoint(Location);
            visibleObjects = visibilityCalculator.Calculate(
                point,
                messierObjects,
                fromUtc,
                toUtc,
                cancellationToken: cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (Exception exception)
        {
            visibleObjects = [];
            calculationError = $"Unable to calculate visible objects: {exception.Message}";
        }
        finally
        {
            if (calculationCancellation?.Token == cancellationToken)
            {
                isCalculating = false;
            }
        }
    }

    private (DateTimeOffset FromUtc, DateTimeOffset ToUtc) ConvertRangeToUtc()
    {
        if (rangeEndLocal <= rangeStartLocal)
        {
            throw new ArgumentException("The end of the time range must be after the start.");
        }

        var fromUtc = TimeZoneInfo.ConvertTimeToUtc(
            DateTime.SpecifyKind(rangeStartLocal, DateTimeKind.Unspecified),
            AstronomyDefaults.LocalTimeZone);
        var toUtc = TimeZoneInfo.ConvertTimeToUtc(
            DateTime.SpecifyKind(rangeEndLocal, DateTimeKind.Unspecified),
            AstronomyDefaults.LocalTimeZone);

        return (new DateTimeOffset(fromUtc), new DateTimeOffset(toUtc));
    }

    private bool IsDirectionSelected(HorizonDirection direction) => selectedDirections.Contains(direction);

    private void SetDirectionSelection(HorizonDirection direction, ChangeEventArgs eventArgs)
    {
        if (eventArgs.Value is true)
        {
            selectedDirections.Add(direction);
        }
        else
        {
            selectedDirections.Remove(direction);
        }
    }

    private bool IsObjectTypeSelected(AstronomyObjectType objectType) => selectedObjectTypes.Contains(objectType);

    private void SetObjectTypeSelection(AstronomyObjectType objectType, ChangeEventArgs eventArgs)
    {
        if (eventArgs.Value is true)
        {
            selectedObjectTypes.Add(objectType);
        }
        else
        {
            selectedObjectTypes.Remove(objectType);
        }
    }

    private static ObservationPoint CreateNewPoint(GeographicCoordinate location) => new()
    {
        Name = string.Empty,
        Latitude = location.Latitude.Degrees,
        Longitude = location.Longitude.Degrees,
        ElevationMeters = location.ElevationMeters == 0 ? null : location.ElevationMeters
    };

    private static ObservationPoint Clone(ObservationPoint source) => new()
    {
        Id = source.Id,
        Name = source.Name,
        Latitude = source.Latitude,
        Longitude = source.Longitude,
        ElevationMeters = source.ElevationMeters,
        HorizonNorth = source.HorizonNorth,
        HorizonNorthEast = source.HorizonNorthEast,
        HorizonEast = source.HorizonEast,
        HorizonSouthEast = source.HorizonSouthEast,
        HorizonSouth = source.HorizonSouth,
        HorizonSouthWest = source.HorizonSouthWest,
        HorizonWest = source.HorizonWest,
        HorizonNorthWest = source.HorizonNorthWest
    };

    private static string FormatDirection(HorizonDirection direction) => direction switch
    {
        HorizonDirection.North => "N",
        HorizonDirection.NorthEast => "NE",
        HorizonDirection.East => "E",
        HorizonDirection.SouthEast => "SE",
        HorizonDirection.South => "S",
        HorizonDirection.SouthWest => "SW",
        HorizonDirection.West => "W",
        HorizonDirection.NorthWest => "NW",
        _ => direction.ToString()
    };

    private static string FormatObjectType(AstronomyObjectType objectType) => objectType switch
    {
        AstronomyObjectType.DeepSky => "Deep sky",
        _ => objectType.ToString()
    };

    private static string FormatLocalTime(DateTimeOffset utcTime) =>
        TimeZoneInfo.ConvertTime(utcTime, AstronomyDefaults.LocalTimeZone).ToString("g");

    private static string FormatDuration(TimeSpan duration)
    {
        if (duration.TotalMinutes < 1)
        {
            return "< 1m";
        }

        return $"{(int)duration.TotalHours}h {duration.Minutes:D2}m";
    }

    public void Dispose()
    {
        calculationCancellation?.Cancel();
        calculationCancellation?.Dispose();
    }
}
