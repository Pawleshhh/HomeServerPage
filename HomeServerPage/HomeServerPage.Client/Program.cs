using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using HomeServerPage.Data.Fridge;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddScoped<IFridgeService, FridgeHttpService>();

await builder.Build().RunAsync();
