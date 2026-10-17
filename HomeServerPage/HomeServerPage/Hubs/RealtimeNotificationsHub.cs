using Microsoft.AspNetCore.SignalR;

namespace HomeServerPage.Hubs;

public sealed class RealtimeNotificationsHub : Hub
{
    public Task Ping() => Task.CompletedTask;
}
