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
using HomeServerPage.Data.Exercise;
using HomeServerPage.Data.Notifications;

namespace HomeServerPage.Client.Pages.Calories;

public partial class Calories
{
    private readonly CancellationTokenSource lifetimeCancellation = new();
    private List<CaloriesFood> foods = [];
    private List<CaloriesLogEntry> weekEntries = [];
    private List<CaloriesLogEntry> dayEntries = [];
    private List<ExerciseEntry> weekExercises = [];
    private List<ExerciseEntry> dayExercises = [];
    private List<DayLogRow> dayLogRows = [];
    private ExerciseDraft exerciseDraft = new();
    private int? exercisePendingDeleteId;
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

    private DateOnly WeekStart => StartOfWeek(this.selectedDate);
    private DateOnly WeekEnd => WeekStart.AddDays(6);
    private long DailyCalories => this.dayEntries.Sum(entry => (long)entry.Calories);
    private long DailyBurned => this.dayExercises.Sum(exercise => (long)exercise.CaloriesBurned);
    private long NetDailyCalories => DailyCalories - DailyBurned;
    private long WeeklyBurned => this.weekExercises.Sum(exercise => (long)exercise.CaloriesBurned);
    private int BurnedProgressPercent => this.dailyGoal is > 0
        ? (int)Math.Clamp(Math.Round(DailyBurned * 100d / this.dailyGoal.Value), 0, 100)
        : 0;
    private int OverGoalProgressPercent => this.dailyGoal is int goal && this.DailyCalories > goal
        ? (int)Math.Clamp(Math.Ceiling((this.DailyCalories - goal) * 100d / goal), 1, 100)
        : 0;
    private long WeeklyCalories => this.weekEntries.Sum(entry => (long)entry.Calories);
    private int? DailyCarbohydrates => SumMacro(this.dayEntries, entry => entry.CarbohydratesGrams);
    private int? DailyProtein => SumMacro(this.dayEntries, entry => entry.ProteinGrams);
    private int? DailyFat => SumMacro(this.dayEntries, entry => entry.FatGrams);
    private int? WeeklyCarbohydrates => SumMacro(this.weekEntries, entry => entry.CarbohydratesGrams);
    private int? WeeklyProtein => SumMacro(this.weekEntries, entry => entry.ProteinGrams);
    private int? WeeklyFat => SumMacro(this.weekEntries, entry => entry.FatGrams);
    private int GoalProgressPercent => this.dailyGoal is > 0
        ? (int)Math.Clamp(Math.Round(DailyCalories * 100d / this.dailyGoal.Value), 0, 100)
        : 0;
    private long HistoryTotal => this.historyPoints.Sum(point => point.Calories);
    private long HistoryAverage => this.historyPoints.Count == 0
        ? 0
        : (long)Math.Round(this.historyPoints.Average(point => point.Calories));
    private int ChartColumnWidth => this.historyPeriod == HistoryPeriod.Daily ? 42 : 62;
    private long ChartAxisMaximum => Math.Max(
        500,
        ((this.historyPoints.Count == 0 ? 0 : this.historyPoints.Max(point => point.Calories)) + 499) / 500 * 500);
    private CaloriesChartPoint? SelectedChartPoint =>
        this.selectedChartPointIndex is int index && index >= 0 && index < this.historyPoints.Count
            ? this.historyPoints[index]
            : null;
    private string HistoryPeriodLabel => this.historyPeriod switch
    {
        HistoryPeriod.Daily => "Day",
        HistoryPeriod.Weekly => "Week",
        _ => "Month"
    };
    private string HistoryRangeText => this.historyPeriod switch
    {
        HistoryPeriod.Daily => $"Last 7 days · {this.historyFrom.ToString("d MMM", CultureInfo.CurrentCulture)}–{this.historyThrough.ToString("d MMM yyyy", CultureInfo.CurrentCulture)}",
        HistoryPeriod.Weekly => $"Last 12 calendar weeks · {this.historyFrom.ToString("d MMM yyyy", CultureInfo.CurrentCulture)}–{this.historyThrough.ToString("d MMM yyyy", CultureInfo.CurrentCulture)}",
        _ => $"Last 12 calendar months · {this.historyFrom.ToString("MMM yyyy", CultureInfo.CurrentCulture)}–{this.historyThrough.ToString("MMM yyyy", CultureInfo.CurrentCulture)}"
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
            await InitializeTrackerAsync(this.lifetimeCancellation.Token);
        }
        catch (OperationCanceledException) when (this.lifetimeCancellation.IsCancellationRequested)
        {
        }
        catch (Exception exception)
        {
            Logger.LogError(exception, "Could not initialize the Calories tracker.");
            this.loadError = "The Calories tracker could not be loaded. Check the connection and try again.";
        }
        finally
        {
            this.isLoading = false;
            if (!this.isDisposed)
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
        var exercisesTask = ExerciseService.GetEntriesAsync(WeekStart, WeekEnd, cancellationToken);
        await Task.WhenAll(foodsTask, goalTask, entriesTask, exercisesTask);

        this.foods = await foodsTask;
        this.dailyGoal = await goalTask;
        this.goalDraft.Calories = this.dailyGoal;
        SetWeekEntries(await entriesTask, await exercisesTask);
    }

