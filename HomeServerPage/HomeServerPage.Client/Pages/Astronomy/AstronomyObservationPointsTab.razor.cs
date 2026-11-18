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
using HomeServerPage.Data.Locations;

namespace HomeServerPage.Client.Pages.Astronomy;

public partial class AstronomyObservationPointsTab
{
    private static readonly HorizonDirection[] Directions = Enum.GetValues<HorizonDirection>();
    private static readonly AstronomyObjectType[] ObjectTypes = Enum.GetValues<AstronomyObjectType>();
    private static readonly ObservationPointVisibilityCalculator VisibilityCalculator = new();

    [Parameter, EditorRequired]
    public SavedLocation Location { get; set; } = default!;

    [Parameter]
    public IReadOnlyList<SavedLocation> Locations { get; set; } = [];

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
    private int? editorPointId;
    private int? editorLocationId;
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
    private SavedLocation ActiveLocation => this.SelectedObservationPoint?.Location ?? this.editPoint.Location ?? this.Location;

    private IReadOnlyList<VisibleAstronomyObject> FilteredVisibleObjects =>
        this.visibleObjects
            .Select(observation => new VisibleAstronomyObject(
                observation.Key,
                observation.Name,
                observation.ObjectType,
                observation.Windows
                    .Where(window => this.selectedDirections.Contains(window.Direction))
                    .ToList()))
            .Where(observation => this.selectedObjectTypes.Contains(observation.ObjectType))
            .Where(observation => observation.Windows.Count > 0)
            .ToList();

    protected override async Task OnParametersSetAsync()
    {
        var selectedId = SelectedObservationPoint?.Id;
        if (!this.hasEditorState
            || this.editorPointId != selectedId
            || (selectedId is null && this.editorLocationId != this.Location.Id))
        {
            this.LoadEditor(this.SelectedObservationPoint);
            this.hasEditorState = true;
        }

        var selectedDate = this.SelectedLocalDateTime.Date;
        if (!this.hasRange || this.rangeDate != selectedDate)
        {
            this.rangeStartLocal = selectedDate;
            this.rangeEndLocal = selectedDate.AddDays(1);
            this.rangeDate = selectedDate;
            this.hasRange = true;
        }

        await this.CalculateVisibilityAsync();
    }

    private async Task SaveObservationPointAsync()
    {
        this.isSaving = true;
        this.editorError = null;

        try
        {
            this.editPoint.Location = this.FindLocation(this.editPoint.LocationId);
            var savedPoint = await this.ObservationPointService.SaveObservationPointAsync(this.editPoint);
            await this.ObservationPointSaved.InvokeAsync(savedPoint);
            this.LoadEditor(savedPoint);
            this.hasEditorState = true;
        }
        catch (Exception exception)
        {
            this.editorError = $"Unable to save observation point: {exception.Message}";
        }
        finally
        {
            this.isSaving = false;
        }
    }

    private async Task DeleteObservationPointAsync()
    {
        if (this.editPoint.Id == 0)
        {
            return;
        }

        this.isDeleting = true;
        this.editorError = null;
        var deletedId = this.editPoint.Id;

        try
        {
            await this.ObservationPointService.DeleteObservationPointAsync(deletedId);
            await this.ObservationPointDeleted.InvokeAsync(deletedId);
            await this.StartNewPoint();
        }
        catch (Exception exception)
        {
            this.editorError = $"Unable to delete observation point: {exception.Message}";
        }
        finally
        {
            this.isDeleting = false;
        }
    }

    private async Task OnEditorPointSelected()
    {
        await this.ObservationPointSelected.InvokeAsync(this.editorPointId);

        var selectedPoint = this.ObservationPoints.FirstOrDefault(point => point.Id == this.editorPointId);
        this.LoadEditor(selectedPoint);
        this.hasEditorState = true;
    }

    private async Task StartNewPoint()
    {
        this.editorPointId = null;
        await this.ObservationPointSelected.InvokeAsync(null);
        this.LoadEditor(null);
        this.hasEditorState = true;
        this.editorError = null;
    }

