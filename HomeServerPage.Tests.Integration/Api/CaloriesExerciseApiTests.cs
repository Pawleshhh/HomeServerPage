using System.Net;
using System.Net.Http.Json;
using HomeServerPage.Data.Calories;
using HomeServerPage.Data.Exercise;
using HomeServerPage.Data.Notifications;
using HomeServerPage.Tests.Integration.Infrastructure;
using Moq;

namespace HomeServerPage.Tests.Integration.Api;

public sealed class CaloriesExerciseApiTests
{
    [Test]
    public async Task FoodEntryAndGoalFlowPersistsSnapshotAndHandlesLocalDateOffset()
    {
        using var factory = new HomeServerPageWebApplicationFactory();
        using var client = factory.CreateClient();
        var foodResponse = await client.PostAsJsonAsync(
            "/api/calories/foods",
            new CaloriesFood(0, " Oats ", 150, 25, 5, 3, 40.1m));
        var food = await foodResponse.Content.ReadFromJsonAsync<CaloriesFood>()
            ?? throw new InvalidOperationException("The saved food response was empty.");
        var localDate = new DateOnly(2026, 11, 19);
        var loggedAt = new DateTimeOffset(2026, 11, 19, 0, 30, 0, TimeSpan.FromHours(10));
        var entry = new CaloriesLogEntry(
            0,
            "Oats",
            188,
            31,
            6,
            4,
            1.25m,
            localDate,
            loggedAt,
            new string('n', 500),
            50.125m);

        var entryResponse = await client.PostAsJsonAsync("/api/calories/entries", entry);
        var createdEntry = await entryResponse.Content.ReadFromJsonAsync<CaloriesLogEntry>()
            ?? throw new InvalidOperationException("The calorie entry response was empty.");
        var earlierEntry = entry with
        {
            Id = 0,
            Name = "Coffee",
            Calories = 50,
            CarbohydratesGrams = null,
            ProteinGrams = null,
            FatGrams = null,
            Servings = null,
            LoggedAt = new DateTimeOffset(2026, 11, 19, 0, 15, 0, TimeSpan.FromHours(10)),
            Notes = null,
            GramsConsumed = null
        };
        var earlierEntryResponse = await client.PostAsJsonAsync("/api/calories/entries", earlierEntry);
        var entries = await client.GetFromJsonAsync<List<CaloriesLogEntry>>(
            $"/api/calories/entries?from={localDate:yyyy-MM-dd}&through={localDate:yyyy-MM-dd}");
        var updateFoodResponse = await client.PutAsJsonAsync(
            $"/api/calories/foods/{food.Id}",
            food with { Name = "Oats updated", CaloriesPerServing = 999 });
        var entriesAfterFoodUpdate = await client.GetFromJsonAsync<List<CaloriesLogEntry>>(
            $"/api/calories/entries?from={localDate:yyyy-MM-dd}&through={localDate:yyyy-MM-dd}");
        var updateEntryResponse = await client.PutAsJsonAsync(
            $"/api/calories/entries/{createdEntry.Id}",
            createdEntry with { Notes = "Edited note", Calories = 190 });
        var deleteFoodResponse = await client.DeleteAsync($"/api/calories/foods/{food.Id}");
        var remainingFoods = await client.GetFromJsonAsync<List<CaloriesFood>>("/api/calories/foods");
        var deleteEntryResponse = await client.DeleteAsync($"/api/calories/entries/{createdEntry.Id}");
        var entriesAfterDelete = await client.GetFromJsonAsync<List<CaloriesLogEntry>>(
            $"/api/calories/entries?from={localDate:yyyy-MM-dd}&through={localDate:yyyy-MM-dd}");
        var goalResponse = await client.PutAsJsonAsync("/api/calories/goal", 2000);
        var storedGoal = await client.GetStringAsync("/api/calories/goal");

        Assert.That(foodResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(food!.Name, Is.EqualTo("Oats"));
        Assert.That(entryResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(createdEntry.LocalDate, Is.EqualTo(localDate));
        Assert.That(createdEntry.LoggedAt, Is.EqualTo(loggedAt));
        Assert.That(createdEntry.Notes, Has.Length.EqualTo(500));
        Assert.That(earlierEntryResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(entries, Has.Count.EqualTo(2));
        Assert.That(entries![0].Name, Is.EqualTo("Coffee"));
        Assert.That(entries[1].Name, Is.EqualTo("Oats"));
        Assert.That(entries[1].GramsConsumed, Is.EqualTo(50.125m));
        Assert.That(updateFoodResponse.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
        Assert.That(entriesAfterFoodUpdate!.Single(item => item.Id == createdEntry.Id).Calories, Is.EqualTo(188));
        Assert.That(updateEntryResponse.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
        Assert.That(deleteFoodResponse.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
        Assert.That(remainingFoods, Is.Empty);
        Assert.That(deleteEntryResponse.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
        Assert.That(entriesAfterDelete, Has.Count.EqualTo(1));
        Assert.That(entriesAfterDelete![0].Name, Is.EqualTo("Coffee"));
        Assert.That(goalResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(storedGoal, Does.Contain("2000"));
        factory.NotificationPublisher.Verify(
            publisher => publisher.PublishAsync(
                RealtimeNotificationTypes.CaloriesChanged,
                It.IsAny<CancellationToken>()),
            Times.AtLeast(2));
    }

    [Test]
    public async Task CaloriesRejectInvalidNutritionPrecisionDatesAndGoal()
    {
        using var factory = new HomeServerPageWebApplicationFactory();
        using var client = factory.CreateClient();
        var invalidFood = new CaloriesFood(0, "Rice", 0, -1, null, null, 10.12m);
        var invalidEntry = new CaloriesLogEntry(
            0,
            "Rice",
            100,
            -1,
            null,
            null,
            1.251m,
            new DateOnly(2026, 11, 19),
            new DateTimeOffset(2026, 11, 19, 0, 30, 0, TimeSpan.FromHours(10)),
            null,
            10.1234m);
        var futureEntry = invalidEntry with
        {
            CarbohydratesGrams = null,
            Servings = 1m,
            GramsConsumed = null,
            LocalDate = new DateOnly(2026, 11, 20),
            LoggedAt = new DateTimeOffset(2026, 11, 20, 12, 0, 0, TimeSpan.FromHours(10))
        };
        var mismatchedDateEntry = futureEntry with
        {
            LocalDate = new DateOnly(2026, 11, 18)
        };

        var foodResponse = await client.PostAsJsonAsync("/api/calories/foods", invalidFood);
        var entryResponse = await client.PostAsJsonAsync("/api/calories/entries", invalidEntry);
        var futureResponse = await client.PostAsJsonAsync("/api/calories/entries", futureEntry);
        var mismatchResponse = await client.PostAsJsonAsync("/api/calories/entries", mismatchedDateEntry);
        var goalResponse = await client.PutAsJsonAsync("/api/calories/goal", 0);

        Assert.That(foodResponse.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        Assert.That(entryResponse.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        Assert.That(futureResponse.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        Assert.That(mismatchResponse.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        Assert.That(goalResponse.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task ExerciseStoresServerCalculatedCaloriesAndSupportsUpdatesAndDeletion()
    {
        using var factory = new HomeServerPageWebApplicationFactory();
        using var client = factory.CreateClient();
        var running = new ExerciseEntry(
            0,
            ExerciseType.Running,
            3m,
            9999,
            new DateOnly(2026, 11, 19),
            new DateTimeOffset(2026, 11, 19, 0, 30, 0, TimeSpan.FromHours(10)));

        var createResponse = await client.PostAsJsonAsync("/api/exercise/entries", running);
        var created = await createResponse.Content.ReadFromJsonAsync<ExerciseEntry>();
        var gym = running with
        {
            Type = ExerciseType.Gym,
            DistanceKm = null,
            CaloriesBurned = 1
        };
        var updateResponse = await client.PutAsJsonAsync(
            $"/api/exercise/entries/{created!.Id}",
            gym);
        var updatedEntries = await client.GetFromJsonAsync<List<ExerciseEntry>>(
            "/api/exercise/entries?from=2026-11-19&through=2026-11-19");
        var deleteResponse = await client.DeleteAsync($"/api/exercise/entries/{created.Id}");

        Assert.That(createResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(created.CaloriesBurned, Is.EqualTo(250));
        Assert.That(updateResponse.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
        Assert.That(updatedEntries, Has.Count.EqualTo(1));
        Assert.That(updatedEntries![0].CaloriesBurned, Is.EqualTo(ExerciseCalorieCalculator.GymCalories));
        Assert.That(deleteResponse.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
        factory.NotificationPublisher.Verify(
            publisher => publisher.PublishAsync(
                RealtimeNotificationTypes.CaloriesChanged,
                It.IsAny<CancellationToken>()),
            Times.AtLeast(3));
    }

    [Test]
    public async Task ExerciseRejectsInvalidDistanceAndFutureDates()
    {
        using var factory = new HomeServerPageWebApplicationFactory();
        using var client = factory.CreateClient();
        var invalidDistance = new ExerciseEntry(
            0,
            ExerciseType.Bike,
            0m,
            0,
            new DateOnly(2026, 11, 18),
            new DateTimeOffset(2026, 11, 18, 12, 0, 0, TimeSpan.Zero));
        var future = invalidDistance with
        {
            DistanceKm = 1m,
            LocalDate = new DateOnly(2026, 11, 20),
            LoggedAt = new DateTimeOffset(2026, 11, 20, 12, 0, 0, TimeSpan.Zero)
        };

        var invalidResponse = await client.PostAsJsonAsync("/api/exercise/entries", invalidDistance);
        var futureResponse = await client.PostAsJsonAsync("/api/exercise/entries", future);

        Assert.That(invalidResponse.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        Assert.That(futureResponse.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }
}
