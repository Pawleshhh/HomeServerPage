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
using System.Globalization;
using HomeServerPage.Data.Calories;
using HomeServerPage.Data.Notifications;

namespace HomeServerPage.Client.Pages.Calories;

public partial class Calories
{
    private readonly CancellationTokenSource lifetimeCancellation = new();
    private List<CaloriesFood> foods = [];
    private List<CaloriesLogEntry> weekEntries = [];
    private List<CaloriesLogEntry> dayEntries = [];
    private List<CaloriesChartPoint> historyPoints = [];
    private CaloriesFoodDraft foodDraft = new();
    private CaloriesEntryDraft savedEntryDraft = new();
    private CustomCaloriesEntryDraft customEntryDraft = new();
    private CaloriesLoggedEntryDraft entryEditDraft = new();
    private CaloriesGoalDraft goalDraft = new();
    private CaloriesTab activeTab = CaloriesTab.Daily;
    private EntryMode entryMode = EntryMode.SavedFood;
    private HistoryPeriod historyPeriod = HistoryPeriod.Daily;
    private DateOnly today;
    private DateOnly selectedDate;
    private DateOnly historyFrom;
    private DateOnly historyThrough;
    private int? dailyGoal;
    private int? editingFoodId;
    private int? foodPendingDeleteId;
    private int? editingEntryId;
    private int? entryPendingDeleteId;
    private int? selectedChartPointIndex;
    private decimal? entryEditServingsBaseline;
    private bool showTabletEntryForm;
    private string entryTime = "12:00";
    private string? entryNotes;
    private string? statusMessage;
    private string? loadError;
    private string? historyError;
    private bool statusIsError;
    private bool isLoading = true;
    private bool isLoadingHistory;
    private bool isSavingFood;
    private bool isSavingEntry;
    private bool isSavingEntryEdit;
    private bool isDeletingEntry;
    private bool isSavingGoal;
    private bool isNotificationsStarted;
    private bool isDisposed;
    private IJSObjectReference? caloriesJsModule;
    private bool isTabletMode => TabletMode;

    [Parameter]
    public bool TabletMode { get; set; }

    private DateOnly WeekStart => StartOfWeek(selectedDate);
    private DateOnly WeekEnd => WeekStart.AddDays(6);
    private long DailyCalories => dayEntries.Sum(entry => (long)entry.Calories);
    private long WeeklyCalories => weekEntries.Sum(entry => (long)entry.Calories);
    private int? DailyCarbohydrates => SumMacro(dayEntries, entry => entry.CarbohydratesGrams);
    private int? DailyProtein => SumMacro(dayEntries, entry => entry.ProteinGrams);
    private int? DailyFat => SumMacro(dayEntries, entry => entry.FatGrams);
    private int? WeeklyCarbohydrates => SumMacro(weekEntries, entry => entry.CarbohydratesGrams);
    private int? WeeklyProtein => SumMacro(weekEntries, entry => entry.ProteinGrams);
    private int? WeeklyFat => SumMacro(weekEntries, entry => entry.FatGrams);
    private int GoalProgressPercent => dailyGoal is > 0
        ? (int)Math.Clamp(Math.Round(DailyCalories * 100d / dailyGoal.Value), 0, 100)
        : 0;
    private long HistoryTotal => historyPoints.Sum(point => point.Calories);
    private long HistoryAverage => historyPoints.Count == 0
        ? 0
        : (long)Math.Round(historyPoints.Average(point => point.Calories));
    private int ChartColumnWidth => historyPeriod == HistoryPeriod.Daily ? 42 : 62;
    private long ChartAxisMaximum => Math.Max(
        500,
        ((historyPoints.Count == 0 ? 0 : historyPoints.Max(point => point.Calories)) + 499) / 500 * 500);
    private CaloriesChartPoint? SelectedChartPoint =>
        selectedChartPointIndex is int index && index >= 0 && index < historyPoints.Count
            ? historyPoints[index]
            : null;
    private string HistoryPeriodLabel => historyPeriod switch
    {
        HistoryPeriod.Daily => "Day",
        HistoryPeriod.Weekly => "Week",
        _ => "Month"
    };
    private string HistoryRangeText => historyPeriod switch
    {
        HistoryPeriod.Daily => $"Last 7 days · {historyFrom.ToString("d MMM", CultureInfo.CurrentCulture)}–{historyThrough.ToString("d MMM yyyy", CultureInfo.CurrentCulture)}",
        HistoryPeriod.Weekly => $"Last 12 calendar weeks · {historyFrom.ToString("d MMM yyyy", CultureInfo.CurrentCulture)}–{historyThrough.ToString("d MMM yyyy", CultureInfo.CurrentCulture)}",
        _ => $"Last 12 calendar months · {historyFrom.ToString("MMM yyyy", CultureInfo.CurrentCulture)}–{historyThrough.ToString("MMM yyyy", CultureInfo.CurrentCulture)}"
    };
    private string ChartAccessibleLabel => $"{HistoryPeriodLabel} calorie totals for {HistoryRangeText}. Total {HistoryTotal:N0} kilocalories.";

