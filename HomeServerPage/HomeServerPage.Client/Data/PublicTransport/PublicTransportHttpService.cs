using System.Net.Http.Json;

namespace HomeServerPage.Data.PublicTransport;

public class PublicTransportHttpService(HttpClient httpClient) : IPublicTransportService
{
    public async Task<DepartureBoard> GetDepartureBoardAsync(int stopNumber, int limit = 10, CancellationToken cancellationToken = default)
    {
        return (await httpClient.GetFromJsonAsync<DepartureBoard>(
            $"api/publictransport/departure-boards/{stopNumber}?limit={limit}",
            cancellationToken))!;
    }

    public async Task<List<DepartureBoard>> GetDepartureBoardsAsync(int limit = 10, CancellationToken cancellationToken = default)
    {
        return await httpClient.GetFromJsonAsync<List<DepartureBoard>>(
            $"api/publictransport/departure-boards?limit={limit}",
            cancellationToken) ?? [];
    }
}
