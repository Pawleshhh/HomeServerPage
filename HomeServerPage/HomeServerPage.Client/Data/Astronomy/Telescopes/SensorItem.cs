namespace HomeServerPage.Client.Data.Astronomy.Telescopes;

public record SensorItem(
    string Name,

    // Sensor
    int ResolutionWidthPx,
    int ResolutionHeightPx,
    double PixelSizeUm,
    double SensorWidthMm,
    double SensorHeightMm
)
{
    public int Id { get; set; }
}
