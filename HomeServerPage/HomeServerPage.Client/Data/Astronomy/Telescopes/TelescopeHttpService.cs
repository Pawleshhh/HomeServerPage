using HomeServerPage.Data.Astronomy;
using System.Net.Http.Json;

namespace HomeServerPage.Client.Data.Astronomy.Telescopes;

public class TelescopeHttpService(HttpClient httpClient) : ITelescopeService
{
    public async Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        return await httpClient.GetFromJsonAsync<bool>("api/telescope/status", cancellationToken);
    }
}
