using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using AstroCalc.Core;
using HomeServerPage.Data.Locations;

namespace HomeServerPage.Data.Astronomy;

public sealed class ObservationPoint
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    public int? LocationId { get; set; }

    [NotMapped]
    public SavedLocation? Location { get; set; }

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
    [NotMapped]
    public GeographicCoordinate Coordinates => GeographicCoordinate.FromDegrees(
        this.Location?.Latitude ?? throw new InvalidOperationException("The observation point location is not loaded."),
        this.Location.Longitude,
        this.Location.ElevationMeters);

    [JsonIgnore]
    [NotMapped]
    public HorizonProfile Horizon => HorizonProfile.FromObservationPoint(this);
}
