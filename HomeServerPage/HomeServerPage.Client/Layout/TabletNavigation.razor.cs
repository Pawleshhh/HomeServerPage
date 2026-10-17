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

namespace HomeServerPage.Client.Layout;

public partial class TabletNavigation
{
    [Parameter]
    public EventCallback OnScreensaverRequested { get; set; }

    private DateTime? currentDateTime;

    protected override async Task OnInitializedAsync()
    {
        await DateTimeService.SyncAsync();
        this.currentDateTime = DateTimeService.Now;
    }

    private void RefreshPage()
    {
        NavigationManager.Refresh(forceReload: true);
    }
}