    private async Task RetryLoadingAsync()
    {
        this.loadError = null;
        this.isLoading = true;
        await InvokeAsync(StateHasChanged);
        try
        {
            await InitializeTrackerAsync(this.lifetimeCancellation.Token);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            Logger.LogError(exception, "Could not reload the Calories tracker.");
            this.loadError = "The Calories tracker could not be loaded. Check the connection and try again.";
        }
        finally
        {
            this.isLoading = false;
        }
    }

    private async Task InitializeTrackerAsync(CancellationToken cancellationToken)
    {
        this.caloriesJsModule ??= await JS.InvokeAsync<IJSObjectReference>(
            "import",
            cancellationToken,
            "./Pages/Calories/Calories.razor.js");
        var clock = await this.caloriesJsModule.InvokeAsync<BrowserClock>(
            "getLocalClock",
            cancellationToken);
        this.today = DateOnly.ParseExact(clock.Date, "yyyy-MM-dd", CultureInfo.InvariantCulture);
        this.selectedDate = this.today;
        this.entryTime = clock.Time;
        await LoadTrackerDataAsync(cancellationToken);
        if (!this.isNotificationsStarted)
        {
            await NotificationEventSource.StartAsync(cancellationToken);
            this.isNotificationsStarted = true;
        }
    }

