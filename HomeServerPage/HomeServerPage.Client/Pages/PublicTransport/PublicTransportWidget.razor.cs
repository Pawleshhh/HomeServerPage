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
using HomeServerPage.Data.PublicTransport;

namespace HomeServerPage.Client.Pages.PublicTransport;

public partial class PublicTransportWidget
{
    [Parameter]
    public string DetailsHref { get; set; } = "/public-transport";

    [Parameter]
    public bool CompactTablet { get; set; }

    [Parameter]
    public bool AutoRefresh { get; set; }

    private List<DepartureBoard>? Boards;
    private string? errorMessage;

    protected override async Task OnInitializedAsync()
    {
        if (AutoRefresh)
        {
            DashboardRefreshService.RefreshRequested += this.HandleRefreshRequestedAsync;
        }

        await this.LoadDeparturesAsync();
    }

    private async Task LoadDeparturesAsync()
    {
        this.errorMessage = null;

        try
        {
            this.Boards = await PublicTransportService.GetDepartureBoardsAsync(limit: 3);
        }
        catch (Exception)
        {
            this.errorMessage = "Could not load departures.";
        }
    }

    private Task HandleRefreshRequestedAsync() => InvokeAsync(async () =>
    {
        await this.LoadDeparturesAsync();
        this.StateHasChanged();
    });

    public void Dispose()
    {
        if (AutoRefresh)
        {
            DashboardRefreshService.RefreshRequested -= this.HandleRefreshRequestedAsync;
        }
    }
}
