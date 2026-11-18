using Bunit;
using HomeServerPage.Client.Pages.PublicTransport;
using HomeServerPage.Data.PublicTransport;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace HomeServerPage.Tests.Unit.Components;

public sealed class PublicTransportComponentTests
{
    [Test]
    public void RendersDepartureBoardReturnedByService()
    {
        var service = new Mock<IPublicTransportService>();
        service
            .Setup(item => item.GetDepartureBoardsAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync([CreateBoard()]);
        using var context = new BunitContext();
        context.Services.AddSingleton(service.Object);

        var component = context.Render<PublicTransport>();

        component.WaitForAssertion(() =>
        {
            Assert.That(component.Markup, Does.Contain("Lubomirskiego"));
            Assert.That(component.Markup, Does.Contain("Centrum"));
        });
        service.Verify(
            item => item.GetDepartureBoardsAsync(10, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public void ShowsRecoverableErrorWhenDepartureServiceFails()
    {
        var service = new Mock<IPublicTransportService>();
        service
            .Setup(item => item.GetDepartureBoardsAsync(10, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("offline"));
        using var context = new BunitContext();
        context.Services.AddSingleton(service.Object);

        var component = context.Render<PublicTransport>();

        component.WaitForAssertion(() =>
            Assert.That(component.Find("[role=alert]").TextContent, Does.Contain("Could not load")));
    }

    private static DepartureBoard CreateBoard() =>
        new(
            new Stop("Lubomirskiego", "12811"),
            [
                new Departure(
                    new Line(5, "5", "tram", "normal", "tram", false, false),
                    new Trip("service", "trip-1", "2026-11-18", new Headsign("Centrum", "City Centre"), 1, 1, "accessible", null),
                    new DepartureTime(
                        new DateTimeOffset(2026, 11, 18, 15, 10, 0, TimeSpan.Zero),
                        new DateTimeOffset(2026, 11, 18, 15, 11, 0, TimeSpan.Zero),
                        false,
                        true,
                        false),
                    false,
                    null)
            ],
            [],
            new DateTimeOffset(2026, 11, 18, 15, 0, 0, TimeSpan.Zero));
}
