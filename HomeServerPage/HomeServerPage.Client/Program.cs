using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using HomeServerPage.Data;
using HomeServerPage.Data.Astronomy;
using HomeServerPage.Data.Fridge;
using HomeServerPage.Data.PublicTransport;
using HomeServerPage.Client.Data.Astronomy.Telescopes;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddScoped<IDateTimeService, ClientDateTimeService>();
builder.Services.AddScoped<IFridgeService, FridgeHttpService>();
builder.Services.AddScoped<IAstronomyService, AstronomyHttpService>();
builder.Services.AddScoped<ITelescopeService, TelescopeHttpService>();

if (builder.HostEnvironment.IsDevelopment())
{
    builder.Services.AddScoped<IPublicTransportService, PublicTransportMockService>();
}
else
{
    builder.Services.AddScoped<IPublicTransportService, PublicTransportHttpService>();
}

await builder.Build().RunAsync();
