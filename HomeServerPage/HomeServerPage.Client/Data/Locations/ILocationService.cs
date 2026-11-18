using HomeServerPage.Data.Locations;

namespace HomeServerPage.Client.Data.Locations;

public interface ILocationService
{
    Task<IReadOnlyList<SavedLocation>> GetLocationsAsync(CancellationToken cancellationToken = default);

    Task<SavedLocation?> GetLocationAsync(int locationId, CancellationToken cancellationToken = default);

    Task<SavedLocation> SaveLocationAsync(
        SavedLocation location,
        CancellationToken cancellationToken = default);

    Task DeleteLocationAsync(int locationId, CancellationToken cancellationToken = default);
}
