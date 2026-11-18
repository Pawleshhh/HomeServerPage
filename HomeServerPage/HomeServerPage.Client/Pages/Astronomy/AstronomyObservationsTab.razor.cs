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
using HomeServerPage.Data.Weather;
using HomeServerPage.Data.Locations;

namespace HomeServerPage.Client.Pages.Astronomy;

public partial class AstronomyObservationsTab
{
    [Parameter, EditorRequired]
    public SavedLocation Location { get; set; } = default!;

    [Parameter, EditorRequired]
    public DateTime RequestDateUtc { get; set; }

    private IReadOnlyList<AstronomyObservation> solarSystemObservations = [];
    private IReadOnlyList<AstronomyObservation> messierObservations = [];
    private WeatherAstronomy? moonAstronomy;
    private DateTimeOffset? observationTime;
    private bool isLoading = true;
    private string? loadError;
    private MessierSort messierSort = MessierSort.Name;
    private int? loadedLocationId;
    private DateTime loadedRequestDateUtc;
    private TimeZoneInfo LocationTimeZone =>
        TimeZoneInfo.FindSystemTimeZoneById(this.Location.TimeZoneId);

    private IEnumerable<AstronomyObservation> SortedMessierObservations => this.messierSort switch
    {
        MessierSort.RiseTime => this.messierObservations.OrderBy(observation => observation.TransitResult.RiseUT),
        MessierSort.SetTime => this.messierObservations.OrderBy(observation => observation.TransitResult.SetUT),
        _ => this.messierObservations.OrderBy(observation => observation.Name)
    };

    protected override async Task OnParametersSetAsync()
    {
        if (this.loadedLocationId == this.Location.Id
            && this.loadedRequestDateUtc == this.RequestDateUtc)
        {
            return;
        }

        this.loadedLocationId = this.Location.Id;
        this.loadedRequestDateUtc = this.RequestDateUtc;
        this.observationTime = TimeZoneInfo.ConvertTime(
            new DateTimeOffset(this.RequestDateUtc, TimeSpan.Zero),
            this.LocationTimeZone);

        await this.LoadObservationsAsync();
    }

    private async Task LoadObservationsAsync()
    {
        this.isLoading = true;
        this.loadError = null;

        try
        {
            this.solarSystemObservations = await AstronomyObservationLoader.LoadSolarSystemAsync(
                this.AstronomyService,
                this.RequestDateUtc,
                this.Location.Coordinates);

            var localRequestDate = TimeZoneInfo.ConvertTime(
                new DateTimeOffset(this.RequestDateUtc, TimeSpan.Zero),
                this.LocationTimeZone);
            this.moonAstronomy = await this.WeatherService.GetAstronomyAsync(
                this.Location,
                DateOnly.FromDateTime(localRequestDate.DateTime));

            this.messierObservations = await AstronomyObservationLoader.LoadMessierCatalogAsync(
                this.AstronomyService,
                this.RequestDateUtc,
                this.Location.Coordinates);
        }
        catch (Exception exception)
        {
            this.loadError = $"Unable to load celestial observations: {exception.Message}";
        }
        finally
        {
            this.isLoading = false;
        }
    }

    private enum MessierSort
    {
        Name,
        RiseTime,
        SetTime
    }
}
