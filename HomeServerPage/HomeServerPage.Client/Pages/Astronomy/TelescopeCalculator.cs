using AstroCalc.Optics;
using HomeServerPage.Client.Data.Astronomy.Telescopes;

namespace HomeServerPage.Client.Pages.Astronomy;

public class TelescopeCalculator
{

    public double CalculateTFov(
        TelescopeItem telescope,
        TelescopeEyepiece eyePiece,
        TelescopeLens? lens = null,
        double? additionalFocalLength = null)
    {
        var effectiveFl = EffectiveFocalLength(telescope, lens, additionalFocalLength);
        var magnification = Telescope.Magnification(effectiveFl, eyePiece.FocalLength);

        return Telescope.TrueFieldOfView(eyePiece.FieldOfView, magnification);
    }

    public AstrofotoSetupFov CalculateAstrofotoFov(
        TelescopeItem telescope,
        SensorItem sensor,
        TelescopeLens? lens = null,
        double? additionalFocalLength = null)
    {
        var effectiveFl = EffectiveFocalLength(telescope, lens, additionalFocalLength);

        double CalculateDimension(double sensorSize)
            => 2 * Math.Atan(sensorSize / (2d * effectiveFl)) * (180d / Math.PI);

        return new(
            CalculateDimension(sensor.SensorWidthMm),
            CalculateDimension(sensor.SensorHeightMm));
    }

    private static double EffectiveFocalLength(
        TelescopeItem telescope,
        TelescopeLens? lens = null,
        double? additionalFocalLength = null)
    {
        return telescope.FocalLength * (lens?.Multiplier ?? 1) + (additionalFocalLength ?? 0);
    }
}

public record AstrofotoSetupFov(double Width, double Height);