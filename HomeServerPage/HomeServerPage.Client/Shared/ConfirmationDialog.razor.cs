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

public partial class ConfirmationDialog
{
    [Parameter]
    public bool IsVisible { get; set; }

    [Parameter]
    public string Id { get; set; } = "confirmation-dialog";

    [Parameter]
    public string Title { get; set; } = "Confirm action";

    [Parameter]
    public string? Message { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter]
    public string ConfirmText { get; set; } = "Confirm";

    [Parameter]
    public string CancelText { get; set; } = "Cancel";

    [Parameter]
    public string ConfirmButtonClass { get; set; } = "btn-danger";

    [Parameter]
    public string Icon { get; set; } = "!";

    [Parameter]
    public EventCallback OnConfirm { get; set; }

    [Parameter]
    public EventCallback OnCancel { get; set; }

    private string TitleId => $"{Id}-title";

    private string DescriptionId => $"{Id}-description";

    private bool HasMessage => ChildContent is not null || !string.IsNullOrWhiteSpace(Message);

    private Task HandleConfirmAsync() => OnConfirm.InvokeAsync();

    private Task HandleCancelAsync() => OnCancel.InvokeAsync();

    private async Task HandleKeyDownAsync(KeyboardEventArgs args)
    {
        if (args.Key == "Escape")
        {
            await OnCancel.InvokeAsync();
        }
    }
}
