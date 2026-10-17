using Microsoft.AspNetCore.Components;
using System.Net.Http;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.Web;
using static Microsoft.AspNetCore.Components.Web.RenderMode;
using Microsoft.AspNetCore.Components.Web.Virtualization;
using Microsoft.JSInterop;
using HomeServerPage.Client;
using HomeServerPage.Client.Layout;
using HomeServerPage.Client.Shared;
using HomeServerPage.Shared.Extensions;
using HomeServerPage.Client.Pages.Astronomy;
using HomeServerPage.Client.Pages.Fridge;
using HomeServerPage.Client.Pages.PublicTransport;
using HomeServerPage.Client.Pages.Weather;

namespace HomeServerPage.Client.Pages.Tablet;

public partial class TabletDashboard
{
    private readonly CancellationTokenSource refreshCancellation = new();
    private Task refreshTask = Task.CompletedTask;

    protected override void OnInitialized()
    {
        this.refreshTask = DashboardRefreshService.RunAsync(this.refreshCancellation.Token);
    }

    public async ValueTask DisposeAsync()
    {
        this.refreshCancellation.Cancel();
        try
        {
            await this.refreshTask;
        }
        catch (OperationCanceledException)
        {
        }
        finally
        {
            this.refreshCancellation.Dispose();
        }
    }
}
