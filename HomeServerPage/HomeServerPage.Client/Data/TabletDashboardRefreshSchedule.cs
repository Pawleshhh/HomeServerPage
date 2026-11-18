using HomeServerPage.Data.Astronomy;

namespace HomeServerPage.Client.Data;

internal static class TabletDashboardRefreshSchedule
{
    public static DateTimeOffset GetNextRefreshTime(
        DateTimeOffset nowInPoland,
        int intervalMinutes,
        TimeOnly activeFrom,
        TimeOnly activeUntil)
    {
        var localDate = DateTime.SpecifyKind(nowInPoland.Date, DateTimeKind.Unspecified);
        var activeStart = localDate.Add(activeFrom.ToTimeSpan());
        var activeEnd = localDate.Add(activeUntil.ToTimeSpan());
        var localNow = nowInPoland.DateTime;
        DateTime nextRefresh;

        if (localNow < activeStart)
        {
            nextRefresh = activeStart;
        }
        else if (localNow >= activeEnd)
        {
            nextRefresh = localDate.AddDays(1).Add(activeFrom.ToTimeSpan());
        }
        else
        {
            var interval = TimeSpan.FromMinutes(intervalMinutes);
            var elapsed = localNow - activeStart;
            var intervalsElapsed = elapsed.Ticks / interval.Ticks + 1;
            nextRefresh = activeStart.AddTicks(intervalsElapsed * interval.Ticks);

            if (nextRefresh >= activeEnd)
            {
                nextRefresh = localDate.AddDays(1).Add(activeFrom.ToTimeSpan());
            }
        }

        var offset = AstronomyDefaults.LocalTimeZone.GetUtcOffset(nextRefresh);
        return new DateTimeOffset(nextRefresh, offset);
    }

    public static bool IsWithinActiveRange(
        DateTimeOffset nowInPoland,
        TimeOnly activeFrom,
        TimeOnly activeUntil)
    {
        var time = TimeOnly.FromDateTime(nowInPoland.DateTime);
        return time >= activeFrom && time < activeUntil;
    }
}
