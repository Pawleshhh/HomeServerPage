using HomeServerPage.Client.Data;
using HomeServerPage.Client.Data.Astronomy.Telescopes;
using HomeServerPage.Data;
using HomeServerPage.Data.Astronomy;
using HomeServerPage.Data.Calories;
using HomeServerPage.Data.Fridge;
using HomeServerPage.Data.Notifications;
using HomeServerPage.Data.PublicTransport;
using HomeServerPage.Data.Weather;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace HomeServerPage.Client.DependencyInjection;

public static class ClientServiceCollectionExtensions
{
    public static IServiceCollection AddHomeServerPageClientServices(
        this IServiceCollection services,
        IWebAssemblyHostEnvironment environment)
    {
        services.AddScoped(serviceProvider =>
            new HttpClient { BaseAddress = new Uri(environment.BaseAddress) });
        services.AddScoped<IDateTimeService, ClientDateTimeService>();
        services.AddScoped<ITabletDashboardRefreshSettingsProvider, HttpTabletDashboardRefreshSettingsProvider>();
        services.AddScoped<IRealtimeNotificationEventSource, SignalRRealtimeNotificationEventSource>();
        services.AddScoped<TabletDashboardRefreshService>();
        services.AddScoped<IFridgeService, FridgeHttpService>();
        services.AddScoped<ICaloriesService, CaloriesHttpService>();
        services.AddScoped<IAstronomyService, AstronomyHttpService>();
        services.AddScoped<IObservationPointService, ObservationPointHttpService>();
        services.AddScoped<ITelescopeService, TelescopeHttpService>();
        AddWeatherServices(services, environment);
        AddPublicTransportServices(services, environment);

        return services;
    }

    private static void AddWeatherServices(
        IServiceCollection services,
        IWebAssemblyHostEnvironment environment)
    {
        if (environment.IsDevelopment())
        {
            services.AddScoped<IWeatherService, MockWeatherService>();
            return;
        }

        services.AddScoped<IWeatherService, WeatherHttpService>();
    }

    private static void AddPublicTransportServices(
        IServiceCollection services,
        IWebAssemblyHostEnvironment environment)
    {
        if (environment.IsDevelopment())
        {
            services.AddScoped<IPublicTransportService, PublicTransportMockService>();
            return;
        }

        services.AddScoped<IPublicTransportService, PublicTransportHttpService>();
    }
}
