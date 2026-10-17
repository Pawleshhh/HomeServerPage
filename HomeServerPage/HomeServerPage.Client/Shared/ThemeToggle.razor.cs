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

namespace HomeServerPage.Client.Shared;

public partial class ThemeToggle
{
    private bool isDarkTheme;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
        {
            return;
        }

        var theme = await JS.InvokeAsync<string?>("homeServerPageTheme.get");
        isDarkTheme = theme == "dark";
        StateHasChanged();
    }

    private async Task ToggleTheme()
    {
        isDarkTheme = !isDarkTheme;
        await JS.InvokeVoidAsync("homeServerPageTheme.set", isDarkTheme ? "dark" : "light");
    }
}
