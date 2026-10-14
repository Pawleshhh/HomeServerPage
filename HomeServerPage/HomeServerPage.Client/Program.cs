using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using HomeServerPage.Data;
using HomeServerPage.Data.Astronomy;
using HomeServerPage.Data.Fridge;
using HomeServerPage.Data.Notifications;
using HomeServerPage.Data.PublicTransport;
using HomeServerPage.Data.Weather;
using HomeServerPage.Client.Data;
using HomeServerPage.Client.Data.Astronomy.Telescopes;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddScoped<IDateTimeService, ClientDateTimeService>();
builder.Services.AddScoped<ITabletDashboardRefreshSettingsProvider, HttpTabletDashboardRefreshSettingsProvider>();
builder.Services.AddScoped<IRealtimeNotificationEventSource, SignalRRealtimeNotificationEventSource>();
builder.Services.AddScoped<TabletDashboardRefreshService>();
builder.Services.AddScoped<IFridgeService, FridgeHttpService>();
builder.Services.AddScoped<IAstronomyService, AstronomyHttpService>();
builder.Services.AddScoped<IObservationPointService, ObservationPointHttpService>();
builder.Services.AddScoped<ITelescopeService, TelescopeHttpService>();
if (builder.HostEnvironment.IsDevelopment())
{
    builder.Services.AddScoped<IWeatherService, MockWeatherService>();
}
else
{
    builder.Services.AddScoped<IWeatherService, WeatherHttpService>();
}

if (builder.HostEnvironment.IsDevelopment())
{
    builder.Services.AddScoped<IPublicTransportService, PublicTransportMockService>();
}
else
{
    builder.Services.AddScoped<IPublicTransportService, PublicTransportHttpService>();
}

await builder.Build().RunAsync();
