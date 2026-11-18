using Bunit;
using HomeServerPage.Client.Data;
using HomeServerPage.Client.Data.Locations;
using HomeServerPage.Client.Pages.Weather;
using HomeServerPage.Data;
using HomeServerPage.Data.Locations;
using HomeServerPage.Data.Weather;
using HomeServerPage.Tests.Unit.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using WeatherForecastModel = HomeServerPage.Data.Weather.WeatherForecast;

namespace HomeServerPage.Tests.Unit.Components;

public sealed class WeatherComponentTests
{
    [Test]
    public void ChangingLocationUpdatesSelectionAndRenderedWeather()
    {
        var firstLocation = CreateLocation(1, "Szczecin");
        var secondLocation = CreateLocation(2, "Gdansk");
        var locations = new[] { firstLocation, secondLocation };
        var locationService = new Mock<ILocationService>();
        locationService
            .Setup(service => service.GetLocationsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(locations);
        var selectionService = new Mock<ILocationSelectionService>();
        selectionService
            .Setup(service => service.GetSelectedLocationAsync(
                LocationFeature.Weather,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(firstLocation);
        selectionService
            .Setup(service => service.SetSelectedLocationAsync(
                LocationFeature.Weather,
                secondLocation.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(secondLocation);
        var weatherService = new Mock<IWeatherService>();
        weatherService
            .Setup(service => service.GetForecastAsync(
                It.IsAny<SavedLocation>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .Returns((SavedLocation location, int _, CancellationToken _) =>
                Task.FromResult(CreateForecast(location)));
        using var context = new BunitContext();
        context.Services.AddSingleton(locationService.Object);
        context.Services.AddSingleton(selectionService.Object);
        context.Services.AddSingleton(weatherService.Object);
        context.Services.AddSingleton(new TabletDashboardRefreshService(
            Mock.Of<IDateTimeService>(),
            Mock.Of<ITabletDashboardRefreshSettingsProvider>(),
            new TestRealtimeNotificationEventSource(),
            NullLogger<TabletDashboardRefreshService>.Instance));

        var component = context.Render<Weather>();
        component.Find("#weather-location-select").Change(secondLocation.Id.ToString());

        component.WaitForAssertion(() =>
        {
            Assert.That(component.Markup, Does.Contain("Current conditions in Gdansk."));
            Assert.That(component.Markup, Does.Contain("12"));
        });
        selectionService.Verify(
            service => service.SetSelectedLocationAsync(
                LocationFeature.Weather,
                secondLocation.Id,
                It.IsAny<CancellationToken>()),
            Times.Once);
        weatherService.Verify(
            service => service.GetForecastAsync(
                secondLocation,
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()),
            Times.AtLeastOnce);
    }

    private static SavedLocation CreateLocation(int id, string name) => new()
    {
        Id = id,
        Name = name,
        Latitude = 53.4,
        Longitude = 14.5,
        TimeZoneId = "Europe/Warsaw"
    };

    private static WeatherForecastModel CreateForecast(SavedLocation location) =>
        new(
            new WeatherSnapshot(
                new WeatherLocation(
                    location.Name,
                    "Region",
                    "Poland",
                    (decimal)location.Latitude,
                    (decimal)location.Longitude,
                    location.TimeZoneId,
                    new DateTimeOffset(2026, 11, 18, 16, 0, 0, TimeSpan.Zero)),
                new DateTimeOffset(2026, 11, 18, 15, 0, 0, TimeSpan.Zero),
                12,
                11,
                70,
                5,
                "NW",
                8,
                1012,
                0,
                20,
                10,
                1,
                new WeatherCondition("Partly cloudy", "https://weather.test/icon.png", 1003),
                null),
            []);
}
