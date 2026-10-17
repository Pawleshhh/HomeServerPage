using HomeServerPage.Components;
using HomeServerPage.Data.Notifications;
using HomeServerPage.DependencyInjection;
using HomeServerPage.Hubs;
using HomeServerPage.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddHomeServerPageServices(builder.Configuration, builder.Environment);

var app = builder.Build();

app.Services.MigrateApplicationDatabases();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapControllers();
app.MapHub<RealtimeNotificationsHub>(RealtimeNotificationContract.HubPath);
app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(HomeServerPage.Client._Imports).Assembly);

app.Run();
