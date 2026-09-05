using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using AstroCalc.Core;

namespace HomeServerPage.Data.Astronomy;

public sealed class ObservationPoint
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Range(-90, 90)]
    public double Latitude { get; set; }

    [Range(-180, 180)]
    public double Longitude { get; set; }

    public double? ElevationMeters { get; set; }

    [Range(0, 90)]
    public double HorizonNorth { get; set; }

    [Range(0, 90)]
    public double HorizonNorthEast { get; set; }

    [Range(0, 90)]
    public double HorizonEast { get; set; }

    [Range(0, 90)]
    public double HorizonSouthEast { get; set; }

    [Range(0, 90)]
    public double HorizonSouth { get; set; }

    [Range(0, 90)]
    public double HorizonSouthWest { get; set; }

    [Range(0, 90)]
    public double HorizonWest { get; set; }

    [Range(0, 90)]
    public double HorizonNorthWest { get; set; }

    [JsonIgnore]
    public GeographicCoordinate Coordinates => GeographicCoordinate.FromDegrees(
        Latitude,
        Longitude,
        ElevationMeters ?? 0);

    [JsonIgnore]
    public HorizonProfile Horizon => HorizonProfile.FromObservationPoint(this);
}
