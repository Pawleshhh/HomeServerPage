using System.Net.Http.Json;

namespace HomeServerPage.Client.Data;

public interface ITabletDashboardRefreshSettingsProvider
{
    Task<TabletDashboardRefreshOptions> GetSettingsAsync(CancellationToken cancellationToken);
}

public sealed class OptionsTabletDashboardRefreshSettingsProvider(
    Microsoft.Extensions.Options.IOptions<TabletDashboardRefreshOptions> options)
    : ITabletDashboardRefreshSettingsProvider
{
    public Task<TabletDashboardRefreshOptions> GetSettingsAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(options.Value);
    }
}

public sealed class HttpTabletDashboardRefreshSettingsProvider(HttpClient httpClient)
    : ITabletDashboardRefreshSettingsProvider
{
    public async Task<TabletDashboardRefreshOptions> GetSettingsAsync(CancellationToken cancellationToken)
    {
        return await httpClient.GetFromJsonAsync<TabletDashboardRefreshOptions>(
                   "api/tablet-dashboard-refresh-settings",
                   cancellationToken)
               ?? throw new InvalidOperationException("The dashboard refresh settings response was empty.");
    }
}
