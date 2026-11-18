using System.Net;
using System.Text;
using HomeServerPage.Data.PublicTransport;
using HomeServerPage.Tests.Unit.Infrastructure;

namespace HomeServerPage.Tests.Unit.Services;

public sealed class PublicTransportServiceTests
{
    private const string BoardResponse = """
        {
          "data": {
            "stop": { "name": "Lubomirskiego", "number": "12811" },
            "departures": [
              {
                "line": {
                  "id": 5,
                  "number": "5",
                  "type": "tram",
                  "subtype": "normal",
                  "vehicle_type": "tram",
                  "on_demand": false,
                  "disruption": false
                },
                "trip": {
                  "service": "weekday",
                  "gtfs_id": "trip-1",
                  "start_date": "2026-01-15",
                  "headsign": { "short": "Centrum", "long": "City Centre" },
                  "direction_id": 1,
                  "route_variant_number": 1,
                  "accessibility": "low-floor",
                  "note": null
                },
                "departure_time": {
                  "scheduled": "2026-01-15T12:10:00+01:00",
                  "estimated": "2026-01-15T12:11:00+01:00",
                  "departing_now": false,
                  "real_time": true,
                  "canceled": false
                },
                "request_stop": false,
                "vehicle": null
              }
            ],
            "messages": ["Service operating normally"],
            "updated_at": "2026-01-15T12:00:00+01:00"
          }
        }
        """;

    [Test]
    public async Task DepartureBoardMapsNestedDataAndOptionalValues()
    {
        HttpRequestMessage? capturedRequest = null;
        using var client = CreateClient((request, _) =>
        {
            capturedRequest = request;
            return Task.FromResult(JsonResponse(BoardResponse));
        });
        var service = new PublicTransportService(client);

        var board = await service.GetDepartureBoardAsync(12811, 3);

        Assert.That(board.Stop.Name, Is.EqualTo("Lubomirskiego"));
        Assert.That(board.Departures, Has.Count.EqualTo(1));
        Assert.That(board.Departures[0].Trip.Headsign.Short, Is.EqualTo("Centrum"));
        Assert.That(board.Departures[0].DepartureTime.RealTime, Is.True);
        Assert.That(board.Departures[0].Vehicle, Is.Null);
        Assert.That(board.Departures[0].Trip.Note, Is.Null);
        Assert.That(board.Messages, Does.Contain("Service operating normally"));
        Assert.That(
            capturedRequest!.RequestUri!.PathAndQuery,
            Is.EqualTo("/v2/departure-boards/12811?limit=3&format=json"));
    }

    [Test]
    public async Task CombinedBoardLoadsTheTwoConfiguredStopsInOrder()
    {
        var stops = new List<string>();
        using var client = CreateClient((request, _) =>
        {
            stops.Add(request.RequestUri!.AbsolutePath);
            return Task.FromResult(JsonResponse(BoardResponse));
        });
        var service = new PublicTransportService(client);

        var boards = await service.GetDepartureBoardsAsync();

        Assert.That(boards, Has.Count.EqualTo(2));
        Assert.That(stops, Is.EqualTo(new[]
        {
            "/v2/departure-boards/12811",
            "/v2/departure-boards/12812"
        }));
    }

    [Test]
    public void JsonNullResponseIsRejected()
    {
        using var client = CreateClient((_, _) => Task.FromResult(JsonResponse("null")));
        var service = new PublicTransportService(client);

        Assert.ThrowsAsync<InvalidOperationException>(
            async () => await service.GetDepartureBoardAsync(12811));
    }

    private static HttpClient CreateClient(
        Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> handler) =>
        new(new StubHttpMessageHandler(handler))
        {
            BaseAddress = new Uri("https://transport.test/v2/")
        };

    private static HttpResponseMessage JsonResponse(string json) => new(HttpStatusCode.OK)
    {
        Content = new StringContent(json, Encoding.UTF8, "application/json")
    };
}
