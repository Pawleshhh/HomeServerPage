using Bunit;
using HomeServerPage.Client.Data;
using HomeServerPage.Client.Data.Locations;
using HomeServerPage.Client.Pages.Tablet;
using HomeServerPage.Data;
using HomeServerPage.Data.Astronomy;
using HomeServerPage.Data.Calories;
using HomeServerPage.Data.Exercise;
using HomeServerPage.Data.Fridge;
using HomeServerPage.Data.Locations;
using HomeServerPage.Data.Notifications;
using HomeServerPage.Data.PublicTransport;
using HomeServerPage.Data.Weather;
using HomeServerPage.Tests.Unit.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.JSInterop;
using Moq;

namespace HomeServerPage.Tests.Unit.Components;

public sealed class TabletDashboardComponentTests
{
    [Test]
    public async Task RendersWidgetsAndRefreshesFridgeAfterConnectionRestoration()
    {
        var location = new SavedLocation
        {
            Id = 1,
            Name = "Szczecin",
            Latitude = 53.4,
            Longitude = 14.5,
            TimeZoneId = "Etc/UTC"
        };
        var dateTimeService = new Mock<IDateTimeService>();
        dateTimeService
            .Setup(service => service.SyncAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        dateTimeService
            .SetupGet(service => service.UtcNow)
            .Returns(new DateTime(2026, 11, 18, 15, 0, 0, DateTimeKind.Utc));
        var settingsProvider = new Mock<ITabletDashboardRefreshSettingsProvider>();
        settingsProvider
            .Setup(provider => provider.GetSettingsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TabletDashboardRefreshOptions());
        var notificationEventSource = new TestRealtimeNotificationEventSource();
        var fridgeService = new Mock<IFridgeService>();
        fridgeService
            .Setup(service => service.GetItemsAsync())
            .ReturnsAsync([]);
        var caloriesService = new Mock<ICaloriesService>();
        caloriesService
            .Setup(service => service.GetDailyGoalAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((int?)null);
        caloriesService
            .Setup(service => service.GetEntriesAsync(
                It.IsAny<DateOnly>(),
                It.IsAny<DateOnly>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        var exerciseService = new Mock<IExerciseService>();
        exerciseService
            .Setup(service => service.GetEntriesAsync(
                It.IsAny<DateOnly>(),
                It.IsAny<DateOnly>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        var transportService = new Mock<IPublicTransportService>();
        transportService
            .Setup(service => service.GetDepartureBoardsAsync(
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        var selectionService = new Mock<ILocationSelectionService>();
        selectionService
            .Setup(service => service.GetSelectedLocationAsync(
                It.IsAny<LocationFeature>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(location);
        var weatherService = new Mock<IWeatherService>();
        weatherService
            .Setup(service => service.GetForecastAsync(
                It.IsAny<SavedLocation>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateForecast(location));
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
        var astronomyService = new Mock<IAstronomyService>();
        astronomyService
            .Setup(service => service.GetSunRiseAndSetTime(
                It.IsAny<DateTime>(),
                It.IsAny<AstroCalc.Core.GeographicCoordinate>()))
            .ThrowsAsync(new InvalidOperationException("not needed for dashboard layout test"));
        astronomyService
            .Setup(service => service.GetMoonRiseAndSetTime(
                It.IsAny<DateTime>(),
                It.IsAny<AstroCalc.Core.GeographicCoordinate>()))
            .ThrowsAsync(new InvalidOperationException("not needed for dashboard layout test"));
        astronomyService
            .Setup(service => service.GetPlanetRiseAndSetTime(
                It.IsAny<DateTime>(),
                It.IsAny<AstroCalc.Core.GeographicCoordinate>(),
                It.IsAny<AstroCalc.SolarSystem.Planet>()))
            .ThrowsAsync(new InvalidOperationException("not needed for dashboard layout test"));

        await using var context = new BunitContext();
        context.Services.AddSingleton<IDateTimeService>(dateTimeService.Object);
        context.Services.AddSingleton<ITabletDashboardRefreshSettingsProvider>(settingsProvider.Object);
        context.Services.AddSingleton<IRealtimeNotificationEventSource>(notificationEventSource);
        context.Services.AddSingleton<IFridgeService>(fridgeService.Object);
        context.Services.AddSingleton<ICaloriesService>(caloriesService.Object);
        context.Services.AddSingleton<IExerciseService>(exerciseService.Object);
        context.Services.AddSingleton<IPublicTransportService>(transportService.Object);
        context.Services.AddSingleton<ILocationSelectionService>(selectionService.Object);
        context.Services.AddSingleton<IWeatherService>(weatherService.Object);
        context.Services.AddSingleton<IAstronomyService>(astronomyService.Object);
        context.Services.AddSingleton<IJSRuntime>(
            new FixedBrowserJsRuntime(new DateOnly(2026, 11, 18), new TimeOnly(16, 0)));
        context.Services.AddSingleton(serviceProvider =>
            new TabletDashboardRefreshService(
                serviceProvider.GetRequiredService<IDateTimeService>(),
                serviceProvider.GetRequiredService<ITabletDashboardRefreshSettingsProvider>(),
                serviceProvider.GetRequiredService<IRealtimeNotificationEventSource>(),
                NullLogger<TabletDashboardRefreshService>.Instance));

        var component = context.Render<TabletDashboard>();
        component.WaitForAssertion(() =>
        {
            Assert.That(component.Find("[aria-label='Daily essentials']"), Is.Not.Null);
            Assert.That(component.FindAll(".tablet-dashboard-item"), Has.Count.EqualTo(4));
        });
        Assert.That(notificationEventSource.IsStarted, Is.True);

        var initialLoadCount = fridgeService.Invocations.Count(
            invocation => invocation.Method.Name == nameof(IFridgeService.GetItemsAsync));
        await notificationEventSource.PublishAsync(RealtimeNotificationTypes.ConnectionRestored);
        var refreshedLoadCount = fridgeService.Invocations.Count(
            invocation => invocation.Method.Name == nameof(IFridgeService.GetItemsAsync));

        Assert.That(refreshedLoadCount, Is.GreaterThan(initialLoadCount));
        await context.DisposeComponentsAsync();
        Assert.That(notificationEventSource.IsStopped, Is.True);
    }

    private static WeatherForecast CreateForecast(SavedLocation location) =>
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