    protected override void OnInitialized()
    {
        NotificationEventSource.NotificationReceived += HandleNotificationReceivedAsync;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
        {
            return;
        }

        try
        {
            await InitializeTrackerAsync(lifetimeCancellation.Token);
        }
        catch (OperationCanceledException) when (lifetimeCancellation.IsCancellationRequested)
        {
        }
        catch (Exception exception)
        {
            Logger.LogError(exception, "Could not initialize the Calories tracker.");
            loadError = "The Calories tracker could not be loaded. Check the connection and try again.";
        }
        finally
        {
            isLoading = false;
            if (!isDisposed)
            {
                await InvokeAsync(StateHasChanged);
            }
        }
    }

    private async Task LoadTrackerDataAsync(CancellationToken cancellationToken)
    {
        var foodsTask = CaloriesService.GetFoodsAsync(cancellationToken);
        var goalTask = CaloriesService.GetDailyGoalAsync(cancellationToken);
        var entriesTask = CaloriesService.GetEntriesAsync(WeekStart, WeekEnd, cancellationToken);
        await Task.WhenAll(foodsTask, goalTask, entriesTask);

        foods = await foodsTask;
        dailyGoal = await goalTask;
        goalDraft.Calories = dailyGoal;
        SetWeekEntries(await entriesTask);
    }

    private async Task RetryLoadingAsync()
    {
        loadError = null;
        isLoading = true;
        await InvokeAsync(StateHasChanged);
        try
        {
            await InitializeTrackerAsync(lifetimeCancellation.Token);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            Logger.LogError(exception, "Could not reload the Calories tracker.");
            loadError = "The Calories tracker could not be loaded. Check the connection and try again.";
        }
        finally
        {
            isLoading = false;
        }
    }

    private async Task InitializeTrackerAsync(CancellationToken cancellationToken)
    {
        caloriesJsModule ??= await JS.InvokeAsync<IJSObjectReference>(
            "import",
            cancellationToken,
            "./Pages/Calories/Calories.razor.js");
        var clock = await caloriesJsModule.InvokeAsync<BrowserClock>(
            "getLocalClock",
            cancellationToken);
        today = DateOnly.ParseExact(clock.Date, "yyyy-MM-dd", CultureInfo.InvariantCulture);
        selectedDate = today;
        entryTime = clock.Time;
        await LoadTrackerDataAsync(cancellationToken);
        if (!isNotificationsStarted)
        {
            await NotificationEventSource.StartAsync(cancellationToken);
            isNotificationsStarted = true;
        }
    }

    private async Task SelectTabAsync(CaloriesTab tab)
    {
        activeTab = tab;
        showTabletEntryForm = false;
        ClearStatus();
        if (tab == CaloriesTab.Statistics)
        {
            await LoadHistoryAsync();
        }
        else if (tab == CaloriesTab.Foods)
        {
            await LoadFoodsAsync();
        }
    }

