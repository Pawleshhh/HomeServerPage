using HomeServerPage.Data.Astronomy;
using System.Net.Http.Json;

namespace HomeServerPage.Client.Data.Astronomy.Telescopes;

public class TelescopeHttpService(HttpClient httpClient) : ITelescopeService
{
    public async Task<IEnumerable<TelescopeItem>> GetTelescopesAsync(CancellationToken cancellationToken = default)
    {
        return await httpClient.GetFromJsonAsync<IEnumerable<TelescopeItem>>("api/telescope", cancellationToken) ?? [];
    }

    public async Task<IEnumerable<TelescopeEyepiece>> GetEyepiecesAsync(CancellationToken cancellationToken = default)
    {
        return await httpClient.GetFromJsonAsync<IEnumerable<TelescopeEyepiece>>("api/telescope/eyepieces", cancellationToken) ?? [];
    }

    public async Task<IEnumerable<TelescopeLens>> GetLensesAsync(CancellationToken cancellationToken = default)
    {
        return await httpClient.GetFromJsonAsync<IEnumerable<TelescopeLens>>("api/telescope/lenses", cancellationToken) ?? [];
    }

    public async Task<IEnumerable<SensorItem>> GetSensorsAsync(CancellationToken cancellationToken = default)
    {
        return await httpClient.GetFromJsonAsync<IEnumerable<SensorItem>>("api/telescope/sensors", cancellationToken) ?? [];
    }

    public async Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        return await httpClient.GetFromJsonAsync<bool>("api/telescope/status", cancellationToken);
    }
}
