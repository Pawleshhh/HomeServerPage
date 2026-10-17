namespace HomeServerPage.Data.Notifications;

public interface IRealtimeNotificationEventSource
{
    event Func<string, Task>? NotificationReceived;

    Task StartAsync(CancellationToken cancellationToken);

    Task StopAsync();
}

public interface IRealtimeNotificationPublisher
{
    Task PublishAsync(string notificationType, CancellationToken cancellationToken = default);
}

public static class RealtimeNotificationContract
{
    public const string HubPath = "/hubs/notifications";
    public const string NotificationMethod = "NotificationReceived";
}

public static class RealtimeNotificationTypes
{
    public const string FridgeChanged = "fridge.changed";
    public const string CaloriesChanged = "calories.changed";
    public const string ConnectionRestored = "realtime.connection-restored";
}
