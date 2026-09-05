using AstroCalc.Catalogs;
using AstroCalc.Coordinates;
using AstroCalc.Core;
using AstroCalc.Corrections;
using AstroCalc.SolarSystem;
using AstroCalc.Time;

namespace HomeServerPage.Data.Astronomy;

public sealed class ObservationPointVisibilityCalculator
{
    private static readonly TimeSpan DefaultSampleInterval = TimeSpan.FromMinutes(5);

    public IReadOnlyList<VisibleAstronomyObject> Calculate(
        ObservationPoint observationPoint,
        IEnumerable<MessierObject> messierObjects,
        DateTimeOffset fromUtc,
        DateTimeOffset toUtc,
        TimeSpan? sampleInterval = null,
        CancellationToken cancellationToken = default)
    {
        var startUtc = fromUtc.ToUniversalTime();
        var endUtc = toUtc.ToUniversalTime();

        if (endUtc <= startUtc)
        {
            throw new ArgumentException("The visibility range must end after it starts.", nameof(toUtc));
        }

        var interval = sampleInterval ?? DefaultSampleInterval;
        if (interval <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(sampleInterval), "The sample interval must be positive.");
        }

        var targets = CreateTargets(messierObjects);
        var horizon = observationPoint.Horizon;
        var observations = new List<VisibleAstronomyObject>(targets.Count);

        foreach (var target in targets)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var windows = CalculateWindows(
                target,
                observationPoint.Coordinates,
                horizon,
                startUtc,
                endUtc,
                interval,
                cancellationToken);