    private async Task OnSelectedDateChangedAsync(DateOnly date)
    {
        selectedDate = date > today ? today : date;
        CancelEntryEdit();
        CancelEntryDelete();
        ClearStatus();
        await LoadSelectedWeekAsync();
    }

    private void OnEntryTimeChanged(ChangeEventArgs args)
    {
        if (args.Value is string value && TimeOnly.TryParseExact(
                value,
                "HH:mm",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out _))
        {
            entryTime = value;
        }
    }

    private void OnEntryEditTimeChanged(ChangeEventArgs args)
    {
        if (args.Value is string value &&
            TimeOnly.TryParseExact(
                value,
                "HH:mm",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out _))
        {
            entryEditDraft.Time = value;
        }
    }

    private async Task GoToPreviousDayAsync()
    {
        if (selectedDate > DateOnly.MinValue)
        {
            selectedDate = selectedDate.AddDays(-1);
            CancelEntryEdit();
            CancelEntryDelete();
            await LoadSelectedWeekAsync();
        }
    }

    private async Task GoToNextDayAsync()
    {
        if (selectedDate < today)
        {
            selectedDate = selectedDate.AddDays(1);
            CancelEntryEdit();
            CancelEntryDelete();
            await LoadSelectedWeekAsync();
        }
    }

    private async Task GoToTodayAsync()
    {
        selectedDate = today;
        CancelEntryEdit();
        CancelEntryDelete();
        await LoadSelectedWeekAsync();
    }

    private async Task<bool> LoadSelectedWeekAsync()
    {
        try
        {
            var entries = await CaloriesService.GetEntriesAsync(WeekStart, WeekEnd, lifetimeCancellation.Token);
            SetWeekEntries(entries);
            return true;
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            Logger.LogError(exception, "Could not load Calories entries for {Date}.", selectedDate);
            SetStatus("Entries for this week could not be loaded.", isError: true);
            return false;
        }
    }

    private void SetWeekEntries(List<CaloriesLogEntry> entries)
    {
        weekEntries = entries;
        dayEntries = entries
            .Where(entry => entry.LocalDate == selectedDate)
            .OrderBy(entry => entry.LoggedAt.UtcDateTime)
            .ThenBy(entry => entry.Id)
            .ToList();
    }

    private async Task SaveGoalAsync()
    {
        if (goalDraft.Calories is not int calories)
        {
            return;
        }

        isSavingGoal = true;
        try
        {
            dailyGoal = await CaloriesService.SetDailyGoalAsync(calories, lifetimeCancellation.Token);
            SetStatus("Daily calorie goal saved.");
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            Logger.LogError(exception, "Could not save the daily calorie goal.");
            SetStatus("The daily goal could not be saved.", isError: true);
        }
        finally
        {
            isSavingGoal = false;
        }
    }

    private async Task AddSavedEntryAsync()
    {
        var selectedFood = foods.FirstOrDefault(food => food.Id == savedEntryDraft.FoodId);
        if (selectedFood is null || savedEntryDraft.Servings is not decimal servings)
        {
            return;
        }

        try
        {
            var entry = new CaloriesLogEntry(
                0,
                selectedFood.Name,
                ScaleCalories(selectedFood.CaloriesPerServing, servings),
                ScaleMacro(selectedFood.CarbohydratesGrams, servings),
                ScaleMacro(selectedFood.ProteinGrams, servings),
                ScaleMacro(selectedFood.FatGrams, servings),
                servings,
                selectedDate,
                await GetLoggedAtAsync(),
                NormalizeNote(entryNotes));
            if (await SaveEntryAsync(entry))
            {
                savedEntryDraft = new CaloriesEntryDraft();
            }
        }
        catch (OverflowException exception)
        {
            Logger.LogWarning(exception, "The serving multiplier is too large to calculate nutrition.");
            SetStatus("That serving multiplier is too large to calculate. Use a smaller value.", isError: true);
        }
        catch (ArgumentOutOfRangeException exception)
        {
            Logger.LogWarning(exception, "The serving multiplier rounds the food's calories below one kcal.");
            SetStatus("That serving multiplier results in less than 1 kcal. Use a larger value.", isError: true);
        }
        catch (OperationCanceledException) when (lifetimeCancellation.IsCancellationRequested)
        {
        }
        catch (OperationCanceledException exception)
        {
            HandleInterruptedEntrySubmission(exception, "preparing a saved-food entry");
        }
    }