    private void LoadEditor(ObservationPoint? observationPoint)
    {
        this.editPoint = observationPoint is null
            ? CreateNewPoint(this.Location)
            : Clone(observationPoint);
        this.editorPointId = observationPoint?.Id;
        this.editorLocationId = this.editPoint.LocationId;
    }

    private Task OnEditLocationChanged()
    {
        this.editPoint.Location = this.FindLocation(this.editPoint.LocationId);
        return this.CalculateVisibilityAsync();
    }

    private Task OnRangeChanged() => CalculateVisibilityAsync();

    private async Task CalculateVisibilityAsync()
    {
        this.calculationCancellation?.Cancel();
        this.calculationCancellation?.Dispose();
        this.calculationCancellation = new CancellationTokenSource();
        var cancellationToken = this.calculationCancellation.Token;

        this.isCalculating = true;
        this.calculationError = null;

        try
        {
            var (fromUtc, toUtc) = this.ConvertRangeToUtc();
            var messierObjects = await this.AstronomyService.GetAllMessierObjects();
            await Task.Yield();

            var point = this.SelectedObservationPoint ?? this.editPoint;
            point.Location ??= this.FindLocation(point.LocationId);
            this.visibleObjects = this.visibilityCalculator.Calculate(
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
            this.visibleObjects = [];
            this.calculationError = $"Unable to calculate visible objects: {exception.Message}";
        }
        finally
        {
            if (this.calculationCancellation?.Token == cancellationToken)
            {
                this.isCalculating = false;
            }
        }
    }

    private (DateTimeOffset FromUtc, DateTimeOffset ToUtc) ConvertRangeToUtc()
    {
        if (this.rangeEndLocal <= this.rangeStartLocal)
        {
            throw new ArgumentException("The end of the time range must be after the start.");
        }

        var timeZone = TimeZoneInfo.FindSystemTimeZoneById(this.ActiveLocation.TimeZoneId);
        var fromUtc = TimeZoneInfo.ConvertTimeToUtc(
            DateTime.SpecifyKind(this.rangeStartLocal, DateTimeKind.Unspecified),
            timeZone);
        var toUtc = TimeZoneInfo.ConvertTimeToUtc(
            DateTime.SpecifyKind(this.rangeEndLocal, DateTimeKind.Unspecified),
            timeZone);

        return (new DateTimeOffset(fromUtc), new DateTimeOffset(toUtc));
    }

    private bool IsDirectionSelected(HorizonDirection direction) => this.selectedDirections.Contains(direction);

    private void SetDirectionSelection(HorizonDirection direction, ChangeEventArgs eventArgs)
    {
        if (eventArgs.Value is true)
        {
            this.selectedDirections.Add(direction);
        }
        else
        {
            this.selectedDirections.Remove(direction);
        }
    }

    private bool IsObjectTypeSelected(AstronomyObjectType objectType) => this.selectedObjectTypes.Contains(objectType);

    private void SetObjectTypeSelection(AstronomyObjectType objectType, ChangeEventArgs eventArgs)
    {
        if (eventArgs.Value is true)
        {
            this.selectedObjectTypes.Add(objectType);
        }
        else
        {
            this.selectedObjectTypes.Remove(objectType);
        }
    }

    private static ObservationPoint CreateNewPoint(SavedLocation location) => new()
    {
        Name = string.Empty,
        LocationId = location.Id,
        Location = location
    };

    private static ObservationPoint Clone(ObservationPoint source) => new()
    {
        Id = source.Id,
        Name = source.Name,
        LocationId = source.LocationId,
        Location = source.Location,
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

    private SavedLocation? FindLocation(int? locationId) =>
        locationId is int id
            ? this.Locations.FirstOrDefault(location => location.Id == id)
            : null;

    private string FormatLocalTime(DateTimeOffset utcTime) =>
        TimeZoneInfo.ConvertTime(
            utcTime,
            TimeZoneInfo.FindSystemTimeZoneById(this.ActiveLocation.TimeZoneId)).ToString("g");

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
        this.calculationCancellation?.Cancel();
        this.calculationCancellation?.Dispose();
    }
}
