using System.Text;
using HomeServerPage.Client.Data;
using HomeServerPage.Data;
using HomeServerPage.Tests.Unit.Infrastructure;

namespace HomeServerPage.Tests.Unit.Services;

public sealed class ClientDateTimeServiceTests
{
    [Test]
    public void ValuesCannotBeReadBeforeSuccessfulSync()
    {
        using var httpClient = CreateClient(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
        {
            Content = new StringContent("null", Encoding.UTF8, "application/json")
        });
        var service = new ClientDateTimeService(httpClient);

        Assert.Throws<InvalidOperationException>(() => _ = service.Now);
        Assert.Throws<InvalidOperationException>(() => _ = service.UtcNow);
        Assert.Throws<InvalidOperationException>(() => _ = service.Today);
    }

    [Test]
    public async Task SyncMakesServerClockValuesAvailable()
    {
        var response = """
            {"serverNow":"2026-11-18T16:30:00","serverUtcNow":"2026-11-18T15:30:00Z"}
            """;
        using var httpClient = CreateClient(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
        {
            Content = new StringContent(response, Encoding.UTF8, "application/json")
        });
        var service = new ClientDateTimeService(httpClient);

        await service.SyncAsync();

        Assert.That(service.Now, Is.GreaterThanOrEqualTo(new DateTime(2026, 11, 18, 16, 30, 0)));
        Assert.That(service.UtcNow, Is.GreaterThanOrEqualTo(new DateTime(2026, 11, 18, 15, 30, 0, DateTimeKind.Utc)));
        Assert.That(service.Today, Is.EqualTo(new DateOnly(2026, 11, 18)));
    }

    private static HttpClient CreateClient(HttpResponseMessage response) =>
        new(new StubHttpMessageHandler((_, _) => Task.FromResult(response)))
        {
            BaseAddress = new Uri("https://homeserver.test/")
        };
}
