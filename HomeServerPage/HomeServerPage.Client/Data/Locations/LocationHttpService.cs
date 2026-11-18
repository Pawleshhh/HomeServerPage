using System.Net.Http.Json;
using HomeServerPage.Data.Locations;

namespace HomeServerPage.Client.Data.Locations;

public sealed class LocationHttpService(HttpClient httpClient) : ILocationService
{
    private const string RequestUri = "api/locations";

    public async Task<IReadOnlyList<SavedLocation>> GetLocationsAsync(
        CancellationToken cancellationToken = default)
    {
        return await httpClient.GetFromJsonAsync<List<SavedLocation>>(
            RequestUri,
            cancellationToken) ?? [];
    }

    public Task<SavedLocation?> GetLocationAsync(
        int locationId,
        CancellationToken cancellationToken = default)
    {
        return httpClient.GetFromJsonAsync<SavedLocation>(
            $"{RequestUri}/{locationId}",
            cancellationToken);
    }

    public async Task<SavedLocation> SaveLocationAsync(
        SavedLocation location,
        CancellationToken cancellationToken = default)
    {
        using var response = location.Id == 0
            ? await httpClient.PostAsJsonAsync(RequestUri, location, cancellationToken)
            : await httpClient.PutAsJsonAsync(
                $"{RequestUri}/{location.Id}",
                location,
                cancellationToken);
        await LocationHttpResponse.EnsureSuccessAsync(response, cancellationToken);

        return await response.Content.ReadFromJsonAsync<SavedLocation>(
                   cancellationToken: cancellationToken)
               ?? throw new InvalidOperationException("The location response was empty.");
    }

    public async Task DeleteLocationAsync(
        int locationId,
        CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.DeleteAsync(
            $"{RequestUri}/{locationId}",
            cancellationToken);
        await LocationHttpResponse.EnsureSuccessAsync(response, cancellationToken);
    }
}
