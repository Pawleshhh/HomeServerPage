using HomeServerPage.Client.Data.Astronomy.Telescopes;
using HomeServerPage.Client.Pages.Astronomy;

namespace HomeServerPage.Tests.Unit.Domain;

public sealed class TelescopeCalculatorTests
{
    private readonly TelescopeCalculator calculator = new();

    [Test]
    public void TrueFieldOfViewUsesMagnificationAndLensMultiplier()
    {
        var telescope = new TelescopeItem("Scope", default, 200, 1000, 5);
        var eyepiece = new TelescopeEyepiece("Wide", 20, 60, default);
        var lens = new TelescopeLens("Barlow", 2, default);

        var withoutLens = this.calculator.CalculateTFov(telescope, eyepiece);
        var withLens = this.calculator.CalculateTFov(telescope, eyepiece, lens);

        Assert.That(withoutLens, Is.EqualTo(1.2).Within(0.001));
        Assert.That(withLens, Is.EqualTo(0.6).Within(0.001));
    }

    [Test]
    public void AstrophotographyFieldOfViewReflectsSensorDimensionsAndLens()
    {
        var telescope = new TelescopeItem("Scope", default, 200, 1000, 5);
        var sensor = new SensorItem("Sensor", 1920, 1080, 3.75, 22.3, 14.9);
        var lens = new TelescopeLens("Barlow", 2, default);

        var fov = this.calculator.CalculateAstrofotoFov(telescope, sensor, lens);

        Assert.That(fov.Width, Is.EqualTo(0.6388413231).Within(1e-9));
        Assert.That(fov.Height, Is.EqualTo(0.4268515831).Within(1e-9));
        Assert.That(fov.Width, Is.GreaterThan(fov.Height));
    }
}
