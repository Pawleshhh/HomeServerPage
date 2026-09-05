namespace HomeServerPage.Data.Astronomy;

public enum HorizonDirection
{
    North,
    NorthEast,
    East,
    SouthEast,
    South,
    SouthWest,
    West,
    NorthWest
}

public readonly record struct HorizonProfile(
    double North,
    double NorthEast,
    double East,
    double SouthEast,
    double South,
    double SouthWest,
    double West,
    double NorthWest)
{
    public static HorizonProfile Flat { get; } = new();

    public static HorizonProfile FromObservationPoint(ObservationPoint observationPoint) => new(
        observationPoint.HorizonNorth,
        observationPoint.HorizonNorthEast,
        observationPoint.HorizonEast,
        observationPoint.HorizonSouthEast,
        observationPoint.HorizonSouth,
        observationPoint.HorizonSouthWest,
        observationPoint.HorizonWest,
        observationPoint.HorizonNorthWest);

    public double GetAltitude(HorizonDirection direction) => direction switch
    {
        HorizonDirection.North => North,
        HorizonDirection.NorthEast => NorthEast,
        HorizonDirection.East => East,
        HorizonDirection.SouthEast => SouthEast,
        HorizonDirection.South => South,
        HorizonDirection.SouthWest => SouthWest,
        HorizonDirection.West => West,
        HorizonDirection.NorthWest => NorthWest,
        _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
    };

    public HorizonDirection GetDirection(double azimuthDegrees)
    {
        var normalizedAzimuth = NormalizeAzimuth(azimuthDegrees);
        var sector = (int)Math.Round(normalizedAzimuth / 45, MidpointRounding.AwayFromZero) % 8;
        return (HorizonDirection)sector;
    }

    public double GetAltitudeAtAzimuth(double azimuthDegrees)
    {
        var normalizedAzimuth = NormalizeAzimuth(azimuthDegrees);
        var sector = normalizedAzimuth / 45;
        var lowerSector = (int)Math.Floor(sector);
        var upperSector = (lowerSector + 1) % 8;
        var fraction = sector - lowerSector;
        var lowerAltitude = GetAltitude((HorizonDirection)lowerSector);
        var upperAltitude = GetAltitude((HorizonDirection)upperSector);

        return lowerAltitude + ((upperAltitude - lowerAltitude) * fraction);
    }

    private static double NormalizeAzimuth(double azimuthDegrees)
    {
        var normalizedAzimuth = azimuthDegrees % 360;
        return normalizedAzimuth < 0 ? normalizedAzimuth + 360 : normalizedAzimuth;
    }
}