    private async Task AddCustomEntryAsync()
    {
        if (customEntryDraft.Name is null || customEntryDraft.Calories is not int calories)
        {
            return;
        }

        try
        {
            var entry = new CaloriesLogEntry(
                0,
                customEntryDraft.Name.Trim(),
                calories,
                customEntryDraft.CarbohydratesGrams,
                customEntryDraft.ProteinGrams,
                customEntryDraft.FatGrams,
                null,
                selectedDate,
                await GetLoggedAtAsync(),
                NormalizeNote(entryNotes));
            if (await SaveEntryAsync(entry))
            {
                customEntryDraft = new CustomCaloriesEntryDraft();
            }
        }
        catch (OperationCanceledException) when (lifetimeCancellation.IsCancellationRequested)
        {
        }
        catch (OperationCanceledException exception)
        {
            HandleInterruptedEntrySubmission(exception, "preparing a custom-food entry");
        }
    }

    private async Task<bool> SaveEntryAsync(CaloriesLogEntry entry)
    {
        isSavingEntry = true;
        try
        {
            await CaloriesService.AddEntryAsync(entry, lifetimeCancellation.Token);
            var entriesLoaded = await LoadSelectedWeekAsync();
            showTabletEntryForm = false;
            entryNotes = null;
            if (entriesLoaded)
            {
                SetStatus("Food added to the daily log.");
            }

            return true;
        }
        catch (OperationCanceledException exception)
        {
            if (!lifetimeCancellation.IsCancellationRequested)
            {
                Logger.LogWarning(exception, "The connection was interrupted while saving a Calories entry.");
                SetStatus(
                    "The connection was interrupted before the save could be confirmed. Check the daily log before retrying to avoid duplicates.",
                    isError: true);
            }

            return false;
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            Logger.LogError(exception, "Could not add food to the Calories log.");
            SetStatus("The food could not be added to the log.", isError: true);
            return false;
        }
        finally
        {
            isSavingEntry = false;
        }
    }

    private Task<DateTimeOffset> GetLoggedAtAsync() => GetLoggedAtAsync(entryTime);

    private void HandleInterruptedEntrySubmission(OperationCanceledException exception, string operation)
    {
        Logger.LogWarning(exception, "The connection was interrupted while {Operation}.", operation);
        SetStatus(
            "The connection was interrupted before the save was attempted. Check the daily log, then retry.",
            isError: true);
    }

    private async Task<DateTimeOffset> GetLoggedAtAsync(string timeValue)
    {
        caloriesJsModule ??= await JS.InvokeAsync<IJSObjectReference>(
            "import",
            lifetimeCancellation.Token,
            "./Pages/Calories/Calories.razor.js");
        var offsetMinutes = await caloriesJsModule.InvokeAsync<int>(
            "getOffsetMinutes",
            lifetimeCancellation.Token,
            selectedDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            timeValue);
        var localDateTime = DateTime.SpecifyKind(
            selectedDate.ToDateTime(TimeOnly.ParseExact(timeValue, "HH:mm", CultureInfo.InvariantCulture)),
            DateTimeKind.Unspecified);
        return new DateTimeOffset(localDateTime, TimeSpan.FromMinutes(offsetMinutes));
    }

