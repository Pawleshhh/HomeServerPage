using HomeServerPage.Data;
using HomeServerPage.Data.Astronomy;
using HomeServerPage.Data.Notifications;

namespace HomeServerPage.Client.Data;

public sealed class TabletDashboardRefreshService(
    IDateTimeService dateTimeService,
    ITabletDashboardRefreshSettingsProvider settingsProvider,
    IRealtimeNotificationEventSource notificationEventSource,
    ILogger<TabletDashboardRefreshService> logger)
{
    public event Func<Task>? RefreshRequested;
    public event Func<string, Task>? NotificationReceived;

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        notificationEventSource.NotificationReceived += this.HandleNotificationReceivedAsync;
        try
        {
            await notificationEventSource.StartAsync(cancellationToken);
            var settings = await this.GetSettingsAsync(cancellationToken);
            if (!settings.TryGetActiveRange(out var activeFrom, out var activeUntil))
            {
                throw new InvalidOperationException("The tablet dashboard refresh settings are invalid.");
            }

            await this.SyncAndGetUtcNowAsync(cancellationToken);

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var nowUtc = this.GetUtcNow();
                var nowInPoland = TimeZoneInfo.ConvertTime(nowUtc, AstronomyDefaults.LocalTimeZone);
                var nextRefresh = GetNextRefreshTime(nowInPoland, settings, activeFrom, activeUntil);
                var delay = nextRefresh - nowUtc;

                if (delay > TimeSpan.Zero)
                {
                    await Task.Delay(delay, cancellationToken);
                }

                nowUtc = await this.SyncAndGetUtcNowAsync(cancellationToken);
                nowInPoland = TimeZoneInfo.ConvertTime(nowUtc, AstronomyDefaults.LocalTimeZone);

                if (nowUtc < nextRefresh || !IsWithinActiveRange(nowInPoland, activeFrom, activeUntil))
                {
                    continue;
                }

                await this.NotifySubscribersAsync(this.RefreshRequested);
            }
        }
        finally
        {
            notificationEventSource.NotificationReceived -= this.HandleNotificationReceivedAsync;
            await notificationEventSource.StopAsync();
        }
    }

    private Task HandleNotificationReceivedAsync(string notificationType) =>
        this.NotifySubscribersAsync(this.NotificationReceived, notificationType);

    private async Task<TabletDashboardRefreshOptions> GetSettingsAsync(CancellationToken cancellationToken)
    {
        while (true)
        {
            try
            {
                var settings = await settingsProvider.GetSettingsAsync(cancellationToken);
                if (!settings.IsValid)
                {
                    throw new InvalidOperationException("The tablet dashboard refresh settings are invalid.");
                }

                return settings;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Could not load the tablet dashboard refresh settings.");
                await Task.Delay(TimeSpan.FromMinutes(1), cancellationToken);
            }
        }
    }

    private async Task<DateTimeOffset> SyncAndGetUtcNowAsync(CancellationToken cancellationToken)
    {
        while (true)
        {
            try
            {
                await dateTimeService.SyncAsync(cancellationToken);
                return this.GetUtcNow();
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Could not synchronize the tablet dashboard refresh schedule.");
                await Task.Delay(TimeSpan.FromMinutes(1), cancellationToken);
            }
        }
    }

    private DateTimeOffset GetUtcNow()
    {
        var utcNow = DateTime.SpecifyKind(dateTimeService.UtcNow, DateTimeKind.Utc);
        return new DateTimeOffset(utcNow);
    }

    private static DateTimeOffset GetNextRefreshTime(
        DateTimeOffset nowInPoland,
        TabletDashboardRefreshOptions settings,
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
            var interval = TimeSpan.FromMinutes(settings.IntervalMinutes);
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

    private static bool IsWithinActiveRange(
        DateTimeOffset nowInPoland,
        TimeOnly activeFrom,
        TimeOnly activeUntil)
    {
        var time = TimeOnly.FromDateTime(nowInPoland.DateTime);
        return time >= activeFrom && time < activeUntil;
    }

    private async Task NotifySubscribersAsync(Func<Task>? subscribers)
    {
        if (subscribers is null)
        {
            return;
        }

        foreach (var handler in subscribers.GetInvocationList().Cast<Func<Task>>())
        {
            try
            {
                await handler();
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "A tablet dashboard component failed to refresh.");
            }
        }
    }

    private async Task NotifySubscribersAsync(
        Func<string, Task>? subscribers,
        string notificationType)
    {
        if (subscribers is null)
        {
            return;
        }

        foreach (var handler in subscribers.GetInvocationList().Cast<Func<string, Task>>())
        {
            try
            {
                await handler(notificationType);
            }
            catch (Exception exception)
            {
                logger.LogError(
                    exception,
                    "A tablet dashboard component failed to handle realtime notification {NotificationType}.",
                    notificationType);
            }
        }
    }
}
