namespace HomeServerPage.Client.Data.Astronomy.Telescopes;

public record CameraItem(
    string Name,

    // Sensor
    int ResolutionWidthPx,
    int ResolutionHeightPx,
    double PixelSizeUm,
    double SensorWidthMm,
    double SensorHeightMm,

    // Sensor performance
    double QuantumEfficiencyPercent,
    double ReadNoiseElectrons,
    double DarkCurrentElectronsPerPixelPerSecond,
    double FullWellCapacityElectrons,

    // Electronics
    int BitDepth,
    double Gain,
    double Offset,

    // Cooling
    double? SensorTemperatureC
)
{
    public int Id { get; init; }

    public CameraItem(
        int Id,
        string Name,
        int ResolutionWidthPx,
        int ResolutionHeightPx,
        double PixelSizeUm,
        double SensorWidthMm,
        double SensorHeightMm,
        double QuantumEfficiencyPercent,
        double ReadNoiseElectrons,
        double DarkCurrentElectronsPerPixelPerSecond,
        double FullWellCapacityElectrons,
        int BitDepth,
        double Gain,
        double Offset,
        double? SensorTemperatureC)
        : this(
            Name,
            ResolutionWidthPx,
            ResolutionHeightPx,
            PixelSizeUm,
            SensorWidthMm,
            SensorHeightMm,
            QuantumEfficiencyPercent,
            ReadNoiseElectrons,
            DarkCurrentElectronsPerPixelPerSecond,
            FullWellCapacityElectrons,
            BitDepth,
            Gain,
            Offset,
            SensorTemperatureC)
    {
        this.Id = Id;
    }
}
