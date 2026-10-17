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

namespace HomeServerPage.Client.Pages.Weather;

public partial class WeatherWidget
{
    private WeatherSnapshot? snapshot;
    private bool isLoading = true;
    private string? loadError;
    private WeatherForecastDay? forecastDay;

    [Parameter]
    public bool AutoRefresh { get; set; }

    private string LocationLabel => this.snapshot?.Location.Name ?? "Szczecin";
    private string WeatherIconClass => WeatherConditionIcons.GetIconClass(this.snapshot?.Condition.Text);
    private int RainChancePercent => this.forecastDay?.ChanceOfRainPercent ?? 0;

    protected override async Task OnInitializedAsync()
    {
        if (AutoRefresh)
        {
            DashboardRefreshService.RefreshRequested += this.HandleRefreshRequestedAsync;
        }

        await this.LoadWeatherAsync();
    }

    private async Task LoadWeatherAsync()
    {
        this.isLoading = true;
        this.loadError = null;

        try
        {
            var forecast = await WeatherService.GetForecastAsync(1);
            this.snapshot = forecast.Current;
            this.forecastDay = forecast.Days.FirstOrDefault();
        }
        catch (Exception)
        {
            this.loadError = "Could not load weather.";
        }
        finally
        {
            this.isLoading = false;
        }
    }

    private Task HandleRefreshRequestedAsync() => InvokeAsync(async () =>
    {
        await this.LoadWeatherAsync();
        this.StateHasChanged();
    });

    public void Dispose()
    {
        if (AutoRefresh)
        {
            DashboardRefreshService.RefreshRequested -= this.HandleRefreshRequestedAsync;
        }
    }
}
