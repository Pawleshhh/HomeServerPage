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
using HomeServerPage.Data.Calories;
using HomeServerPage.Data.Notifications;
using HomeServerPage.Client.Data;

namespace HomeServerPage.Client.Pages.Fridge;

public partial class ExpiringFridgeItems
{
    [Parameter]
    public string DetailsHref { get; set; } = "/fridge";

    [Parameter]
    public bool AutoRefresh { get; set; }

    [Parameter]
    public bool CompactTablet { get; set; }

    private List<FridgeItem>? Items;
    private string? errorMessage;
    private bool isLoadingItems = true;
    private bool isRefreshing;
    private int? caloriesGoal;
    private long caloriesConsumed;
    private string? caloriesError;
    private bool isLoadingCalories = true;

    private int CaloriesProgressPercent => caloriesGoal is > 0
        ? (int)Math.Clamp(Math.Round(caloriesConsumed * 100d / caloriesGoal.Value), 0, 100)
        : 0;

    private string CaloriesTodayValue => isLoadingCalories
        ? "Loading…"
        : caloriesError is not null
            ? "Unavailable"
            : caloriesConsumed.ToString("N0", System.Globalization.CultureInfo.CurrentCulture) +
                (caloriesGoal is null ? " kcal" : $" / {caloriesGoal.Value.ToString("N0", System.Globalization.CultureInfo.CurrentCulture)} kcal");

    protected override async Task OnInitializedAsync()
    {
        if (AutoRefresh)
        {
            DashboardRefreshService.WidgetRefreshRequested += this.HandleWidgetRefreshRequestedAsync;
            DashboardRefreshService.NotificationReceived += this.HandleNotificationReceivedAsync;
        }

        await this.LoadItemsAsync();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await this.LoadCaloriesAsync();
            await InvokeAsync(StateHasChanged);
        }
    }

    private async Task LoadItemsAsync()
    {
        this.isLoadingItems = true;
        this.errorMessage = null;

        try
        {
            await DateTimeService.SyncAsync();
            var items = await FridgeService.GetItemsAsync();
            this.Items = items
                .OrderBy(FridgeLabels.GetEffectiveExpirationDate)
                .Take(3)
                .ToList();
        }
        catch (Exception)
        {
            this.errorMessage = "Could not load fridge items.";
        }
        finally
        {
            this.isLoadingItems = false;
        }
    }

    private Task HandleRefreshRequestedAsync() => InvokeAsync(async () =>
    {
        this.isRefreshing = true;
        this.StateHasChanged();

        try
        {
            await this.LoadItemsAsync();
            await this.LoadCaloriesAsync();
        }
        finally
        {
            this.isRefreshing = false;
            this.StateHasChanged();
        }
    });

    private Task HandleWidgetRefreshRequestedAsync(string widgetId) =>
        widgetId == TabletDashboardWidgetRefreshOptions.Fridge
            ? this.HandleRefreshRequestedAsync()
            : Task.CompletedTask;

    private Task HandleNotificationReceivedAsync(string notificationType) =>
        notificationType switch
        {
            RealtimeNotificationTypes.FridgeChanged => this.HandleRefreshRequestedAsync(),
            RealtimeNotificationTypes.CaloriesChanged => this.HandleCaloriesRefreshAsync(),
            RealtimeNotificationTypes.ConnectionRestored => this.HandleRefreshRequestedAsync(),
            _ => Task.CompletedTask
        };

    private Task HandleCaloriesRefreshAsync() => InvokeAsync(async () =>
    {
        await this.LoadCaloriesAsync();
        this.StateHasChanged();
    });

    private async Task LoadCaloriesAsync()
    {
        isLoadingCalories = true;
        caloriesError = null;

        try
        {
            var clock = await JS.InvokeAsync<BrowserClock>("homeServerPageCalories.getLocalClock");
            var today = DateOnly.ParseExact(
                clock.Date,
                "yyyy-MM-dd",
                System.Globalization.CultureInfo.InvariantCulture);
            var goalTask = CaloriesService.GetDailyGoalAsync();
            var entriesTask = CaloriesService.GetEntriesAsync(today, today);
            await Task.WhenAll(goalTask, entriesTask);

            caloriesGoal = await goalTask;
            caloriesConsumed = (await entriesTask).Sum(entry => (long)entry.Calories);
        }
        catch (Exception exception)
        {
            Logger.LogError(exception, "Could not load today's calorie total for the fridge widget.");
            caloriesError = "Unavailable";
        }
        finally
        {
            isLoadingCalories = false;
        }
    }

    private sealed record BrowserClock(string Date, string Time);

    public void Dispose()
    {
        if (AutoRefresh)
        {
            DashboardRefreshService.WidgetRefreshRequested -= this.HandleWidgetRefreshRequestedAsync;
            DashboardRefreshService.NotificationReceived -= this.HandleNotificationReceivedAsync;
        }
    }
}
