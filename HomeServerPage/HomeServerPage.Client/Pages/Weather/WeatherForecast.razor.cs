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
using WeatherForecastModel = HomeServerPage.Data.Weather.WeatherForecast;

namespace HomeServerPage.Client.Pages.Weather;

public partial class WeatherForecast
{
    private WeatherForecastModel? forecast;
    private bool isLoading = true;
    private string? loadError;

    private string LocationLabel => forecast?.Current.Location.Name ?? "Szczecin";

    protected override async Task OnInitializedAsync()
    {
        try
        {
            forecast = await WeatherService.GetForecastAsync();
        }
        catch (Exception)
        {
            loadError = "Could not load the weather forecast.";
        }
        finally
        {
            isLoading = false;
        }
    }

    private static string GetIconClass(WeatherCondition condition) =>
        WeatherConditionIcons.GetIconClass(condition.Text);
}
