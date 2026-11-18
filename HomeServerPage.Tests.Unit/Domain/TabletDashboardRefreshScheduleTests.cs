using HomeServerPage.Client.Data;
using HomeServerPage.Data.Astronomy;

namespace HomeServerPage.Tests.Unit.Domain;

public sealed class TabletDashboardRefreshScheduleTests
{
    [Test]
    public void DefaultSettingsAreValidAndActiveUntilIsExclusive()
    {
        var settings = new TabletDashboardRefreshOptions();

        Assert.That(settings.IsValid, Is.True);
        Assert.That(
            settings.TryGetActiveRange(out var activeFrom, out var activeUntil),
            Is.True);
        Assert.That(activeFrom, Is.EqualTo(new TimeOnly(7, 0)));
        Assert.That(activeUntil, Is.EqualTo(new TimeOnly(23, 0)));
        Assert.That(
            TabletDashboardRefreshSchedule.IsWithinActiveRange(
                InPoland(2026, 1, 15, 7, 0),
                activeFrom,
                activeUntil),
            Is.True);
        Assert.That(
            TabletDashboardRefreshSchedule.IsWithinActiveRange(
                InPoland(2026, 1, 15, 23, 0),
                activeFrom,
                activeUntil),
            Is.False);
    }

    [TestCase(0)]
    [TestCase(1441)]
    public void SettingsRejectIntervalsOutsideSupportedRange(int intervalMinutes)
    {
        var settings = new TabletDashboardRefreshOptions();
        settings.Widgets.WeatherIntervalMinutes = intervalMinutes;

        Assert.That(settings.IsValid, Is.False);
    }

    [TestCase("7:00", "23:00")]
    [TestCase("23:00", "07:00")]
    [TestCase("24:00", "23:00")]
    public void SettingsRejectInvalidOrOvernightActiveRanges(string activeFrom, string activeUntil)
    {
        var settings = new TabletDashboardRefreshOptions
        {
            ActiveFrom = activeFrom,
            ActiveUntil = activeUntil
        };

        Assert.That(settings.IsValid, Is.False);
    }

    [Test]
    public void NextRefreshBeforeStartIsAnchoredToActiveFrom()
    {
        var next = TabletDashboardRefreshSchedule.GetNextRefreshTime(
            InPoland(2026, 1, 15, 6, 59),
            20,
            new TimeOnly(7, 0),
            new TimeOnly(23, 0));

        Assert.That(next, Is.EqualTo(InPoland(2026, 1, 15, 7, 0)));
    }

    [Test]
    public void NextRefreshAtAnIntervalBoundaryUsesTheFollowingInterval()
    {
        var next = TabletDashboardRefreshSchedule.GetNextRefreshTime(
            InPoland(2026, 1, 15, 7, 20),
            20,
            new TimeOnly(7, 0),
            new TimeOnly(23, 0));

        Assert.That(next, Is.EqualTo(InPoland(2026, 1, 15, 7, 40)));
    }

    [TestCase(22, 50)]
    [TestCase(23, 0)]
    public void NextRefreshAtOrPastActiveUntilMovesToNextDay(
        int hour,
        int minute)
    {
        var next = TabletDashboardRefreshSchedule.GetNextRefreshTime(
            InPoland(2026, 1, 15, hour, minute),
            20,
            new TimeOnly(7, 0),
            new TimeOnly(23, 0));

        Assert.That(next, Is.EqualTo(InPoland(2026, 1, 16, 7, 0)));
    }

    private static DateTimeOffset InPoland(int year, int month, int day, int hour, int minute)
    {
        var localTime = new DateTime(year, month, day, hour, minute, 0, DateTimeKind.Unspecified);
        return new DateTimeOffset(
            localTime,
            AstronomyDefaults.LocalTimeZone.GetUtcOffset(localTime));
    }
}
