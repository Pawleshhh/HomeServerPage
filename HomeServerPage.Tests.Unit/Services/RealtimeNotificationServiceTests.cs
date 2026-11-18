using HomeServerPage.Data.Notifications;
using HomeServerPage.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace HomeServerPage.Tests.Unit.Services;

public sealed class RealtimeNotificationServiceTests
{
    [Test]
    public async Task PublishNotifiesLocalSubscribersAndConnectedClients()
    {
        const string notificationType = RealtimeNotificationTypes.FridgeChanged;
        var clientProxy = new Mock<IClientProxy>();
        clientProxy
            .Setup(proxy => proxy.SendCoreAsync(
                RealtimeNotificationContract.NotificationMethod,
                It.Is<object?[]>(arguments =>
                    arguments.Length == 1 &&
                    Equals(arguments[0], notificationType)),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        var clients = new Mock<IHubClients>();
        clients.SetupGet(hubClients => hubClients.All).Returns(clientProxy.Object);
        var hubContext = new Mock<IHubContext<RealtimeNotificationsHub>>();
        hubContext.SetupGet(context => context.Clients).Returns(clients.Object);
        var service = new RealtimeNotificationService(
            hubContext.Object,
            NullLogger<RealtimeNotificationService>.Instance);
        var received = new List<string>();
        service.NotificationReceived += type =>
        {
            received.Add(type);
            return Task.CompletedTask;
        };

        await service.PublishAsync(notificationType);

        Assert.That(received, Is.EqualTo(new[] { notificationType }));
        clientProxy.Verify(
            proxy => proxy.SendCoreAsync(
                RealtimeNotificationContract.NotificationMethod,
                It.IsAny<object?[]>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public void PublishRejectsAnEmptyNotificationType()
    {
        var service = new RealtimeNotificationService(
            Mock.Of<IHubContext<RealtimeNotificationsHub>>(),
            NullLogger<RealtimeNotificationService>.Instance);

        Assert.ThrowsAsync<ArgumentException>(() => service.PublishAsync(" "));
    }
}
