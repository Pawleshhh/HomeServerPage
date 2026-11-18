using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using AstroCalc.Core;

namespace HomeServerPage.Data.Locations;

public sealed class SavedLocation
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Range(-90, 90)]
    public double Latitude { get; set; }

    [Range(-180, 180)]
    public double Longitude { get; set; }

    public double ElevationMeters { get; set; }

    [Required]
    [StringLength(100)]
    public string TimeZoneId { get; set; } = "Etc/UTC";

    public bool IsSystemDefault { get; set; }

    [JsonIgnore]
    public int? LegacyObservationPointId { get; set; }

    [JsonIgnore]
    public GeographicCoordinate Coordinates => GeographicCoordinate.FromDegrees(
        this.Latitude,
        this.Longitude,
        this.ElevationMeters);
}