    private async Task SelectTabAsync(CaloriesTab tab)
    {
        this.activeTab = tab;
        this.showTabletEntryForm = false;
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
        this.selectedDate = date > this.today ? this.today : date;
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
            this.entryTime = value;
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
            this.entryEditDraft.Time = value;
        }
    }

    private async Task GoToPreviousDayAsync()
    {
        if (this.selectedDate > DateOnly.MinValue)
        {
            this.selectedDate = this.selectedDate.AddDays(-1);
            CancelEntryEdit();
            CancelEntryDelete();
            await LoadSelectedWeekAsync();
        }
    }

    private async Task GoToNextDayAsync()
    {
        if (this.selectedDate < this.today)
        {
            this.selectedDate = this.selectedDate.AddDays(1);
            CancelEntryEdit();
            CancelEntryDelete();
            await LoadSelectedWeekAsync();
        }
    }

    private async Task GoToTodayAsync()
    {
        this.selectedDate = this.today;
        CancelEntryEdit();
        CancelEntryDelete();
        await LoadSelectedWeekAsync();
    }

    private async Task<bool> LoadSelectedWeekAsync()
    {
        try
        {
            var entriesTask = CaloriesService.GetEntriesAsync(WeekStart, WeekEnd, this.lifetimeCancellation.Token);
            var exercisesTask = ExerciseService.GetEntriesAsync(WeekStart, WeekEnd, this.lifetimeCancellation.Token);
            await Task.WhenAll(entriesTask, exercisesTask);
            SetWeekEntries(await entriesTask, await exercisesTask);
            return true;
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            Logger.LogError(exception, "Could not load Calories entries for {Date}.", this.selectedDate);
            SetStatus("Entries for this week could not be loaded.", isError: true);
            return false;
        }
    }

    private void SetWeekEntries(List<CaloriesLogEntry> entries, List<ExerciseEntry>? exercises = null)
    {
        this.weekEntries = entries;
        if (exercises is not null)
        {
            this.weekExercises = exercises;
        }

        this.dayEntries = entries
            .Where(entry => entry.LocalDate == this.selectedDate)
            .OrderBy(entry => entry.LoggedAt.UtcDateTime)
            .ThenBy(entry => entry.Id)
            .ToList();
        this.dayExercises = this.weekExercises
            .Where(exercise => exercise.LocalDate == this.selectedDate)
            .OrderBy(exercise => exercise.LoggedAt.UtcDateTime)
            .ThenBy(exercise => exercise.Id)
            .ToList();
        this.dayLogRows = this.dayEntries
            .Select(entry => new DayLogRow(entry.LoggedAt, 0, entry, null))
            .Concat(this.dayExercises.Select(exercise => new DayLogRow(exercise.LoggedAt, 1, null, exercise)))
            .OrderBy(row => row.LoggedAt.UtcDateTime)
            .ThenBy(row => row.Kind)
            .ThenBy(row => row.Food?.Id ?? row.Exercise!.Id)
            .ToList();
    }

    private void OnExerciseTypeChanged(ExerciseType type)
    {
        this.exerciseDraft.Type = type;
        if (!ExerciseCalorieCalculator.RequiresDistance(type))
        {
            this.exerciseDraft.DistanceKm = null;
        }
    }

    private int? ExercisePreviewCalories =>
        ExerciseCalorieCalculator.IsValid(this.exerciseDraft.Type, this.exerciseDraft.DistanceKm)
            ? ExerciseCalorieCalculator.Calculate(this.exerciseDraft.Type, this.exerciseDraft.DistanceKm)
            : null;

    private async Task AddExerciseAsync()
    {
        if (ExercisePreviewCalories is not int calories)
        {
            return;
        }

        this.isSavingEntry = true;
        try
        {
            var loggedAt = await GetLoggedAtAsync();
            var entry = new ExerciseEntry(
                0,
                this.exerciseDraft.Type,
                this.exerciseDraft.DistanceKm,
                calories,
                this.selectedDate,
                loggedAt);
            await ExerciseService.AddEntryAsync(entry, this.lifetimeCancellation.Token);
            this.exerciseDraft = new ExerciseDraft();
            this.showTabletEntryForm = false;
            if (await LoadSelectedWeekAsync())
            {
                SetStatus("Exercise added to the daily log.");
            }
        }
        catch (OperationCanceledException exception)
        {
            if (!this.lifetimeCancellation.IsCancellationRequested)
            {
                Logger.LogWarning(exception, "The connection was interrupted while saving an exercise entry.");
                SetStatus(
                    "The connection was interrupted before the save could be confirmed. Check the daily log before retrying to avoid duplicates.",
                    isError: true);
            }
        }
        catch (Exception exception)
        {
            Logger.LogError(exception, "Could not add exercise to the Calories log.");
            SetStatus("The exercise could not be added to the log.", isError: true);
        }
        finally
        {
            this.isSavingEntry = false;
        }
    }

    private async Task RemoveExerciseAsync(int id)
    {
        this.isDeletingEntry = true;
        try
        {
            if (!await ExerciseService.RemoveEntryAsync(id, this.lifetimeCancellation.Token))
            {
                this.exercisePendingDeleteId = null;
                SetStatus("This exercise could not be found. Refresh the daily log.", isError: true);
                return;
            }

            this.exercisePendingDeleteId = null;
            if (await LoadSelectedWeekAsync())
            {
                SetStatus("Exercise deleted.");
            }
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            Logger.LogError(exception, "Could not delete exercise entry {EntryId}.", id);
            SetStatus("The exercise could not be deleted.", isError: true);
        }
        finally
        {
            this.isDeletingEntry = false;
        }
    }

    private static string FormatExercise(ExerciseEntry exercise) =>
        exercise.DistanceKm is decimal km
            ? $"{ExerciseCalorieCalculator.GetDisplayName(exercise.Type)} · {km.ToString("0.##", CultureInfo.CurrentCulture)} km"
            : ExerciseCalorieCalculator.GetDisplayName(exercise.Type);


    private async Task SaveGoalAsync()
    {
        if (this.goalDraft.Calories is not int calories)
        {
            return;
        }

        this.isSavingGoal = true;
        try
        {
            this.dailyGoal = await CaloriesService.SetDailyGoalAsync(calories, this.lifetimeCancellation.Token);
            SetStatus("Daily calorie goal saved.");
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            Logger.LogError(exception, "Could not save the daily calorie goal.");
            SetStatus("The daily goal could not be saved.", isError: true);
        }
        finally
        {
            this.isSavingGoal = false;
        }
    }

    private async Task AddSavedEntryAsync()
    {
        var selectedFood = this.foods.FirstOrDefault(food => food.Id == this.savedEntryDraft.FoodId);
        if (selectedFood is null || this.savedEntryDraft.Servings is not decimal servings)
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
                this.selectedDate,
                await GetLoggedAtAsync(),
                NormalizeNote(this.entryNotes));
            if (await SaveEntryAsync(entry))
            {
                this.savedEntryDraft = new CaloriesEntryDraft();
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
        catch (OperationCanceledException) when (this.lifetimeCancellation.IsCancellationRequested)
        {
        }
        catch (OperationCanceledException exception)
        {
            HandleInterruptedEntrySubmission(exception, "preparing a saved-food entry");
        }
    }

    private async Task AddCustomEntryAsync()
    {
        if (this.customEntryDraft.Name is null || this.customEntryDraft.Calories is not int calories)
        {
            return;
        }

        try
        {
            var entry = new CaloriesLogEntry(
                0,
                this.customEntryDraft.Name.Trim(),
                calories,
                this.customEntryDraft.CarbohydratesGrams,
                this.customEntryDraft.ProteinGrams,
                this.customEntryDraft.FatGrams,
                null,
                this.selectedDate,
                await GetLoggedAtAsync(),
                NormalizeNote(this.entryNotes));
            if (await SaveEntryAsync(entry))
            {
                this.customEntryDraft = new CustomCaloriesEntryDraft();
            }
        }
        catch (OperationCanceledException) when (this.lifetimeCancellation.IsCancellationRequested)
        {
        }
        catch (OperationCanceledException exception)
        {
            HandleInterruptedEntrySubmission(exception, "preparing a custom-food entry");
        }
    }

    private async Task<bool> SaveEntryAsync(CaloriesLogEntry entry)
    {
        this.isSavingEntry = true;
        try
        {
            await CaloriesService.AddEntryAsync(entry, this.lifetimeCancellation.Token);
            var entriesLoaded = await LoadSelectedWeekAsync();
            this.showTabletEntryForm = false;
            this.entryNotes = null;
            if (entriesLoaded)
            {
                SetStatus("Food added to the daily log.");
            }

            return true;
        }
        catch (OperationCanceledException exception)
        {
            if (!this.lifetimeCancellation.IsCancellationRequested)
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
            this.isSavingEntry = false;
        }
    }

    private Task<DateTimeOffset> GetLoggedAtAsync() => GetLoggedAtAsync(this.entryTime);

    private void HandleInterruptedEntrySubmission(OperationCanceledException exception, string operation)
    {
        Logger.LogWarning(exception, "The connection was interrupted while {Operation}.", operation);
        SetStatus(
            "The connection was interrupted before the save was attempted. Check the daily log, then retry.",
            isError: true);
    }

    private async Task<DateTimeOffset> GetLoggedAtAsync(string timeValue)
    {
        this.caloriesJsModule ??= await JS.InvokeAsync<IJSObjectReference>(
            "import",
            this.lifetimeCancellation.Token,
            "./Pages/Calories/Calories.razor.js");
        var offsetMinutes = await this.caloriesJsModule.InvokeAsync<int>(
            "getOffsetMinutes",
            this.lifetimeCancellation.Token,
            this.selectedDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            timeValue);
        var localDateTime = DateTime.SpecifyKind(
            this.selectedDate.ToDateTime(TimeOnly.ParseExact(timeValue, "HH:mm", CultureInfo.InvariantCulture)),
            DateTimeKind.Unspecified);
        return new DateTimeOffset(localDateTime, TimeSpan.FromMinutes(offsetMinutes));
    }

    private void EditLoggedEntry(CaloriesLogEntry entry)
    {
        this.editingEntryId = entry.Id;
        this.entryPendingDeleteId = null;
        this.entryEditDraft = new CaloriesLoggedEntryDraft
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
        this.entryEditServingsBaseline = entry.Servings;
        ClearStatus();
    }

    private void ScaleEditedEntryForServingsChange()
    {
        if (this.entryEditServingsBaseline is not decimal previousServings ||
            this.entryEditDraft.Servings is not decimal nextServings ||
            previousServings <= 0 ||
            nextServings <= 0)
        {
            return;
        }

        if (nextServings != decimal.Round(nextServings, 2))
        {
            this.entryEditDraft.Servings = previousServings;
            SetStatus("Serving multipliers can use up to two decimal places.", isError: true);
            return;
        }

        try
        {
            var factor = nextServings / previousServings;
            this.entryEditDraft.Calories = ScaleCalories(this.entryEditDraft.Calories ?? 0, factor);
            this.entryEditDraft.CarbohydratesGrams = ScaleMacro(this.entryEditDraft.CarbohydratesGrams, factor);
            this.entryEditDraft.ProteinGrams = ScaleMacro(this.entryEditDraft.ProteinGrams, factor);
            this.entryEditDraft.FatGrams = ScaleMacro(this.entryEditDraft.FatGrams, factor);
            this.entryEditServingsBaseline = nextServings;
        }
        catch (OverflowException exception)
        {
            this.entryEditDraft.Servings = previousServings;
            Logger.LogWarning(exception, "The edited serving multiplier is too large to calculate nutrition.");
            SetStatus("That serving multiplier is too large to calculate. Use a smaller value.", isError: true);
        }
        catch (ArgumentOutOfRangeException exception)
        {
            this.entryEditDraft.Servings = previousServings;
            Logger.LogWarning(exception, "The edited serving multiplier rounds the food's calories below one kcal.");
            SetStatus("That serving multiplier results in less than 1 kcal. Use a larger value.", isError: true);
        }
    }

    private void CancelEntryEdit()
    {
        this.editingEntryId = null;
        this.entryEditServingsBaseline = null;
        this.entryEditDraft = new CaloriesLoggedEntryDraft();
    }

    private async Task SaveEntryEditAsync()
    {
        if (this.editingEntryId is not int entryId ||
            this.entryEditDraft.Name is null ||
            this.entryEditDraft.Calories is not int calories ||
            this.entryEditDraft.Time is not string time)
        {
            return;
        }

        this.isSavingEntryEdit = true;
        try
        {
            var originalEntry = this.dayEntries.FirstOrDefault(entry => entry.Id == entryId);
            if (originalEntry is null)
            {
                CancelEntryEdit();
                SetStatus("This log entry is no longer available. Refresh the daily log.", isError: true);
                return;
            }

            var updatedEntry = originalEntry with
            {
                Name = this.entryEditDraft.Name.Trim(),
                Calories = calories,
                CarbohydratesGrams = this.entryEditDraft.CarbohydratesGrams,
                ProteinGrams = this.entryEditDraft.ProteinGrams,
                FatGrams = this.entryEditDraft.FatGrams,
                Servings = this.entryEditDraft.Servings,
                LocalDate = this.selectedDate,
                LoggedAt = await GetLoggedAtAsync(time),
                Notes = NormalizeNote(this.entryEditDraft.Notes)
            };

            if (!await CaloriesService.UpdateEntryAsync(updatedEntry, this.lifetimeCancellation.Token))
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
            this.isSavingEntryEdit = false;
        }
    }

    private void BeginEntryDelete(int id)
    {
        this.entryPendingDeleteId = id;
        CancelEntryEdit();
    }

    private void CancelEntryDelete() => this.entryPendingDeleteId = null;

    private async Task RemoveLoggedEntryAsync(int id)
    {
        this.isDeletingEntry = true;
        try
        {
            if (!await CaloriesService.RemoveEntryAsync(id, this.lifetimeCancellation.Token))
            {
                this.entryPendingDeleteId = null;
                SetStatus("This log entry could not be found. Refresh the daily log.", isError: true);
                return;
            }

            this.entryPendingDeleteId = null;
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
            this.isDeletingEntry = false;
        }
    }

    private async Task SaveFoodAsync()
    {
        if (this.foodDraft.Name is null || this.foodDraft.CaloriesPerServing is not int calories)
        {
            return;
        }

        this.isSavingFood = true;
        try
        {
            var food = new CaloriesFood(
                this.editingFoodId ?? 0,
                this.foodDraft.Name.Trim(),
                calories,
                this.foodDraft.CarbohydratesGrams,
                this.foodDraft.ProteinGrams,
                this.foodDraft.FatGrams);
            var isNewFood = this.editingFoodId is null;
            var succeeded = this.editingFoodId is int id
                ? await CaloriesService.UpdateFoodAsync(
                    food with { Id = id },
                    this.lifetimeCancellation.Token)
                : await AddFoodAsync(food);

            if (!succeeded)
            {
                SetStatus("The saved food could not be found. Refresh and try again.", isError: true);
                return;
            }

            this.foodDraft = new CaloriesFoodDraft();
            this.editingFoodId = null;
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
            this.isSavingFood = false;
        }
    }

    private async Task<bool> AddFoodAsync(CaloriesFood food)
    {
        await CaloriesService.AddFoodAsync(food, this.lifetimeCancellation.Token);
        return true;
    }

    private void EditFood(CaloriesFood food)
    {
        this.editingFoodId = food.Id;
        this.foodDraft = new CaloriesFoodDraft
        {
            Name = food.Name,
            CaloriesPerServing = food.CaloriesPerServing,
            CarbohydratesGrams = food.CarbohydratesGrams,
            ProteinGrams = food.ProteinGrams,
            FatGrams = food.FatGrams
        };
        this.foodPendingDeleteId = null;
        ClearStatus();
    }

    private void CancelFoodEdit()
    {
        this.editingFoodId = null;
        this.foodDraft = new CaloriesFoodDraft();
    }

    private void BeginFoodDelete(int id)
    {
        this.foodPendingDeleteId = id;
        this.editingFoodId = null;
    }

    private void CancelFoodDelete() => this.foodPendingDeleteId = null;

    private async Task RemoveFoodAsync(int id)
    {
        try
        {
            var removed = await CaloriesService.RemoveFoodAsync(id, this.lifetimeCancellation.Token);
            if (!removed)
            {
                SetStatus("The saved food could not be found. Refresh and try again.", isError: true);
                return;
            }

            this.foodPendingDeleteId = null;
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
            this.foods = await CaloriesService.GetFoodsAsync(this.lifetimeCancellation.Token);
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
        this.historyPeriod = period;
        await LoadHistoryAsync();
    }

    private async Task LoadHistoryAsync()
    {
        this.isLoadingHistory = true;
        this.historyError = null;
        this.selectedChartPointIndex = null;
        try
        {
            (this.historyFrom, this.historyThrough) = GetHistoryRange(this.historyPeriod, this.today);
            var entries = await CaloriesService.GetEntriesAsync(
                this.historyFrom,
                this.historyThrough,
                this.lifetimeCancellation.Token);
            this.historyPoints = CreateHistoryPoints(entries, this.historyPeriod, this.historyFrom, this.historyThrough);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            Logger.LogError(exception, "Could not load {Period} Calories history.", this.historyPeriod);
            this.historyError = "Calories history could not be loaded. Check the connection and try again.";
        }
        finally
        {
            this.isLoadingHistory = false;
        }
    }

    private async Task HandleNotificationReceivedAsync(string notificationType)
    {
        if (notificationType != RealtimeNotificationTypes.CaloriesChanged || this.isDisposed || this.today == default)
        {
            return;
        }

        await InvokeAsync(async () =>
        {
            try
            {
                await LoadTrackerDataAsync(this.lifetimeCancellation.Token);
                if (this.activeTab == CaloriesTab.Statistics)
                {
                    await LoadHistoryAsync();
                }
                else if (this.activeTab == CaloriesTab.Foods)
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

    private void SelectChartPoint(int index) => this.selectedChartPointIndex = index;

    private void OpenTabletEntryForm()
    {
        this.showTabletEntryForm = true;
        ClearStatus();
    }

    private void CloseTabletEntryForm() => this.showTabletEntryForm = false;

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
        this.statusMessage = message;
        this.statusIsError = isError;
    }

    private void ClearStatus()
    {
        this.statusMessage = null;
        this.statusIsError = false;
    }

    public async ValueTask DisposeAsync()
    {
        this.isDisposed = true;
        this.lifetimeCancellation.Cancel();
        NotificationEventSource.NotificationReceived -= HandleNotificationReceivedAsync;
        if (this.isNotificationsStarted)
        {
            await NotificationEventSource.StopAsync();
        }

        if (this.caloriesJsModule is not null)
        {
            await this.caloriesJsModule.DisposeAsync();
        }

        this.lifetimeCancellation.Dispose();
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
        CustomFood,
        Exercise
    }

    private enum HistoryPeriod
    {
        Daily,
        Weekly,
        Monthly
    }

    private sealed record BrowserClock(string Date, string Time);

    private sealed record DayLogRow(DateTimeOffset LoggedAt, int Kind, CaloriesLogEntry? Food, ExerciseEntry? Exercise);

    private sealed record CaloriesChartPoint(DateOnly Start, string Label, long Calories);
}