    private void EditLoggedEntry(CaloriesLogEntry entry)
    {
        editingEntryId = entry.Id;
        entryPendingDeleteId = null;
        entryEditDraft = new CaloriesLoggedEntryDraft
        {
            Name = entry.Name,
            Calories = entry.Calories,
            CarbohydratesGrams = entry.CarbohydratesGrams,
            ProteinGrams = entry.ProteinGrams,
            FatGrams = entry.FatGrams,
            Servings = entry.Servings,
            Time = entry.LoggedAt.ToString("HH:mm", CultureInfo.InvariantCulture),
            Notes = entry.Notes
        };
        entryEditServingsBaseline = entry.Servings;
        ClearStatus();
    }

    private void ScaleEditedEntryForServingsChange()
    {
        if (entryEditServingsBaseline is not decimal previousServings ||
            entryEditDraft.Servings is not decimal nextServings ||
            previousServings <= 0 ||
            nextServings <= 0)
        {
            return;
        }

        if (nextServings != decimal.Round(nextServings, 2))
        {
            entryEditDraft.Servings = previousServings;
            SetStatus("Serving multipliers can use up to two decimal places.", isError: true);
            return;
        }

        try
        {
            var factor = nextServings / previousServings;
            entryEditDraft.Calories = ScaleCalories(entryEditDraft.Calories ?? 0, factor);
            entryEditDraft.CarbohydratesGrams = ScaleMacro(entryEditDraft.CarbohydratesGrams, factor);
            entryEditDraft.ProteinGrams = ScaleMacro(entryEditDraft.ProteinGrams, factor);
            entryEditDraft.FatGrams = ScaleMacro(entryEditDraft.FatGrams, factor);
            entryEditServingsBaseline = nextServings;
        }
        catch (OverflowException exception)
        {
            entryEditDraft.Servings = previousServings;
            Logger.LogWarning(exception, "The edited serving multiplier is too large to calculate nutrition.");
            SetStatus("That serving multiplier is too large to calculate. Use a smaller value.", isError: true);
        }
        catch (ArgumentOutOfRangeException exception)
        {
            entryEditDraft.Servings = previousServings;
            Logger.LogWarning(exception, "The edited serving multiplier rounds the food's calories below one kcal.");
            SetStatus("That serving multiplier results in less than 1 kcal. Use a larger value.", isError: true);
        }
    }

    private void CancelEntryEdit()
    {
        editingEntryId = null;
        entryEditServingsBaseline = null;
        entryEditDraft = new CaloriesLoggedEntryDraft();
    }

    private async Task SaveEntryEditAsync()
    {
        if (editingEntryId is not int entryId ||
            entryEditDraft.Name is null ||
            entryEditDraft.Calories is not int calories ||
            entryEditDraft.Time is not string time)
        {
            return;
        }

        isSavingEntryEdit = true;
        try
        {
            var originalEntry = dayEntries.FirstOrDefault(entry => entry.Id == entryId);
            if (originalEntry is null)
            {
                CancelEntryEdit();
                SetStatus("This log entry is no longer available. Refresh the daily log.", isError: true);
                return;
            }

            var updatedEntry = originalEntry with
            {
                Name = entryEditDraft.Name.Trim(),
                Calories = calories,
                CarbohydratesGrams = entryEditDraft.CarbohydratesGrams,
                ProteinGrams = entryEditDraft.ProteinGrams,
                FatGrams = entryEditDraft.FatGrams,
                Servings = entryEditDraft.Servings,
                LocalDate = selectedDate,
                LoggedAt = await GetLoggedAtAsync(time),
                Notes = NormalizeNote(entryEditDraft.Notes)
            };

            if (!await CaloriesService.UpdateEntryAsync(updatedEntry, lifetimeCancellation.Token))
            {
                CancelEntryEdit();
                SetStatus("This log entry could not be found. Refresh the daily log.", isError: true);
                return;
            }

            CancelEntryEdit();
            if (await LoadSelectedWeekAsync())
            {
                SetStatus("Daily log entry updated.");
            }
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            Logger.LogError(exception, "Could not update Calories log entry {EntryId}.", entryId);
            SetStatus("The log entry could not be updated.", isError: true);
        }
        finally
        {
            isSavingEntryEdit = false;
        }
    }

