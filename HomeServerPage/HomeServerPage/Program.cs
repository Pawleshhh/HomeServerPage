using HomeServerPage.Client.Pages;
using HomeServerPage.Components;
using HomeServerPage.Data.Fridge;
using HomeServerPage.Data.PublicTransport;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddControllers();

var fridgeConnection = builder.Configuration.GetConnectionString("FridgeDbConnection");

builder.Services.AddDbContextFactory<FridgeDbContext>(op => op.UseSqlite(fridgeConnection));
builder.Services.AddScoped<IFridgeService, FridgeService>();

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddScoped<IPublicTransportService, PublicTransportMockService>();
}
else
{
    builder.Services.AddHttpClient<IPublicTransportService, PublicTransportService>(client =>
    {
        client.BaseAddress = new Uri("https://www.zditm.szczecin.pl/api/v2/");
    });
}

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<FridgeDbContext>>();
    using var db = dbFactory.CreateDbContext();
    db.Database.Migrate();
}

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
app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(HomeServerPage.Client._Imports).Assembly);

app.Run();
