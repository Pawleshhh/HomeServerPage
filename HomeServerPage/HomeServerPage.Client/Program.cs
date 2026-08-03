using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using HomeServerPage.Data.Fridge;
using HomeServerPage.Data.PublicTransport;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddScoped<IFridgeService, FridgeHttpService>();

if (builder.HostEnvironment.IsDevelopment())
{
    builder.Services.AddScoped<IPublicTransportService, PublicTransportMockService>();
}
else
{
    builder.Services.AddScoped<IPublicTransportService, PublicTransportHttpService>();
}

await builder.Build().RunAsync();
