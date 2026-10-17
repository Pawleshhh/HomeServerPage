using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using HomeServerPage.Client.DependencyInjection;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddHomeServerPageClientServices(builder.HostEnvironment);

await builder.Build().RunAsync();
