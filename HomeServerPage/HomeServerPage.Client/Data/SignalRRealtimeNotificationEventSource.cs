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
    private static readonly TimeSpan HealthCheckInterval = TimeSpan.FromSeconds(20);
    private static readonly TimeSpan HealthCheckTimeout = TimeSpan.FromSeconds(10);
    private static readonly TimeSpan ReconnectDelay = TimeSpan.FromSeconds(5);

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
                if (hubConnection.State != HubConnectionState.Connected)
                {
                    await hubConnection.StartAsync(cancellationToken);
                    logger.LogInformation("Connected to the realtime notifications hub.");
                    await this.NotifySubscribersAsync(RealtimeNotificationTypes.ConnectionRestored);
                }

                using var healthCheckCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                healthCheckCancellation.CancelAfter(HealthCheckTimeout);
                await hubConnection.InvokeAsync("Ping", healthCheckCancellation.Token);
                await Task.Delay(HealthCheckInterval, cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception exception)
            {
                logger.LogWarning(exception, "Realtime notifications hub health check failed; reconnecting.");
                try
                {
                    await hubConnection.StopAsync(cancellationToken);
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    return;
                }
                catch (Exception stopException)
                {
                    logger.LogWarning(stopException, "Could not stop the unhealthy realtime notifications connection.");
                }

                await Task.Delay(ReconnectDelay, cancellationToken);
            }
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
