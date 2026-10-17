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

namespace HomeServerPage.Client.Pages.Fridge;

public partial class Fridge
{
    public FridgeItemModel EditedFridgeItem { get; set; } = FridgeItemModel.CreateClearEditedFridgeItem(DateTime.Now);
    public List<FridgeItem> Items { get; set; } = [];
    public int? SelectedItemId { get; set; }

    private List<FridgeItemTemplate> Templates { get; set; } = [];

    private bool IsQuickTemplateSelection { get; set; }

    private FridgeItem? PendingDeleteItem { get; set; }

    private string? StatusMessage { get; set; }

    private bool StatusMessageIsError { get; set; }

    private static IReadOnlyList<QuantityType> QuantityTypes { get; } = Enum.GetValues<QuantityType>();

    private enum TimeAfterOpenUnit
    {
        Hours,
        Days
    }

    private TimeAfterOpenUnit SelectedTimeAfterOpenUnit { get; set; } = TimeAfterOpenUnit.Hours;

    private int? TimeAfterOpenDisplayValue
    {
        get
        {
            if (EditedFridgeItem.TimeAfterOpen is null)
            {
                return null;
            }

            return SelectedTimeAfterOpenUnit switch
            {
                TimeAfterOpenUnit.Days => EditedFridgeItem.TimeAfterOpen.Value / 24,
                _ => EditedFridgeItem.TimeAfterOpen
            };
        }
        set
        {
            EditedFridgeItem.TimeAfterOpen = value is null
                ? null
                : SelectedTimeAfterOpenUnit switch
                {
                    TimeAfterOpenUnit.Days => value.Value * 24,
                    _ => value.Value
                };
        }
    }

    private enum FridgeSortColumn
    {
        Added,
        Opened,
        Expires,
        DaysLeft
    }

    private FridgeSortColumn CurrentSortColumn { get; set; } = FridgeSortColumn.DaysLeft;

    private bool SortAscending { get; set; } = true;

    private List<FridgeItem> SortedItems
    {
        get
        {
            IOrderedEnumerable<FridgeItem> ordered = CurrentSortColumn switch
            {
                FridgeSortColumn.Added => SortAscending
                    ? Items.OrderBy(i => i.AddedDate)
                    : Items.OrderByDescending(i => i.AddedDate),
                FridgeSortColumn.Opened => SortAscending
                    ? Items.OrderBy(i => i.OpenDate)
                    : Items.OrderByDescending(i => i.OpenDate),
                FridgeSortColumn.Expires => SortAscending
                    ? Items.OrderBy(i => i.ExpirationDate)
                    : Items.OrderByDescending(i => i.ExpirationDate),
                FridgeSortColumn.DaysLeft => SortAscending
                    ? Items.OrderBy(FridgeLabels.GetEffectiveExpirationDate)
                    : Items.OrderByDescending(FridgeLabels.GetEffectiveExpirationDate),
                _ => Items.OrderBy(i => i.Id)
            };

            return ordered.ToList();
        }
    }

    protected override async Task OnInitializedAsync()
    {
        await DateTimeService.SyncAsync();
        EditedFridgeItem = FridgeItemModel.CreateClearEditedFridgeItem(DateTimeService.Now);
        Items = await FridgeService.GetItemsAsync();
        Templates = await FridgeService.GetTemplatesAsync();
        await base.OnInitializedAsync();
    }

    private async Task OnValidFridgeItemSubmitAsync(EditContext args)
    {
        var itemName = EditedFridgeItem.Name ?? "Item";

        if (SelectedItemId is int selectedId)
        {
            var updated = await FridgeService.UpdateItemAsync(this.EditedFridgeItem.ToFridgeItem() with { Id = selectedId });
            SetStatus(
                updated ? $"{itemName} updated." : $"Could not update {itemName}.",
                !updated);
        }
        else
        {
            var created = await FridgeService.AddItemAsync(
                this.EditedFridgeItem.ToFridgeItem(),
                saveAsTemplate: !IsQuickTemplateSelection);
            SetStatus(IsQuickTemplateSelection
                ? $"{created.Name} added from quick history."
                : $"{created.Name} added to the fridge and saved to quick history.");
        }

        ClearSelection();
        Items = await FridgeService.GetItemsAsync();
        Templates = await FridgeService.GetTemplatesAsync();
    }

