using HomeServerPage.Data.Locations;

namespace HomeServerPage.Data.Locations;

public sealed class LocationSelection
{
    public LocationFeature Feature { get; set; }

    public int LocationId { get; set; }

    public bool IsInitialized { get; set; }
}
