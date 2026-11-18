using System.Net.Http.Json;
using HomeServerPage.Data.Locations;

namespace HomeServerPage.Client.Data.Locations;

public sealed class LocationSelectionHttpService(HttpClient httpClient) : ILocationSelectionService
{
    public async Task<SavedLocation> GetSelectedLocationAsync(
        LocationFeature feature,
        CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.GetAsync(
            $"api/locations/selection/{feature}",
            cancellationToken);
        await LocationHttpResponse.EnsureSuccessAsync(response, cancellationToken);

        return await response.Content.ReadFromJsonAsync<SavedLocation>(
                   cancellationToken: cancellationToken)
               ?? throw new InvalidOperationException("The location selection response was empty.");
    }

    public async Task<SavedLocation> SetSelectedLocationAsync(
        LocationFeature feature,
        int locationId,
        CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.PutAsJsonAsync(
            $"api/locations/selection/{feature}",
            new LocationSelectionRequest(locationId),
            cancellationToken);
        await LocationHttpResponse.EnsureSuccessAsync(response, cancellationToken);

        return await response.Content.ReadFromJsonAsync<SavedLocation>(
                   cancellationToken: cancellationToken)
               ?? throw new InvalidOperationException("The location selection response was empty.");
    }
}
