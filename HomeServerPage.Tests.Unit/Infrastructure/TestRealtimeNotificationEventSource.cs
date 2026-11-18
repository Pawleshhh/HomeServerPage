using HomeServerPage.Data.Notifications;

namespace HomeServerPage.Tests.Unit.Infrastructure;

internal sealed class TestRealtimeNotificationEventSource : IRealtimeNotificationEventSource
{
    public event Func<string, Task>? NotificationReceived;

    public bool IsStarted { get; private set; }

    public bool IsStopped { get; private set; }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        this.IsStarted = true;
        return Task.CompletedTask;
    }

    public Task StopAsync()
    {
        this.IsStopped = true;
        return Task.CompletedTask;
    }

    public async Task PublishAsync(string notificationType)
    {
        if (this.NotificationReceived is null)
        {
            return;
        }

        foreach (var handler in this.NotificationReceived.GetInvocationList().Cast<Func<string, Task>>())
        {
            await handler(notificationType);
        }
    }
}
