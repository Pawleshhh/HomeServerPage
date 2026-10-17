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
using HomeServerPage.Client.Data.Astronomy.Telescopes;

namespace HomeServerPage.Client.Pages.Astronomy;

public partial class AstronomyToolsTab
{
    private enum ResultViewMode
    {
        Both,
        Visual,
        Photography
    }

    private static readonly TelescopeCalculator calculator = new();

    private IReadOnlyList<TelescopeItem> telescopes = [];
    private IReadOnlyList<TelescopeEyepiece> eyepieces = [];
    private IReadOnlyList<TelescopeLens> lenses = [];
    private IReadOnlyList<SensorItem> sensors = [];
    private IReadOnlyList<DeepSkyObject> deepSkyObjects = [];
    private string? setupLoadError;
    private int? selectedTelescopeId;
    private int? selectedLensId;
    private int? selectedEyepieceId;
    private int? selectedSensorId;
    private ResultViewMode resultViewMode = ResultViewMode.Both;

    private List<DeepSkyObject> visualObjects = [];
    private List<DeepSkyObject> visualPartialObjects = [];
    private List<DeepSkyObject> photoObjects = [];
    private List<DeepSkyObject> photoPartialObjects = [];
    private const double VisibleSizeThreshold = 0.9;
    private double? visualFieldOfViewArcmin;
    private double? photoFieldOfViewWidthArcmin;
    private double? photoFieldOfViewHeightArcmin;

    private TelescopeItem? selectedTelescope => telescopes.FirstOrDefault(t => t.Id == selectedTelescopeId);
    private TelescopeLens? selectedLens => lenses.FirstOrDefault(l => l.Id == selectedLensId);
    private TelescopeEyepiece? selectedEyepiece => eyepieces.FirstOrDefault(e => e.Id == selectedEyepieceId);
    private SensorItem? selectedSensor => sensors.FirstOrDefault(s => s.Id == selectedSensorId);

    protected override async Task OnInitializedAsync()
    {
        await LoadSetupItemsAsync();
        RecalculateResults();
    }

    private async Task LoadSetupItemsAsync()
    {
        setupLoadError = null;

        try
        {
            var telescopesTask = TelescopeService.GetTelescopesAsync();
            var eyepiecesTask = TelescopeService.GetEyepiecesAsync();
            var lensesTask = TelescopeService.GetLensesAsync();
            var sensorsTask = TelescopeService.GetSensorsAsync();
            var deepSkyObjectsTask = TelescopeService.GetDeepSkyObjectsAsync();

            await Task.WhenAll(telescopesTask, eyepiecesTask, lensesTask, sensorsTask, deepSkyObjectsTask);

            telescopes = telescopesTask.Result.ToList();
            eyepieces = eyepiecesTask.Result.ToList();
            lenses = lensesTask.Result.ToList();
            sensors = sensorsTask.Result.ToList();
            deepSkyObjects = deepSkyObjectsTask.Result.ToList();
        }
        catch (Exception exception)
        {
            setupLoadError = $"Unable to load telescope equipment: {exception.Message}";
        }
    }

    private void RecalculateResults()
    {
        visualObjects = [];
        visualPartialObjects = [];
        photoObjects = [];
        photoPartialObjects = [];
        visualFieldOfViewArcmin = null;
        photoFieldOfViewWidthArcmin = null;
        photoFieldOfViewHeightArcmin = null;

        var telescope = selectedTelescope;
        if (telescope is null)
        {
            return;
        }

        var lens = selectedLens;
        var eyepiece = selectedEyepiece;
        var sensor = selectedSensor;

        if (eyepiece is not null)
        {
            var trueFieldOfViewDegrees = calculator.CalculateTFov(telescope, eyepiece, lens);
            visualFieldOfViewArcmin = trueFieldOfViewDegrees * 60;

            var visualObjectsWithThreshold = deepSkyObjects
                .Where(dso => Math.Max(dso.Width, dso.Height) <= visualFieldOfViewArcmin / VisibleSizeThreshold)
                .ToList();

            visualObjects = visualObjectsWithThreshold
                .Where(dso => Math.Max(dso.Width, dso.Height) <= visualFieldOfViewArcmin)
                .ToList();
            visualPartialObjects = visualObjectsWithThreshold
                .Except(visualObjects)
                .ToList();
        }

        if (sensor is not null)
        {
            var fov = calculator.CalculateAstrofotoFov(telescope, sensor, lens);
            photoFieldOfViewWidthArcmin = fov.Width * 60;
            photoFieldOfViewHeightArcmin = fov.Height * 60;

            var photoObjectsWithThreshold = deepSkyObjects
                .Where(dso => dso.Width <= photoFieldOfViewWidthArcmin / VisibleSizeThreshold
                    && dso.Height <= photoFieldOfViewHeightArcmin / VisibleSizeThreshold)
                .ToList();

            photoObjects = photoObjectsWithThreshold
                .Where(dso => dso.Width <= photoFieldOfViewWidthArcmin && dso.Height <= photoFieldOfViewHeightArcmin)
                .ToList();
            photoPartialObjects = photoObjectsWithThreshold
                .Except(photoObjects)
                .ToList();
        }
    }

    private string GetVisualPartialObjectStyle(DeepSkyObject deepSkyObject)
    {
        var visibleRatio = visualFieldOfViewArcmin!.Value / Math.Max(deepSkyObject.Width, deepSkyObject.Height);
        return GetPartialObjectStyle(visibleRatio);
    }

    private string GetPhotoPartialObjectStyle(DeepSkyObject deepSkyObject)
    {
        var widthVisibleRatio = photoFieldOfViewWidthArcmin!.Value / deepSkyObject.Width;
        var heightVisibleRatio = photoFieldOfViewHeightArcmin!.Value / deepSkyObject.Height;
        return GetPartialObjectStyle(Math.Min(widthVisibleRatio, heightVisibleRatio));
    }

    private static string GetPartialObjectStyle(double visibleRatio)
    {
        var progress = Math.Clamp(
            (visibleRatio - VisibleSizeThreshold) / (1 - VisibleSizeThreshold),
            0,
            1);
        var red = (int)Math.Round(220 + 35 * progress);
        var green = (int)Math.Round(53 + 140 * progress);
        var blue = (int)Math.Round(69 - 62 * progress);

        return $"background-color: rgba({red}, {green}, {blue}, 0.2);";
    }
}