            if (windows.Count > 0)
            {
                observations.Add(new VisibleAstronomyObject(
                    target.Key,
                    target.Name,
                    target.Type,
                    windows));
            }
        }

        return observations;
    }

    private static List<VisibilityWindow> CalculateWindows(
        CelestialTarget target,
        GeographicCoordinate location,
        HorizonProfile horizon,
        DateTimeOffset startUtc,
        DateTimeOffset endUtc,
        TimeSpan sampleInterval,
        CancellationToken cancellationToken)
    {
        var windows = new List<VisibilityWindow>();
        var previous = Evaluate(target, location, horizon, startUtc);
        var openWindow = previous.IsVisible
            ? new OpenWindow(previous.TimeUtc, previous.Direction, previous.AltitudeDegrees)
            : null;
        var currentTime = startUtc;

        while (currentTime < endUtc)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var nextTime = Min(currentTime + sampleInterval, endUtc);
            var current = Evaluate(target, location, horizon, nextTime);

            if (openWindow is null)
            {
                if (current.IsVisible)
                {
                    var startTime = previous.IsVisible
                        ? previous.TimeUtc
                        : FindVisibilityBoundary(target, location, horizon, previous, current, true);
                    openWindow = new OpenWindow(startTime, current.Direction, current.AltitudeDegrees);
                }
            }
            else if (!current.IsVisible)
            {
                var endTime = previous.IsVisible
                    ? FindVisibilityBoundary(target, location, horizon, previous, current, false)
                    : current.TimeUtc;
                windows.Add(openWindow.Close(target, endTime));
                openWindow = null;
            }
            else if (current.Direction != openWindow.Direction)
            {
                var directionChange = FindDirectionBoundary(
                    target,
                    location,
                    horizon,
                    previous,
                    current,
                    openWindow.Direction);
                windows.Add(openWindow.Close(target, directionChange));
                openWindow = new OpenWindow(directionChange, current.Direction, current.AltitudeDegrees);
            }

            if (openWindow is not null)
            {
                openWindow.MaximumAltitudeDegrees = Math.Max(
                    openWindow.MaximumAltitudeDegrees,
                    current.AltitudeDegrees);
            }

            previous = current;
            currentTime = nextTime;
        }

        if (openWindow is not null)
        {
            windows.Add(openWindow.Close(target, endUtc));
        }

        return windows;
    }

    private static DateTimeOffset FindVisibilityBoundary(
        CelestialTarget target,
        GeographicCoordinate location,
        HorizonProfile horizon,
        VisibilitySample previous,
        VisibilitySample current,
        bool targetVisibility)
    {
        var left = previous.TimeUtc;
        var right = current.TimeUtc;

        for (var iteration = 0; iteration < 12; iteration++)
        {
            var midpoint = left + ((right - left) / 2);
            var sample = Evaluate(target, location, horizon, midpoint);

            if (sample.IsVisible == targetVisibility)
            {
                right = midpoint;
            }
            else
            {
                left = midpoint;
            }
        }

        return right;
    }

    private static DateTimeOffset FindDirectionBoundary(
        CelestialTarget target,
        GeographicCoordinate location,
        HorizonProfile horizon,
        VisibilitySample previous,
        VisibilitySample current,
        HorizonDirection previousDirection)
    {
        var left = previous.TimeUtc;
        var right = current.TimeUtc;

        for (var iteration = 0; iteration < 12; iteration++)
        {
            var midpoint = left + ((right - left) / 2);
            var sample = Evaluate(target, location, horizon, midpoint);

            if (sample.Direction == previousDirection)
            {
                left = midpoint;
            }
            else
            {
                right = midpoint;
            }
        }

        return right;
    }

    private static VisibilitySample Evaluate(
        CelestialTarget target,
        GeographicCoordinate location,
        HorizonProfile horizon,
        DateTimeOffset timeUtc)
    {
        var dateTime = timeUtc.UtcDateTime;
        var julianDate = JulianDate.FromDateTime(dateTime);
        var equatorial = target.GetEquatorialPosition(julianDate);
        var siderealTime = SiderealTime.LocalMeanSiderealTime(julianDate, location.Longitude);
        var horizontal = CoordinateTransform.EquatorialToHorizontal(
            equatorial,
            siderealTime,
            location.Latitude);
        var azimuthDegrees = horizontal.Azimuth.Degrees;
        var altitudeDegrees = horizontal.Altitude.Degrees;
        var direction = horizon.GetDirection(azimuthDegrees);
        var minimumAltitude = horizon.GetAltitudeAtAzimuth(azimuthDegrees);

        return new VisibilitySample(
            timeUtc,
            altitudeDegrees >= minimumAltitude,
            direction,
            altitudeDegrees);
    }

    private static List<CelestialTarget> CreateTargets(IEnumerable<MessierObject> messierObjects)
    {
        var targets = new List<CelestialTarget>
        {
            new("sun", "Sun", AstronomyObjectType.Sun, GetSunPosition),
            new("moon", "Moon", AstronomyObjectType.Moon, GetMoonPosition)
        };

        targets.AddRange(Enum.GetValues<Planet>()
            .Where(planet => planet != Planet.Earth)
            .Select(planet => new CelestialTarget(
                planet.ToString().ToLowerInvariant(),
                planet.ToString(),
                AstronomyObjectType.Planet,
                julianDate => GetPlanetPosition(planet, julianDate))));

        targets.AddRange(messierObjects.Select(messier => new CelestialTarget(
            $"M{messier.Number}",
            GetMessierName(messier),
            AstronomyObjectType.DeepSky,
            _ => messier.Position)));

        return targets;
    }

    private static EquatorialCoordinate GetSunPosition(JulianDate julianDate)
    {
        var solarPosition = SolarPosition.MediumPrecision(julianDate);
        return new EquatorialCoordinate(
            solarPosition.RightAscension,
            solarPosition.Declination);
    }

    private static EquatorialCoordinate GetMoonPosition(JulianDate julianDate)
    {
        var obliquity = Nutation.TrueObliquity(julianDate.CenturiesFromJ2000);
        return CoordinateTransform.EclipticToEquatorial(
            LunarPosition.Calculate(julianDate).Position,
            obliquity);
    }

    private static EquatorialCoordinate GetPlanetPosition(Planet planet, JulianDate julianDate)
    {
        var obliquity = Nutation.TrueObliquity(julianDate.CenturiesFromJ2000);
        return CoordinateTransform.EclipticToEquatorial(
            PlanetaryPosition.Calculate(planet, julianDate).GeocentricPosition,
            obliquity);
    }

    private static string GetMessierName(MessierObject messierObject) =>
        string.IsNullOrWhiteSpace(messierObject.CommonName)
            ? $"M{messierObject.Number}"
            : $"M{messierObject.Number} {messierObject.CommonName}";

    private static DateTimeOffset Min(DateTimeOffset left, DateTimeOffset right) =>
        left < right ? left : right;

    private sealed record CelestialTarget(
        string Key,
        string Name,
        AstronomyObjectType Type,
        Func<JulianDate, EquatorialCoordinate> GetEquatorialPosition);

    private sealed record VisibilitySample(
        DateTimeOffset TimeUtc,
        bool IsVisible,
        HorizonDirection Direction,
        double AltitudeDegrees);

    private sealed class OpenWindow(DateTimeOffset startUtc, HorizonDirection direction, double altitudeDegrees)
    {
        public DateTimeOffset StartUtc { get; } = startUtc;

        public HorizonDirection Direction { get; } = direction;

        public double MaximumAltitudeDegrees { get; set; } = altitudeDegrees;

        public VisibilityWindow Close(CelestialTarget target, DateTimeOffset endUtc) => new(
            target.Key,
            target.Name,
            target.Type,
            Direction,
            StartUtc,
            endUtc,
            MaximumAltitudeDegrees);
    }
}
