using HomeServerPage.Data.Locations;

namespace HomeServerPage.Data.Locations;

public sealed class UnavailableWeatherLocationResolver : IWeatherLocationResolver
{
    public Task<SavedLocation> ResolveAsync(
        string query,
        CancellationToken cancellationToken = default)
    {
        throw new InvalidOperationException(
            $"The configured initial weather location '{query}' can only be resolved in Production.");
    }
}
