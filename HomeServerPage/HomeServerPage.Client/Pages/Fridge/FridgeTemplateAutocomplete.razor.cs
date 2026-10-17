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
using HomeServerPage.Data.Fridge;
using System.Diagnostics.CodeAnalysis;

namespace HomeServerPage.Client.Pages.Fridge;

public partial class FridgeTemplateAutocomplete
{
    [Parameter]
    public string Id { get; set; } = "fridge-template-name";

    [Parameter]
    public IReadOnlyList<FridgeItemTemplate> Templates { get; set; } = [];

    [Parameter]
    public EventCallback<FridgeItemTemplate> TemplateSelected { get; set; }

    [Parameter]
    public EventCallback ManualValueChanged { get; set; }

    private bool IsSuggestionsVisible { get; set; }

    private int ActiveSuggestionIndex { get; set; } = -1;

    private string SuggestionsId => $"{Id}-suggestions";

    private string? ActiveSuggestionId
        => ActiveSuggestionIndex >= 0 && ActiveSuggestionIndex < Suggestions.Count
            ? GetSuggestionId(ActiveSuggestionIndex)
            : null;

    private IReadOnlyList<FridgeItemTemplate> Suggestions
        => (string.IsNullOrWhiteSpace(CurrentValueAsString)
                ? Templates
                : Templates.Where(template => template.Name.Contains(CurrentValueAsString, StringComparison.OrdinalIgnoreCase)))
            .Take(8)
            .ToList();

    private void HandleInput(ChangeEventArgs args)
    {
        CurrentValueAsString = args.Value?.ToString() ?? string.Empty;
        IsSuggestionsVisible = true;
        ActiveSuggestionIndex = Suggestions.Count > 0 ? 0 : -1;
        _ = ManualValueChanged.InvokeAsync();
    }

    private void HandleFocus(FocusEventArgs args)
    {
        IsSuggestionsVisible = true;
        ActiveSuggestionIndex = Suggestions.Count > 0 ? 0 : -1;
    }

    private async Task HandleKeyDown(KeyboardEventArgs args)
    {
        if (Suggestions.Count == 0)
        {
            if (args.Key is "Escape")
            {
                IsSuggestionsVisible = false;
                ActiveSuggestionIndex = -1;
            }

            return;
        }

        switch (args.Key)
        {
            case "ArrowDown":
                IsSuggestionsVisible = true;
                ActiveSuggestionIndex = (ActiveSuggestionIndex + 1) % Suggestions.Count;
                break;
            case "ArrowUp":
                IsSuggestionsVisible = true;
                ActiveSuggestionIndex = ActiveSuggestionIndex <= 0
                    ? Suggestions.Count - 1
                    : ActiveSuggestionIndex - 1;
                break;
            case "Enter" when IsSuggestionsVisible && ActiveSuggestionIndex >= 0:
                await SelectTemplateAsync(Suggestions[ActiveSuggestionIndex]);
                break;
            case "Escape":
                IsSuggestionsVisible = false;
                ActiveSuggestionIndex = -1;
                break;
        }
    }

    private async Task SelectTemplateAsync(FridgeItemTemplate template)
    {
        CurrentValueAsString = template.Name;
        IsSuggestionsVisible = false;
        ActiveSuggestionIndex = -1;
        await TemplateSelected.InvokeAsync(template);
    }

    private string GetSuggestionId(int index) => $"{SuggestionsId}-{index}";

    protected override bool TryParseValueFromString(
        string? value,
        out string result,
        [NotNullWhen(false)] out string? validationErrorMessage)
    {
        result = value ?? string.Empty;
        validationErrorMessage = null;
        return true;
    }
}
