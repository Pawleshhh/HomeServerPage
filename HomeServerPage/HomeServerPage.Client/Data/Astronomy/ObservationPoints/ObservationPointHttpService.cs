using System.Net.Http.Json;
using HomeServerPage.Data.Astronomy;

namespace HomeServerPage.Data.Astronomy;

public sealed class ObservationPointHttpService(HttpClient httpClient) : IObservationPointService
{
    private const string RequestUri = "api/observation-points";

    public async Task<IReadOnlyList<ObservationPoint>> GetObservationPointsAsync(
        CancellationToken cancellationToken = default)
    {
        return await httpClient.GetFromJsonAsync<List<ObservationPoint>>(
            RequestUri,
            cancellationToken) ?? [];
    }

    public async Task<ObservationPoint> SaveObservationPointAsync(
        ObservationPoint observationPoint,
        CancellationToken cancellationToken = default)
    {
        using var response = observationPoint.Id == 0
            ? await httpClient.PostAsJsonAsync(RequestUri, observationPoint, cancellationToken)
            : await httpClient.PutAsJsonAsync(
                $"{RequestUri}/{observationPoint.Id}",
                observationPoint,
                cancellationToken);

        response.EnsureSuccessStatusCode();

        return (await response.Content.ReadFromJsonAsync<ObservationPoint>(cancellationToken: cancellationToken))
            ?? throw new InvalidOperationException("The observation point response was empty.");
    }

    public async Task DeleteObservationPointAsync(
        int observationPointId,
        CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.DeleteAsync(
            $"{RequestUri}/{observationPointId}",
            cancellationToken);
        response.EnsureSuccessStatusCode();
    }
}
