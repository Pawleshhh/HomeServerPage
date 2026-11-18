using Bunit;
using HomeServerPage.Client.Data;
using HomeServerPage.Client.Pages.Calories;
using HomeServerPage.Data.Calories;
using HomeServerPage.Data.Exercise;
using HomeServerPage.Data.Notifications;
using HomeServerPage.Tests.Unit.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using Moq;

namespace HomeServerPage.Tests.Unit.Components;

public sealed class CaloriesComponentTests
{
    [Test]
    public void SavedFoodFormScalesNutritionAndRendersTheSavedEntry()
    {
        var food = new CaloriesFood(1, "Oats", 101, 10, 5, null, 40m);
        var entries = new List<CaloriesLogEntry>();
        var caloriesService = new Mock<ICaloriesService>();
        caloriesService
            .Setup(service => service.GetFoodsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<CaloriesFood> { food });
        caloriesService
            .Setup(service => service.GetDailyGoalAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(2000);
        caloriesService
            .Setup(service => service.GetEntriesAsync(
                It.IsAny<DateOnly>(),
                It.IsAny<DateOnly>(),
                It.IsAny<CancellationToken>()))
            .Returns((DateOnly _, DateOnly _, CancellationToken _) =>
                Task.FromResult(entries.ToList()));
        caloriesService
            .Setup(service => service.AddEntryAsync(
                It.IsAny<CaloriesLogEntry>(),
                It.IsAny<CancellationToken>()))
            .Returns((CaloriesLogEntry entry, CancellationToken _) =>
            {
                var savedEntry = entry with { Id = 12 };
                entries.Add(savedEntry);
                return Task.FromResult(savedEntry);
            });
        var exerciseService = new Mock<IExerciseService>();
        exerciseService
            .Setup(service => service.GetEntriesAsync(
                It.IsAny<DateOnly>(),
                It.IsAny<DateOnly>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        var notifications = new TestRealtimeNotificationEventSource();
        using var context = new BunitContext();
        context.Services.AddSingleton<ICaloriesService>(caloriesService.Object);
        context.Services.AddSingleton<IExerciseService>(exerciseService.Object);
        context.Services.AddSingleton<IRealtimeNotificationEventSource>(notifications);
        context.Services.AddSingleton<IJSRuntime>(
            new FixedBrowserJsRuntime(new DateOnly(2026, 11, 18), new TimeOnly(12, 0)));

        var component = context.Render<Calories>();
        component.WaitForAssertion(() =>
            Assert.That(component.Markup, Does.Contain("Oats - 101 kcal/serving")));

        component.Find("#calories-saved-food").Change("1");
        component.Find("#calories-serving-count").Change("1.5");
        component.Find("#calories-entry-notes").Change("Afternoon snack");
        component
            .FindAll("form.calories-entry-form")
            .Single(form => form.QuerySelector("#calories-saved-food") is not null)
            .Submit();

        component.WaitForAssertion(() =>
        {
            Assert.That(component.Find(".calories-status").TextContent, Does.Contain("Food added"));
            Assert.That(component.Markup, Does.Contain("152"));
        });
        caloriesService.Verify(
            service => service.AddEntryAsync(
                It.Is<CaloriesLogEntry>(entry =>
                    entry.Name == "Oats" &&
                    entry.Calories == 152 &&
                    entry.CarbohydratesGrams == 15 &&
                    entry.ProteinGrams == 8 &&
                    entry.FatGrams == null &&
                    entry.Servings == 1.5m &&
                    entry.GramsConsumed == 60m &&
                    entry.Notes == "Afternoon snack" &&
                    entry.LocalDate == new DateOnly(2026, 11, 18)),
                It.IsAny<CancellationToken>()),
            Times.Once);
        Assert.That(notifications.IsStarted, Is.True);
    }

    [Test]
    public void TabletModeHidesWeeklyAndGoalEditorButKeepsDailyCalories()
    {
        var caloriesService = new Mock<ICaloriesService>();
        caloriesService
            .Setup(service => service.GetFoodsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        caloriesService
            .Setup(service => service.GetDailyGoalAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1800);
        caloriesService
            .Setup(service => service.GetEntriesAsync(
                It.IsAny<DateOnly>(),
                It.IsAny<DateOnly>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        var exerciseService = new Mock<IExerciseService>();
        exerciseService
            .Setup(service => service.GetEntriesAsync(
                It.IsAny<DateOnly>(),
                It.IsAny<DateOnly>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        using var context = new BunitContext();
        context.Services.AddSingleton<ICaloriesService>(caloriesService.Object);
        context.Services.AddSingleton<IExerciseService>(exerciseService.Object);
        context.Services.AddSingleton<IRealtimeNotificationEventSource>(
            new TestRealtimeNotificationEventSource());
        context.Services.AddSingleton<IJSRuntime>(
            new FixedBrowserJsRuntime(new DateOnly(2026, 11, 18), new TimeOnly(12, 0)));

        var component = context.Render<Calories>(parameters =>
            parameters.Add(page => page.TabletMode, true));

        component.WaitForAssertion(() =>
        {
            Assert.That(component.Find("#calories-daily-panel"), Is.Not.Null);
            Assert.That(component.Find("#daily-total-heading"), Is.Not.Null);
            Assert.That(component.FindAll("#weekly-total-heading"), Is.Empty);
            Assert.That(component.FindAll("#calories-goal-heading"), Is.Empty);
        });
    }
}
