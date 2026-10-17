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

public partial class AstronomyEquipmentTab
{
    private IReadOnlyList<TelescopeItem> telescopes = [];
    private IReadOnlyList<TelescopeEyepiece> eyepieces = [];
    private IReadOnlyList<TelescopeLens> lenses = [];
    private IReadOnlyList<SensorItem> sensors = [];
    private string? loadError;
    private bool isLoading = true;

    protected override async Task OnInitializedAsync()
    {
        loadError = null;
        isLoading = true;

        try
        {
            var telescopesTask = TelescopeService.GetTelescopesAsync();
            var eyepiecesTask = TelescopeService.GetEyepiecesAsync();
            var lensesTask = TelescopeService.GetLensesAsync();
            var sensorsTask = TelescopeService.GetSensorsAsync();

            await Task.WhenAll(telescopesTask, eyepiecesTask, lensesTask, sensorsTask);

            telescopes = telescopesTask.Result.ToList();
            eyepieces = eyepiecesTask.Result.ToList();
            lenses = lensesTask.Result.ToList();
            sensors = sensorsTask.Result.ToList();
        }
        catch (Exception exception)
        {
            loadError = $"Unable to load equipment: {exception.Message}";
        }
        finally
        {
            isLoading = false;
        }
    }

    private static string FormatEnum<TEnum>(TEnum value) where TEnum : Enum
    {
        var text = value.ToString();
        var sb = new System.Text.StringBuilder();

        for (var i = 0; i < text.Length; i++)
        {
            var c = text[i];
            if (i > 0 && char.IsUpper(c) && !char.IsUpper(text[i - 1]))
            {
                sb.Append(' ');
            }

            sb.Append(c);
        }

        return sb.ToString();
    }
}
