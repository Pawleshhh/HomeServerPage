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

namespace HomeServerPage.Client.Pages.Astronomy;

public partial class AstronomyObservationsTab
{
    [Parameter, EditorRequired]
    public GeographicCoordinate Location { get; set; }

    [Parameter, EditorRequired]
    public DateTime RequestDateUtc { get; set; }

    private IReadOnlyList<AstronomyObservation> solarSystemObservations = [];
    private IReadOnlyList<AstronomyObservation> messierObservations = [];
    private WeatherAstronomy? moonAstronomy;
    private DateTimeOffset? observationTime;
    private bool isLoading = true;
    private string? loadError;
    private MessierSort messierSort = MessierSort.Name;
    private GeographicCoordinate? loadedLocation;
    private DateTime loadedRequestDateUtc;

    private IEnumerable<AstronomyObservation> SortedMessierObservations => messierSort switch
    {
        MessierSort.RiseTime => messierObservations.OrderBy(observation => observation.TransitResult.RiseUT),
        MessierSort.SetTime => messierObservations.OrderBy(observation => observation.TransitResult.SetUT),
        _ => messierObservations.OrderBy(observation => observation.Name)
    };

    protected override async Task OnParametersSetAsync()
    {
        if (loadedLocation is not null && loadedLocation.Equals(Location) && loadedRequestDateUtc == RequestDateUtc)
        {
            return;
        }

        loadedLocation = Location;
        loadedRequestDateUtc = RequestDateUtc;
        observationTime = TimeZoneInfo.ConvertTime(
            new DateTimeOffset(RequestDateUtc, TimeSpan.Zero),
            AstronomyDefaults.LocalTimeZone);

        await LoadObservationsAsync();
    }

    private async Task LoadObservationsAsync()
    {
        isLoading = true;
        loadError = null;

        try
        {
            solarSystemObservations = await AstronomyObservationLoader.LoadSolarSystemAsync(
                AstronomyService,
                RequestDateUtc,
                Location);

            moonAstronomy = await WeatherService.GetAstronomyAsync(
                DateOnly.FromDateTime(RequestDateUtc));

            messierObservations = await AstronomyObservationLoader.LoadMessierCatalogAsync(
                AstronomyService,
                RequestDateUtc,
                Location);
        }
        catch (Exception exception)
        {
            loadError = $"Unable to load celestial observations: {exception.Message}";
        }
        finally
        {
            isLoading = false;
        }
    }

    private enum MessierSort
    {
        Name,
        RiseTime,
        SetTime
    }
}
