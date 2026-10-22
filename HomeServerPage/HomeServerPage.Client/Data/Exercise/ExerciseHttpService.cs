using System.Globalization;
using System.Net.Http.Json;

namespace HomeServerPage.Data.Exercise;

public sealed class ExerciseHttpService(HttpClient httpClient) : IExerciseService
{
    public async Task<List<ExerciseEntry>> GetEntriesAsync(
        DateOnly from,
        DateOnly through,
        CancellationToken cancellationToken = default)
    {
        var fromValue = Uri.EscapeDataString(from.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
        var throughValue = Uri.EscapeDataString(through.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
        return await httpClient.GetFromJsonAsync<List<ExerciseEntry>>(
                $"api/exercise/entries?from={fromValue}&through={throughValue}",
                cancellationToken)
            ?? throw new InvalidOperationException("The Exercise API returned no entry list.");
    }

    public async Task<ExerciseEntry> AddEntryAsync(
        ExerciseEntry entry,
        CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.PostAsJsonAsync("api/exercise/entries", entry, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ExerciseEntry>(cancellationToken)
            ?? throw new InvalidOperationException("The Exercise API returned no created entry.");
    }

    public async Task<bool> UpdateEntryAsync(
        ExerciseEntry entry,
        CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.PutAsJsonAsync(
            $"api/exercise/entries/{entry.Id}",
            entry,
            cancellationToken);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> RemoveEntryAsync(int id, CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.DeleteAsync($"api/exercise/entries/{id}", cancellationToken);
        return response.IsSuccessStatusCode;
    }
}
