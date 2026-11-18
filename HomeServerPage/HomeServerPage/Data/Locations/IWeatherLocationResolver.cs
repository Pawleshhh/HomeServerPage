using HomeServerPage.Data.Locations;

namespace HomeServerPage.Data.Locations;

public interface IWeatherLocationResolver
{
    Task<SavedLocation> ResolveAsync(string query, CancellationToken cancellationToken = default);
}
