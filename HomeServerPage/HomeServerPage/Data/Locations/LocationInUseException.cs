namespace HomeServerPage.Data.Locations;

public sealed class LocationInUseException(int locationId)
    : InvalidOperationException($"Location {locationId} is used by an observation point.")
{
    public int LocationId { get; } = locationId;
}