    private void BeginEntryDelete(int id)
    {
        entryPendingDeleteId = id;
        CancelEntryEdit();
    }

    private void CancelEntryDelete() => entryPendingDeleteId = null;

    private async Task RemoveLoggedEntryAsync(int id)
    {
        isDeletingEntry = true;
        try
        {
            if (!await CaloriesService.RemoveEntryAsync(id, lifetimeCancellation.Token))
            {
                entryPendingDeleteId = null;
                SetStatus("This log entry could not be found. Refresh the daily log.", isError: true);
                return;
            }

            entryPendingDeleteId = null;
            if (await LoadSelectedWeekAsync())
            {
                SetStatus("Daily log entry deleted.");
            }
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            Logger.LogError(exception, "Could not delete Calories log entry {EntryId}.", id);
            SetStatus("The log entry could not be deleted.", isError: true);
        }
        finally
        {
            isDeletingEntry = false;
        }
    }

    private async Task SaveFoodAsync()
    {
        if (foodDraft.Name is null || foodDraft.CaloriesPerServing is not int calories)
        {
            return;
        }

        isSavingFood = true;
        try
        {
            var food = new CaloriesFood(
                editingFoodId ?? 0,
                foodDraft.Name.Trim(),
                calories,
                foodDraft.CarbohydratesGrams,
                foodDraft.ProteinGrams,
                foodDraft.FatGrams);
            var isNewFood = editingFoodId is null;
            var succeeded = editingFoodId is int id
                ? await CaloriesService.UpdateFoodAsync(
                    food with { Id = id },
                    lifetimeCancellation.Token)
                : await AddFoodAsync(food);

            if (!succeeded)
            {
                SetStatus("The saved food could not be found. Refresh and try again.", isError: true);
                return;
            }

            foodDraft = new CaloriesFoodDraft();
            editingFoodId = null;
            if (await LoadFoodsAsync())
            {
                SetStatus(isNewFood ? "Saved food added." : "Saved food updated.");
            }
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            Logger.LogError(exception, "Could not save a reusable Calories food.");
            SetStatus("The saved food could not be saved.", isError: true);
        }
        finally
        {
            isSavingFood = false;
        }
    }

    private async Task<bool> AddFoodAsync(CaloriesFood food)
    {
        await CaloriesService.AddFoodAsync(food, lifetimeCancellation.Token);
        return true;
    }

    private void EditFood(CaloriesFood food)
    {
        editingFoodId = food.Id;
        foodDraft = new CaloriesFoodDraft
        {
            Name = food.Name,
            CaloriesPerServing = food.CaloriesPerServing,
            CarbohydratesGrams = food.CarbohydratesGrams,
            ProteinGrams = food.ProteinGrams,
            FatGrams = food.FatGrams
        };
        foodPendingDeleteId = null;
        ClearStatus();
    }

    private void CancelFoodEdit()
    {
        editingFoodId = null;
        foodDraft = new CaloriesFoodDraft();
    }

    private void BeginFoodDelete(int id)
    {
        foodPendingDeleteId = id;
        editingFoodId = null;
    }

    private void CancelFoodDelete() => foodPendingDeleteId = null;

    private async Task RemoveFoodAsync(int id)
    {
        try
        {
            var removed = await CaloriesService.RemoveFoodAsync(id, lifetimeCancellation.Token);
            if (!removed)
            {
                SetStatus("The saved food could not be found. Refresh and try again.", isError: true);
                return;
            }

            foodPendingDeleteId = null;
            await LoadFoodsAsync();
            SetStatus("Saved food deleted. Existing history is unchanged.");
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            Logger.LogError(exception, "Could not delete saved food {FoodId}.", id);
            SetStatus("The saved food could not be deleted.", isError: true);
        }
    }

    private async Task<bool> LoadFoodsAsync()
    {
        try
        {
            foods = await CaloriesService.GetFoodsAsync(lifetimeCancellation.Token);
            return true;
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            Logger.LogError(exception, "Could not load reusable Calories foods.");
            SetStatus("Saved foods could not be loaded.", isError: true);
            return false;
        }
    }

