using AstroCalc.Catalogs;
using AstroCalc.Coordinates;
using AstroCalc.Core;
using AstroCalc.Observation;
using AstroCalc.SolarSystem;
using Bunit;
using HomeServerPage.Client.Data.Locations;
using HomeServerPage.Client.Pages.Astronomy;
using HomeServerPage.Data;
using HomeServerPage.Data.Astronomy;
using HomeServerPage.Data.Locations;
using HomeServerPage.Data.Weather;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using AstronomyPage = HomeServerPage.Client.Pages.Astronomy.Astronomy;

namespace HomeServerPage.Tests.Unit.Components;

public sealed class AstronomyComponentTests
{
    [Test]
    public void RendersObservationTabForSelectedLocationAndRequestsItsSkyData()
    {
        var location = new SavedLocation
        {
            Id = 3,
            Name = "Dark site",
            Latitude = 53.4,
            Longitude = 14.5,
            ElevationMeters = 20,
            TimeZoneId = "Etc/UTC"
        };
        var locationService = new Mock<ILocationService>();
        locationService
            .Setup(service => service.GetLocationsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { location });
        var selectionService = new Mock<ILocationSelectionService>();
        selectionService
            .Setup(service => service.GetSelectedLocationAsync(
                LocationFeature.Astronomy,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(location);
        var observationPointService = new Mock<IObservationPointService>();
        observationPointService
            .Setup(service => service.GetObservationPointsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<ObservationPoint>());
        var dateTimeService = new Mock<IDateTimeService>();
        dateTimeService
            .Setup(service => service.SyncAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        dateTimeService
            .SetupGet(service => service.UtcNow)
            .Returns(new DateTime(2026, 11, 18, 15, 0, 0, DateTimeKind.Utc));
        var realAstronomyService = new AstronomyService();
        var astronomyService = new Mock<IAstronomyService>();
        astronomyService
            .Setup(service => service.GetAllMessierObjects())
            .ReturnsAsync(Array.Empty<MessierObject>());
        astronomyService
            .Setup(service => service.GetSunRiseAndSetTime(
                It.IsAny<DateTime>(),
                It.IsAny<GeographicCoordinate>()))
            .Returns((DateTime date, GeographicCoordinate coordinates) =>
                realAstronomyService.GetSunRiseAndSetTime(date, coordinates));
        astronomyService
            .Setup(service => service.GetMoonRiseAndSetTime(
                It.IsAny<DateTime>(),
                It.IsAny<GeographicCoordinate>()))
            .Returns((DateTime date, GeographicCoordinate coordinates) =>
                realAstronomyService.GetMoonRiseAndSetTime(date, coordinates));
        astronomyService
            .Setup(service => service.GetPlanetRiseAndSetTime(
                It.IsAny<DateTime>(),
                It.IsAny<GeographicCoordinate>(),
                It.IsAny<Planet>()))
            .Returns((DateTime date, GeographicCoordinate coordinates, Planet planet) =>
                realAstronomyService.GetPlanetRiseAndSetTime(date, coordinates, planet));
        var weatherService = new Mock<IWeatherService>();
        weatherService
            .Setup(service => service.GetAstronomyAsync(
                It.IsAny<SavedLocation>(),
                It.IsAny<DateOnly>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new WeatherAstronomy(
                "6:00 AM",
                "8:00 PM",
                "7:00 PM",
                "5:00 AM",
                "Waxing Crescent",
                25));
        using var context = new BunitContext();
        context.Services.AddSingleton(locationService.Object);
        context.Services.AddSingleton(selectionService.Object);
        context.Services.AddSingleton(observationPointService.Object);
        context.Services.AddSingleton(dateTimeService.Object);
        context.Services.AddSingleton(astronomyService.Object);
        context.Services.AddSingleton(weatherService.Object);

        var component = context.Render<AstronomyPage>();

        component.WaitForAssertion(() =>
        {
            Assert.That(component.Markup, Does.Contain("Astronomy location"));
            Assert.That(component.Markup, Does.Contain("Dark site"));
            Assert.That(component.Markup, Does.Contain("Celestial bodies"));
            Assert.That(component.Find(".moon-phase").GetAttribute("aria-label"),
                Is.EqualTo("Waxing Crescent, 25% illuminated"));
        });
        astronomyService.Verify(
            service => service.GetSunRiseAndSetTime(
                It.IsAny<DateTime>(),
                It.IsAny<GeographicCoordinate>()),
            Times.Once);
        weatherService.Verify(
            service => service.GetAstronomyAsync(
                location,
                It.IsAny<DateOnly>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
