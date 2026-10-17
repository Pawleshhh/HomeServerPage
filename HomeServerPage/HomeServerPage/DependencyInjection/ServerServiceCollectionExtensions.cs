using HomeServerPage.Client.Data;
using HomeServerPage.Client.Data.Astronomy.Telescopes;
using HomeServerPage.Data;
using HomeServerPage.Data.Astronomy;
using HomeServerPage.Data.Astronomy.Telescopes;
using HomeServerPage.Data.Calories;
using HomeServerPage.Data.Fridge;
using HomeServerPage.Data.Notifications;
using HomeServerPage.Data.PublicTransport;
using HomeServerPage.Data.Weather;
using Microsoft.EntityFrameworkCore;

namespace HomeServerPage.DependencyInjection;

public static class ServerServiceCollectionExtensions
{
    public static IServiceCollection AddHomeServerPageServices(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        services.AddRazorComponents()
            .AddInteractiveServerComponents()
            .AddInteractiveWebAssemblyComponents();

        services.AddControllers();
        services.AddSignalR();

        services.AddSingleton<IDateTimeService, DateTimeService>();
        services.AddOptions<TabletDashboardRefreshOptions>()
            .Bind(configuration.GetSection(TabletDashboardRefreshOptions.SectionName))
            .Validate(
                options => options.IsValid,
                "Tablet dashboard refresh settings must use a positive interval up to 1440 minutes and a valid same-day active range.")
            .ValidateOnStart();
        services.AddScoped<ITabletDashboardRefreshSettingsProvider, OptionsTabletDashboardRefreshSettingsProvider>();
        services.AddScoped<TabletDashboardRefreshService>();
        services.AddSingleton<RealtimeNotificationService>();
        services.AddSingleton<IRealtimeNotificationEventSource>(serviceProvider =>
            serviceProvider.GetRequiredService<RealtimeNotificationService>());
        services.AddSingleton<IRealtimeNotificationPublisher>(serviceProvider =>
            serviceProvider.GetRequiredService<RealtimeNotificationService>());

        AddFridgeServices(services, configuration);
        AddAstronomyServices(services, configuration);
        AddCaloriesServices(services, configuration);
        AddWeatherServices(services, configuration, environment);
        AddPublicTransportServices(services, environment);

        return services;
    }

    private static void AddFridgeServices(IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("FridgeDbConnection");
        services.AddDbContextFactory<FridgeDbContext>(options => options.UseSqlite(connectionString));
        services.AddScoped<IFridgeService, FridgeService>();
    }

    private static void AddAstronomyServices(IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("AstronomyDbConnection");
        services.AddDbContextFactory<AstronomyDbContext>(options => options.UseSqlite(connectionString));
        services.AddScoped<IAstronomyService, AstronomyService>();
        services.AddScoped<IObservationPointService, ObservationPointService>();
        services.AddScoped<ITelescopeService, TelescopeService>();
    }

    private static void AddCaloriesServices(IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("CaloriesDbConnection");
        services.AddDbContextFactory<CaloriesDbContext>(options => options.UseSqlite(connectionString));
        services.AddScoped<ICaloriesService, CaloriesService>();
    }

    private static void AddWeatherServices(
        IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        if (environment.IsDevelopment())
        {
            services.AddScoped<IWeatherService, MockWeatherService>();
            return;
        }

        services.AddOptions<WeatherApiOptions>()
            .Configure(options =>
            {
                options.ApiKey = configuration["WEATHER_API"]
                    ?? configuration["WeatherApi:ApiKey"]
                    ?? string.Empty;
                options.Location = configuration["WeatherApi:Location"] ?? "Szczecin";
                options.ForecastDays = int.TryParse(
                    configuration["WeatherApi:ForecastDays"],
                    out var forecastDays)
                    ? Math.Clamp(forecastDays, 1, 14)
                    : 3;
            });
        services.AddHttpClient<IWeatherService, WeatherApiService>(client =>
        {
            client.BaseAddress = new Uri("https://api.weatherapi.com/v1/");
        });
    }

    private static void AddPublicTransportServices(
        IServiceCollection services,
        IHostEnvironment environment)
    {
        if (environment.IsDevelopment())
        {
            services.AddScoped<IPublicTransportService, PublicTransportMockService>();
            return;
        }

        services.AddHttpClient<IPublicTransportService, PublicTransportService>(client =>
        {
            client.BaseAddress = new Uri("https://www.zditm.szczecin.pl/api/v2/");
        });
    }
}
