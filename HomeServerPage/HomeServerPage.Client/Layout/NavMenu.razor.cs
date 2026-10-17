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

public partial class NavMenu
{
    private bool isMenuOpen;
    protected override void OnInitialized()
    {
        NavigationManager.LocationChanged += OnLocationChanged;
    }

    private void ToggleMenu() => isMenuOpen = !isMenuOpen;

    private void CloseMenu() => isMenuOpen = false;

    private void OnLocationChanged(object? sender, LocationChangedEventArgs args)
    {
        if (!isMenuOpen)
        {
            return;
        }

        isMenuOpen = false;
        _ = InvokeAsync(StateHasChanged);
    }

    public void Dispose()
    {
        NavigationManager.LocationChanged -= OnLocationChanged;
    }
}
