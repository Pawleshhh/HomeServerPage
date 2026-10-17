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

public partial class PublicTransport
{
    private List<DepartureBoard> boards = [];
    private bool isLoading;
    private string? errorMessage;

    protected override async Task OnInitializedAsync()
    {
        await LoadBoardsAsync();
    }

    private async Task LoadBoardsAsync()
    {
        isLoading = true;
        errorMessage = null;
        try
        {
            boards = await PublicTransportService.GetDepartureBoardsAsync();
        }
        catch (Exception)
        {
            errorMessage = "Could not load the departure boards.";
        }
        finally
        {
            isLoading = false;
        }
    }
}
