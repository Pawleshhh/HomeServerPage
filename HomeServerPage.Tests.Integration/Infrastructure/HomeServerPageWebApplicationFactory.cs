using HomeServerPage.Client.Data.Locations;
using HomeServerPage.Data;
using HomeServerPage.Data.Astronomy;
using HomeServerPage.Data.Calories;
using HomeServerPage.Data.Exercise;
using HomeServerPage.Data.Fridge;
using HomeServerPage.Data.Locations;
using HomeServerPage.Data.Notifications;
using HomeServerPage.Data.PublicTransport;
using HomeServerPage.Data.Weather;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;

namespace HomeServerPage.Tests.Integration.Infrastructure;

internal sealed class HomeServerPageWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly bool deleteDatabaseDirectoryOnDispose;

    public HomeServerPageWebApplicationFactory(
        string? databaseDirectory = null,
        bool deleteDatabaseDirectoryOnDispose = true)
    {
        this.DatabaseDirectory = databaseDirectory
            ?? Path.Combine(
                Path.GetTempPath(),
                "HomeServerPage.Tests",
                Guid.NewGuid().ToString("N"));
        this.deleteDatabaseDirectoryOnDispose = deleteDatabaseDirectoryOnDispose;
        Directory.CreateDirectory(this.DatabaseDirectory);

        this.ConfigureMocks();
    }

    public string DatabaseDirectory { get; }

    public Mock<IWeatherService> WeatherService { get; } = new(MockBehavior.Strict);

    public Mock<IPublicTransportService> PublicTransportService { get; } = new(MockBehavior.Strict);

    public Mock<IWeatherLocationResolver> WeatherLocationResolver { get; } = new(MockBehavior.Strict);

    public Mock<IRealtimeNotificationPublisher> NotificationPublisher { get; } = new();

    public FixedDateTimeService Clock { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:FridgeDbConnection"] =
                    $"Data Source={Path.Combine(this.DatabaseDirectory, "fridge.db")};Pooling=False",
                ["ConnectionStrings:AstronomyDbConnection"] =
                    $"Data Source={Path.Combine(this.DatabaseDirectory, "astronomy.db")};Pooling=False",
                ["ConnectionStrings:LocationsDbConnection"] =
                    $"Data Source={Path.Combine(this.DatabaseDirectory, "locations.db")};Pooling=False",
                ["ConnectionStrings:CaloriesDbConnection"] =
                    $"Data Source={Path.Combine(this.DatabaseDirectory, "calories.db")};Pooling=False",
                ["ConnectionStrings:ExerciseDbConnection"] =
                    $"Data Source={Path.Combine(this.DatabaseDirectory, "exercise.db")};Pooling=False",
                ["WeatherApi:Location"] = "Szczecin",
                ["TabletDashboardRefresh:ActiveFrom"] = "07:00",
                ["TabletDashboardRefresh:ActiveUntil"] = "23:00",
                ["TabletDashboardRefresh:Widgets:FridgeIntervalMinutes"] = "60",
                ["TabletDashboardRefresh:Widgets:PublicTransportIntervalMinutes"] = "20",
                ["TabletDashboardRefresh:Widgets:WeatherIntervalMinutes"] = "60",
                ["TabletDashboardRefresh:Widgets:AstronomyIntervalMinutes"] = "1440"
            });
        });
        builder.ConfigureTestServices(services =>
        {
            ConfigureDatabaseFactory<FridgeDbContext>(
                services,
                Path.Combine(this.DatabaseDirectory, "fridge.db"));
            ConfigureDatabaseFactory<AstronomyDbContext>(
                services,
                Path.Combine(this.DatabaseDirectory, "astronomy.db"));
            ConfigureDatabaseFactory<LocationsDbContext>(
                services,
                Path.Combine(this.DatabaseDirectory, "locations.db"));
            ConfigureDatabaseFactory<CaloriesDbContext>(
                services,
                Path.Combine(this.DatabaseDirectory, "calories.db"));
            ConfigureDatabaseFactory<ExerciseDbContext>(
                services,
                Path.Combine(this.DatabaseDirectory, "exercise.db"));

            services.RemoveAll<IDateTimeService>();
            services.AddSingleton<IDateTimeService>(this.Clock);

            services.RemoveAll<IWeatherService>();
            services.AddSingleton(this.WeatherService.Object);

            services.RemoveAll<IPublicTransportService>();
            services.AddSingleton(this.PublicTransportService.Object);

            services.RemoveAll<IWeatherLocationResolver>();
            services.AddSingleton(this.WeatherLocationResolver.Object);

            services.RemoveAll<IRealtimeNotificationPublisher>();
            services.AddSingleton(this.NotificationPublisher.Object);
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing &&
            this.deleteDatabaseDirectoryOnDispose &&
            Directory.Exists(this.DatabaseDirectory))
        {
            Directory.Delete(this.DatabaseDirectory, recursive: true);
        }
    }

    private static void ConfigureDatabaseFactory<TContext>(
        IServiceCollection services,
        string databasePath)
        where TContext : DbContext
    {
        services.RemoveAll<IDbContextFactory<TContext>>();
        services.RemoveAll<DbContextOptions<TContext>>();
        services.AddDbContextFactory<TContext>(
            options => options.UseSqlite($"Data Source={databasePath};Pooling=False"));
    }

    private void ConfigureMocks()
    {
        this.WeatherService
            .Setup(service => service.GetCurrentAsync(
                It.IsAny<SavedLocation>(),
                It.IsAny<CancellationToken>()))
            .Returns((SavedLocation location, CancellationToken _) =>
                Task.FromResult(CreateWeatherSnapshot(location)));
        this.WeatherService
            .Setup(service => service.GetForecastAsync(
                It.IsAny<SavedLocation>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .Returns((SavedLocation location, int _, CancellationToken _) =>
                Task.FromResult(new WeatherForecast(
                    CreateWeatherSnapshot(location),
                    Array.Empty<WeatherForecastDay>())));
        this.WeatherService
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

        this.PublicTransportService
            .Setup(service => service.GetDepartureBoardAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .Returns((int stopNumber, int _, CancellationToken _) =>
                Task.FromResult(CreateBoard(stopNumber)));
        this.PublicTransportService
            .Setup(service => service.GetDepartureBoardsAsync(
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .Returns((int _, CancellationToken _) =>
                Task.FromResult(new List<DepartureBoard>
                {
                    CreateBoard(12811),
                    CreateBoard(12812)
                }));

        this.WeatherLocationResolver
            .Setup(resolver => resolver.ResolveAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .Returns((string query, CancellationToken _) =>
                Task.FromResult(new SavedLocation
                {
                    Name = query,
                    Latitude = 0,
                    Longitude = 0,
                    ElevationMeters = 0,
                    TimeZoneId = "Etc/UTC"
                }));

        this.NotificationPublisher
            .Setup(publisher => publisher.PublishAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
    }

    private static WeatherSnapshot CreateWeatherSnapshot(SavedLocation location) =>
        new(
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
            null);

    private static DepartureBoard CreateBoard(int stopNumber) =>
        new(
            new Stop($"Stop {stopNumber}", stopNumber.ToString()),
            [],
            [],
            new DateTimeOffset(2026, 11, 18, 15, 0, 0, TimeSpan.Zero));
}
