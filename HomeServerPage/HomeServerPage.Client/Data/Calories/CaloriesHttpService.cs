using System.Globalization;
using System.Net.Http.Json;

namespace HomeServerPage.Data.Calories;

public sealed class CaloriesHttpService(HttpClient httpClient) : ICaloriesService
{
    public async Task<List<CaloriesFood>> GetFoodsAsync(CancellationToken cancellationToken = default) =>
        await httpClient.GetFromJsonAsync<List<CaloriesFood>>("api/calories/foods", cancellationToken)
        ?? throw new InvalidOperationException("The Calories API returned no food list.");

    public async Task<CaloriesFood> AddFoodAsync(
        CaloriesFood food,
        CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.PostAsJsonAsync("api/calories/foods", food, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<CaloriesFood>(cancellationToken)
            ?? throw new InvalidOperationException("The Calories API returned no created food.");
    }

    public async Task<bool> UpdateFoodAsync(
        CaloriesFood food,
        CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.PutAsJsonAsync(
            $"api/calories/foods/{food.Id}",
            food,
            cancellationToken);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> RemoveFoodAsync(int id, CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.DeleteAsync($"api/calories/foods/{id}", cancellationToken);
        return response.IsSuccessStatusCode;
    }

    public async Task<List<CaloriesLogEntry>> GetEntriesAsync(
        DateOnly from,
        DateOnly through,
        CancellationToken cancellationToken = default)
    {
        var fromValue = Uri.EscapeDataString(from.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
        var throughValue = Uri.EscapeDataString(through.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
        return await httpClient.GetFromJsonAsync<List<CaloriesLogEntry>>(
                $"api/calories/entries?from={fromValue}&through={throughValue}",
                cancellationToken)
            ?? throw new InvalidOperationException("The Calories API returned no entry list.");
    }

    public async Task<CaloriesLogEntry> AddEntryAsync(
        CaloriesLogEntry entry,
        CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.PostAsJsonAsync(
            "api/calories/entries",
            entry,
            cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<CaloriesLogEntry>(cancellationToken)
            ?? throw new InvalidOperationException("The Calories API returned no created entry.");
    }

    public async Task<int?> GetDailyGoalAsync(CancellationToken cancellationToken = default)
    {
        var response = await httpClient.GetFromJsonAsync<CaloriesGoalResponse>(
            "api/calories/goal",
            cancellationToken);
        if (response is null)
        {
            throw new InvalidOperationException("The Calories API returned no goal response.");
        }

        return response.Calories;
    }

    public async Task<int> SetDailyGoalAsync(
        int calories,
        CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.PutAsJsonAsync(
            "api/calories/goal",
            calories,
            cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<int>(cancellationToken);
    }
}
