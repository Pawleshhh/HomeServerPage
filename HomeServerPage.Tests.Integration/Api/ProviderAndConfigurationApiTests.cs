using System.Net;
using System.Net.Http.Json;
using HomeServerPage.Client.Data.Astronomy.Telescopes;
using HomeServerPage.Client.Data;
using HomeServerPage.Data;
using HomeServerPage.Data.Astronomy;
using HomeServerPage.Data.Locations;
using HomeServerPage.Data.Notifications;
using HomeServerPage.Data.PublicTransport;
using HomeServerPage.Data.Weather;
using HomeServerPage.Tests.Integration.Infrastructure;
using Moq;

namespace HomeServerPage.Tests.Integration.Api;

public sealed class ProviderAndConfigurationApiTests
{
    [Test]
    public async Task WeatherEndpointsUseSavedLocationAndMapProviderErrors()
    {
        using var factory = new HomeServerPageWebApplicationFactory();
        using var client = factory.CreateClient();
        var locations = await client.GetFromJsonAsync<List<SavedLocation>>("/api/locations");
        var location = locations!.Single(item => item.Name == "Szczecin");

        var currentResponse = await client.GetAsync($"/api/weather/current?locationId={location.Id}");
        var current = await currentResponse.Content.ReadFromJsonAsync<WeatherSnapshot>();
        var invalidDaysResponse = await client.GetAsync(
            $"/api/weather/forecast?locationId={location.Id}&days=15");
        var missingLocationResponse = await client.GetAsync("/api/weather/current?locationId=9999");

        factory.WeatherService
            .Setup(service => service.GetForecastAsync(
                It.IsAny<SavedLocation>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new WeatherApiException(503, "provider unavailable"));
        var providerErrorResponse = await client.GetAsync(
            $"/api/weather/forecast?locationId={location.Id}&days=2");

        Assert.That(currentResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(current!.Location.Name, Is.EqualTo("Szczecin"));
        Assert.That(current.TemperatureCelsius, Is.EqualTo(12));
        Assert.That(invalidDaysResponse.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        Assert.That(missingLocationResponse.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        Assert.That(providerErrorResponse.StatusCode, Is.EqualTo(HttpStatusCode.ServiceUnavailable));
        Assert.That(factory.WeatherService.Invocations.Any(invocation =>
            invocation.Method.Name == nameof(IWeatherService.GetCurrentAsync)), Is.True);
    }

    [Test]
    public async Task PublicTransportAndAstronomyEndpointsReturnMeaningfulResults()
    {
        using var factory = new HomeServerPageWebApplicationFactory();
        using var client = factory.CreateClient();

        var boardsResponse = await client.GetAsync("/api/publictransport/departure-boards?limit=2");
        var boards = await boardsResponse.Content.ReadFromJsonAsync<List<DepartureBoard>>();
        var telescopeResponse = await client.GetAsync("/api/telescope");
        var telescopes = await telescopeResponse.Content.ReadFromJsonAsync<List<TelescopeItem>>();
        var invalidPlanetResponse = await client.GetAsync(
            "/api/astronomy/solarsystem/999?dateTime=2026-11-18T12:00:00&latitude=53.4&longitude=14.5&elevationMeters=0");
        var sunResponse = await client.GetAsync(
            "/api/astronomy/solarsystem/sun?dateTime=2026-11-18T12:00:00&latitude=53.4&longitude=14.5&elevationMeters=0");

        Assert.That(boardsResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(boards, Has.Count.EqualTo(2));
        Assert.That(boards![0].Stop.Number, Is.EqualTo("12811"));
        Assert.That(telescopeResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(telescopes, Has.Count.EqualTo(3));
        Assert.That(telescopes![0].FocalLength, Is.GreaterThan(0));
        Assert.That(invalidPlanetResponse.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        Assert.That(sunResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        factory.PublicTransportService.Verify(
            service => service.GetDepartureBoardsAsync(2, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task DateTimeAndDashboardSettingsExposeConfiguredValues()
    {
        using var factory = new HomeServerPageWebApplicationFactory();
        using var client = factory.CreateClient();

        var locations = await client.GetFromJsonAsync<List<SavedLocation>>("/api/locations");
        var defaultLocation = locations!.Single(location => location.IsSystemDefault);
        var sync = await client.GetFromJsonAsync<DateTimeSyncResult>("/api/datetime/sync");
        var settings = await client.GetFromJsonAsync<TabletDashboardRefreshOptions>(
            "/api/tablet-dashboard-refresh-settings");
        var astronomy = await client.GetAsync(
            $"/api/weather/astronomy?locationId={defaultLocation.Id}&date=2026-11-18");

        Assert.That(sync!.ServerUtcNow, Is.EqualTo(factory.Clock.UtcNow));
        Assert.That(settings!.ActiveFrom, Is.EqualTo("07:00"));
        Assert.That(settings.ActiveUntil, Is.EqualTo("23:00"));
        Assert.That(settings.Widgets.PublicTransportIntervalMinutes, Is.EqualTo(20));
        Assert.That(astronomy.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        factory.WeatherService.Verify(
            service => service.GetAstronomyAsync(
                It.IsAny<SavedLocation>(),
                new DateOnly(2026, 11, 18),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
