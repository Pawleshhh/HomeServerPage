using HomeServerPage.Data.Locations;

namespace HomeServerPage.Client.Data.Locations;

public interface ILocationSelectionService
{
    Task<SavedLocation> GetSelectedLocationAsync(
        LocationFeature feature,
        CancellationToken cancellationToken = default);

    Task<SavedLocation> SetSelectedLocationAsync(
        LocationFeature feature,
        int locationId,
        CancellationToken cancellationToken = default);
}