    private async Task ChangeHistoryPeriodAsync(HistoryPeriod period)
    {
        historyPeriod = period;
        await LoadHistoryAsync();
    }

    private async Task LoadHistoryAsync()
    {
        isLoadingHistory = true;
        historyError = null;
        selectedChartPointIndex = null;
        try
        {
            (historyFrom, historyThrough) = GetHistoryRange(historyPeriod, today);
            var entries = await CaloriesService.GetEntriesAsync(
                historyFrom,
                historyThrough,
                lifetimeCancellation.Token);
            historyPoints = CreateHistoryPoints(entries, historyPeriod, historyFrom, historyThrough);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            Logger.LogError(exception, "Could not load {Period} Calories history.", historyPeriod);
            historyError = "Calories history could not be loaded. Check the connection and try again.";
        }
        finally
        {
            isLoadingHistory = false;
        }
    }

    private async Task HandleNotificationReceivedAsync(string notificationType)
    {
        if (notificationType != RealtimeNotificationTypes.CaloriesChanged || isDisposed || today == default)
        {
            return;
        }

        await InvokeAsync(async () =>
        {
            try
            {
                await LoadTrackerDataAsync(lifetimeCancellation.Token);
                if (activeTab == CaloriesTab.Statistics)
                {
                    await LoadHistoryAsync();
                }
                else if (activeTab == CaloriesTab.Foods)
                {
                    await LoadFoodsAsync();
                }
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                Logger.LogError(exception, "Could not refresh Calories data after a realtime update.");
                SetStatus("New tracker data was detected, but refreshing it failed.", isError: true);
            }

            StateHasChanged();
        });
    }

    private int GetBarHeightPercent(long calories) =>
        (int)Math.Clamp(Math.Round(calories * 100d / ChartAxisMaximum), 1, 100);

    private long GetChartTickValue(int tickIndex) => ChartAxisMaximum * tickIndex / 4;

    private void SelectChartPoint(int index) => selectedChartPointIndex = index;

    private void OpenTabletEntryForm()
    {
        showTabletEntryForm = true;
        ClearStatus();
    }

    private void CloseTabletEntryForm() => showTabletEntryForm = false;

    private static List<CaloriesChartPoint> CreateHistoryPoints(
        IEnumerable<CaloriesLogEntry> entries,
        HistoryPeriod period,
        DateOnly from,
        DateOnly through)
    {
        var caloriesByDay = entries
            .GroupBy(entry => entry.LocalDate)
            .ToDictionary(group => group.Key, group => group.Sum(entry => (long)entry.Calories));

        if (period == HistoryPeriod.Daily)
        {
            var points = new List<CaloriesChartPoint>();
            for (var date = from; date <= through; date = date.AddDays(1))
            {
                points.Add(new CaloriesChartPoint(
                    date,
                    date.ToString("d MMM", CultureInfo.CurrentCulture),
                    caloriesByDay.GetValueOrDefault(date)));
            }

            return points;
        }

        if (period == HistoryPeriod.Weekly)
        {
            var points = new List<CaloriesChartPoint>();
            for (var weekStart = from; weekStart <= through; weekStart = weekStart.AddDays(7))
            {
                var weekEnd = weekStart.AddDays(6);
                var calories = caloriesByDay
                    .Where(pair => pair.Key >= weekStart && pair.Key <= weekEnd)
                    .Sum(pair => pair.Value);
                points.Add(new CaloriesChartPoint(
                    weekStart,
                    weekStart.ToString("d MMM", CultureInfo.CurrentCulture),
                    calories));
            }

            return points;
        }

        var monthPoints = new List<CaloriesChartPoint>();
        for (var monthStart = from; monthStart <= through; monthStart = monthStart.AddMonths(1))
        {
            var monthEnd = monthStart.AddMonths(1).AddDays(-1);
            var calories = caloriesByDay
                .Where(pair => pair.Key >= monthStart && pair.Key <= monthEnd)
                .Sum(pair => pair.Value);
            monthPoints.Add(new CaloriesChartPoint(
                monthStart,
                monthStart.ToString("MMM yy", CultureInfo.CurrentCulture),
                calories));
        }

        return monthPoints;
    }

