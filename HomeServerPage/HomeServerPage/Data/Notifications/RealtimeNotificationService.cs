using HomeServerPage.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace HomeServerPage.Data.Notifications;

public sealed class RealtimeNotificationService(
    IHubContext<RealtimeNotificationsHub> hubContext,
    ILogger<RealtimeNotificationService> logger)
    : IRealtimeNotificationEventSource, IRealtimeNotificationPublisher
{
    public event Func<string, Task>? NotificationReceived;

    public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StopAsync() => Task.CompletedTask;

    public async Task PublishAsync(
        string notificationType,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(notificationType);
        await this.NotifyLocalSubscribersAsync(notificationType);

        try
        {
            await hubContext.Clients.All.SendAsync(
                RealtimeNotificationContract.NotificationMethod,
                notificationType,
                cancellationToken);
        }
        catch (Exception exception)
        {
            logger.LogError(
                exception,
                "Could not send realtime notification {NotificationType} to connected clients.",
                notificationType);
        }
    }

    private async Task NotifyLocalSubscribersAsync(string notificationType)
    {
        var handlers = this.NotificationReceived;
        if (handlers is null)
        {
            return;
        }

        foreach (var handler in handlers.GetInvocationList().Cast<Func<string, Task>>())
        {
            try
            {
                await handler(notificationType);
            }
            catch (Exception exception)
            {
                logger.LogError(
                    exception,
                    "A local dashboard failed to handle realtime notification {NotificationType}.",
                    notificationType);
            }
        }
    }
}
