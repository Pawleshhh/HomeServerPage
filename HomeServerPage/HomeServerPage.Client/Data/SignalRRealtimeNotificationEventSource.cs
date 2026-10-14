using HomeServerPage.Data.Notifications;
using Microsoft.AspNetCore.SignalR.Client;

namespace HomeServerPage.Client.Data;

public sealed class SignalRRealtimeNotificationEventSource(
    HttpClient httpClient,
    ILogger<SignalRRealtimeNotificationEventSource> logger) : IRealtimeNotificationEventSource
{
    private HubConnection? connection;
    private CancellationTokenSource? lifetimeCancellation;
    private Task? connectionTask;

    public event Func<string, Task>? NotificationReceived;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (this.connection is not null)
        {
            return Task.CompletedTask;
        }

        if (httpClient.BaseAddress is null)
        {
            throw new InvalidOperationException("The HTTP client must have a base address to connect to the notifications hub.");
        }

        this.lifetimeCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        this.connection = new HubConnectionBuilder()
            .WithUrl(new Uri(httpClient.BaseAddress, RealtimeNotificationContract.HubPath))
            .WithAutomaticReconnect()
            .Build();
        this.connection.On<string>(
            RealtimeNotificationContract.NotificationMethod,
            this.NotifySubscribersAsync);
        this.connectionTask = this.MaintainConnectionAsync(this.connection, this.lifetimeCancellation.Token);

        return Task.CompletedTask;
    }

    public async Task StopAsync()
    {
        if (this.lifetimeCancellation is null)
        {
            return;
        }

        this.lifetimeCancellation.Cancel();

        if (this.connection is not null)
        {
            try
            {
                await this.connection.StopAsync();
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Could not stop the realtime notifications connection cleanly.");
            }
        }

        if (this.connectionTask is not null)
        {
            try
            {
                await this.connectionTask;
            }
            catch (OperationCanceledException)
            {
            }
        }

        if (this.connection is not null)
        {
            await this.connection.DisposeAsync();
        }

        this.lifetimeCancellation.Dispose();
        this.lifetimeCancellation = null;
        this.connection = null;
        this.connectionTask = null;
    }

    private async Task MaintainConnectionAsync(HubConnection hubConnection, CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await hubConnection.StartAsync(cancellationToken);
                var disconnected = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
                Func<Exception?, Task> onClosed = _ =>
                {
                    disconnected.TrySetResult();
                    return Task.CompletedTask;
                };
                hubConnection.Closed += onClosed;

                if (hubConnection.State == HubConnectionState.Disconnected)
                {
                    disconnected.TrySetResult();
                }

                await disconnected.Task.WaitAsync(cancellationToken);
                hubConnection.Closed -= onClosed;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Could not connect to the realtime notifications hub.");
            }

            await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
        }
    }

    private async Task NotifySubscribersAsync(string notificationType)
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
                    "A dashboard component failed to handle realtime notification {NotificationType}.",
                    notificationType);
            }
        }
    }
}