    private static (DateOnly From, DateOnly Through) GetHistoryRange(HistoryPeriod period, DateOnly currentDate)
    {
        if (period == HistoryPeriod.Daily)
        {
            return (currentDate.AddDays(-6), currentDate);
        }

        if (period == HistoryPeriod.Weekly)
        {
            var thisWeek = StartOfWeek(currentDate);
            return (thisWeek.AddDays(-77), thisWeek.AddDays(6));
        }

        var currentMonth = new DateOnly(currentDate.Year, currentDate.Month, 1);
        return (currentMonth.AddMonths(-11), currentMonth.AddMonths(1).AddDays(-1));
    }

    private static DateOnly StartOfWeek(DateOnly date) =>
        date.AddDays(-(((int)date.DayOfWeek + 6) % 7));

    private static int? SumMacro(
        IEnumerable<CaloriesLogEntry> entries,
        Func<CaloriesLogEntry, int?> selector)
    {
        var knownValues = entries
            .Select(selector)
            .Where(value => value.HasValue)
            .Select(value => value!.Value)
            .ToList();
        return knownValues.Count == 0 ? null : knownValues.Sum();
    }

    private static int ScaleCalories(int calories, decimal multiplier)
    {
        var scaledCalories = ScaleNutritionValue(calories, multiplier);
        return scaledCalories > 0
            ? scaledCalories
            : throw new ArgumentOutOfRangeException(nameof(multiplier));
    }

    private static int? ScaleMacro(int? grams, decimal multiplier) =>
        grams is int value ? ScaleNutritionValue(value, multiplier) : null;

    private static int ScaleNutritionValue(int value, decimal multiplier) =>
        checked(decimal.ToInt32(decimal.Round(
            value * multiplier,
            decimals: 0,
            MidpointRounding.AwayFromZero)));

    private static string FormatMacro(int? value) =>
        value is int grams ? $"{grams.ToString(CultureInfo.CurrentCulture)} g" : "—";

    private static string? NormalizeNote(string? note) =>
        string.IsNullOrWhiteSpace(note) ? null : note.Trim();

    private static string FormatEntryMacros(CaloriesLogEntry entry)
    {
        var macros = new List<string>();
        if (entry.CarbohydratesGrams is int carbs)
        {
            macros.Add($"Carbs {carbs} g");
        }

        if (entry.ProteinGrams is int protein)
        {
            macros.Add($"Protein {protein} g");
        }

        if (entry.FatGrams is int fat)
        {
            macros.Add($"Fat {fat} g");
        }

        return macros.Count == 0 ? "Macros not recorded" : string.Join(" · ", macros);
    }

    private void SetStatus(string message, bool isError = false)
    {
        statusMessage = message;
        statusIsError = isError;
    }

    private void ClearStatus()
    {
        statusMessage = null;
        statusIsError = false;
    }

    public async ValueTask DisposeAsync()
    {
        isDisposed = true;
        lifetimeCancellation.Cancel();
        NotificationEventSource.NotificationReceived -= HandleNotificationReceivedAsync;
        if (isNotificationsStarted)
        {
            await NotificationEventSource.StopAsync();
        }

        if (caloriesJsModule is not null)
        {
            await caloriesJsModule.DisposeAsync();
        }

        lifetimeCancellation.Dispose();
    }

    private enum CaloriesTab
    {
        Daily,
        Foods,
        Statistics
    }

    private enum EntryMode
    {
        SavedFood,
        CustomFood
    }

    private enum HistoryPeriod
    {
        Daily,
        Weekly,
        Monthly
    }

    private sealed record BrowserClock(string Date, string Time);

    private sealed record CaloriesChartPoint(DateOnly Start, string Label, long Calories);
}