    private void ClearSelection()
    {
        SelectedItemId = null;
        IsQuickTemplateSelection = false;
        EditedFridgeItem = FridgeItemModel.CreateClearEditedFridgeItem(DateTimeService.Now);
    }

    private void ClearQuickTemplateSelection()
    {
        if (SelectedItemId is null)
        {
            IsQuickTemplateSelection = false;
        }
    }

    private void ApplyTemplate(FridgeItemTemplate template)
    {
        if (SelectedItemId is not null)
        {
            return;
        }

        EditedFridgeItem = FridgeItemModel.FromFridgeItemTemplate(template, DateTimeService.Now);
        IsQuickTemplateSelection = true;
    }

    private void SelectItem(FridgeItem item)
    {
        if (SelectedItemId == item.Id)
        {
            ClearSelection();
            return;
        }

        SelectedItemId = item.Id;
        IsQuickTemplateSelection = false;
        EditedFridgeItem = FridgeItemModel.FromFridgeItem(item);
    }

    private void SelectItemOnKeyDown(KeyboardEventArgs args, FridgeItem item)
    {
        if (args.Key is "Enter" or " ")
        {
            SelectItem(item);
        }
    }

    private void SortBy(FridgeSortColumn column)
    {
        if (CurrentSortColumn == column)
        {
            SortAscending = !SortAscending;
        }
        else
        {
            CurrentSortColumn = column;
            SortAscending = true;
        }
    }

    private string GetSortIcon(FridgeSortColumn column)
    {
        if (CurrentSortColumn != column)
        {
            return string.Empty;
        }

        return SortAscending ? "\u25b2" : "\u25bc";
    }

    private async Task CopyItemAsync(FridgeItem item)
    {
        var cloned = FridgeItemTemplate
            .FromFridgeItem(item)
            .ToFridgeItem(DateTimeService.Now);
        var created = await FridgeService.AddItemAsync(cloned);

        Items = await FridgeService.GetItemsAsync();
        SelectItem(created);
        SetStatus($"{created.Name} copied as a new unopened item.");
    }

    private static double GetQuantityStep(FridgeItem item)
        => item.QuantityType == QuantityType.Weight ? 0.1 : 1;

    private async Task ChangeQuantityAsync(FridgeItem item, double adjustment)
    {
        var quantity = Math.Round(Math.Max(0, item.QuantityValue + adjustment), 2);
        if (quantity == item.QuantityValue)
        {
            return;
        }

        var updated = await FridgeService.UpdateItemAsync(item with { QuantityValue = quantity });
        Items = await FridgeService.GetItemsAsync();

        if (updated && SelectedItemId == item.Id)
        {
            EditedFridgeItem.QuanitityValue = quantity;
        }
    }

    private async Task OpenItemAsync(FridgeItem item)
    {
        var openedAt = DateTimeService.Now.Date;
        var updated = await FridgeService.UpdateItemAsync(item with { OpenDate = openedAt });

        Items = await FridgeService.GetItemsAsync();

        if (updated && SelectedItemId == item.Id)
        {
            EditedFridgeItem.OpenDate = openedAt;
        }
    }

    private void RequestDelete(FridgeItem item)
    {
        PendingDeleteItem = item;
    }

    private void CancelDelete()
    {
        PendingDeleteItem = null;
    }

    private async Task ConfirmDeleteAsync()
    {
        if (PendingDeleteItem is not FridgeItem item)
        {
            return;
        }

        PendingDeleteItem = null;
        var removed = await FridgeService.RemoveItemAsync(item.Id);
        Items = await FridgeService.GetItemsAsync();

        if (!removed)
        {
            SetStatus($"Could not remove {item.Name}.", isError: true);
            return;
        }

        if (SelectedItemId == item.Id)
        {
            ClearSelection();
        }

        SetStatus($"{item.Name} removed from the fridge.");
    }

    private void SetStatus(string message, bool isError = false)
    {
        StatusMessage = message;
        StatusMessageIsError = isError;
    }
}
