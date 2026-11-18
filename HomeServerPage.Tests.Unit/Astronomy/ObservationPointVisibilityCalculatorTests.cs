using HomeServerPage.Data.Astronomy;

namespace HomeServerPage.Tests.Unit.Astronomy;

public sealed class ObservationPointVisibilityCalculatorTests
{
    private readonly ObservationPointVisibilityCalculator calculator = new();

    [Test]
    public void CalculateRejectsAnEmptyOrReversedTimeRange()
    {
        var start = new DateTimeOffset(2026, 1, 15, 18, 0, 0, TimeSpan.Zero);

        Assert.Throws<ArgumentException>(() =>
            this.calculator.Calculate(new ObservationPoint(), [], start, start));
        Assert.Throws<ArgumentException>(() =>
            this.calculator.Calculate(new ObservationPoint(), [], start.AddHours(1), start));
    }

    [Test]
    public void CalculateRejectsNonPositiveSampleIntervals()
    {
        var start = new DateTimeOffset(2026, 1, 15, 18, 0, 0, TimeSpan.Zero);

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            this.calculator.Calculate(
                new ObservationPoint(),
                [],
                start,
                start.AddHours(1),
                TimeSpan.Zero));
    }

    [Test]
    public void CalculateHonorsCancellationBeforeComputingTargets()
    {
        var start = new DateTimeOffset(2026, 1, 15, 18, 0, 0, TimeSpan.Zero);
        using var cancellationSource = new CancellationTokenSource();
        cancellationSource.Cancel();

        Assert.Throws<OperationCanceledException>(() =>
            this.calculator.Calculate(
                new ObservationPoint(),
                [],
                start,
                start.AddHours(1),
                cancellationToken: cancellationSource.Token));
    }
}
