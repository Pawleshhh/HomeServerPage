using System.Net.Http.Json;

namespace HomeServerPage.Data.Astronomy;

public class AstronomyHttpService(HttpClient httpClient) : IAstronomyService
{
    public async Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        return await httpClient.GetFromJsonAsync<bool>("api/astronomy/status", cancellationToken);
    }
}
