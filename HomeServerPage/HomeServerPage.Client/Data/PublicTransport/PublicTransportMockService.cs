namespace HomeServerPage.Data.PublicTransport;

/// <summary>
/// Provides fake departure board data for local development/testing so the real ZDiTM API is not called.
/// </summary>
public class PublicTransportMockService(IDateTimeService dateTimeService) : IPublicTransportService
{
    private static readonly (int Number, int StopNumber)[] Stops = [(12811, 1), (12812, 2)];

    public Task<DepartureBoard> GetDepartureBoardAsync(int stopNumber, int limit = 10, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(CreateBoard(stopNumber, limit));
    }

    public async Task<List<DepartureBoard>> GetDepartureBoardsAsync(int limit = 10, CancellationToken cancellationToken = default)
    {
        var boards = new List<DepartureBoard>();
        foreach (var (stopNumber, _) in Stops)
        {
            boards.Add(await GetDepartureBoardAsync(stopNumber, limit, cancellationToken));
        }

        return boards;
    }

    private DepartureBoard CreateBoard(int stopNumber, int limit)
    {
        var now = dateTimeService.Now;
        var stop = new Stop("Lubomirskiego", stopNumber.ToString());

        var timetable = new (string Line, string VehicleType, string Headsign, int MinutesFromNow, bool RealTime)[]
        {
            ("8", "TRAM", "Gumieńce", 2, true),
            ("11", "TRAM", "Krzekowo", 5, true),
            ("53", "BUS", "Kijewo", 7, false),
            ("8", "TRAM", "Pomorzany", 12, true),
            ("60", "BUS", "Głębokie", 15, false),
            ("11", "TRAM", "Basen Górniczy", 19, true),
            ("53", "BUS", "Turkusowa", 24, false),
        };

        var departures = timetable.Take(limit).Select((t, index) =>
        {
            var scheduled = now.AddMinutes(t.MinutesFromNow);
            var estimated = t.RealTime ? scheduled.AddMinutes(-0.5) : scheduled;

            return new Departure(
                new Line(index, t.Line, "DAY", "NORMAL", t.VehicleType, false, false),
                new Trip(
                    $"{t.Line}-{stopNumber}",
                    null,
                    now.ToString("yyyy-MM-dd"),
                    new Headsign(t.Headsign, t.Headsign),
                    0,
                    1,
                    "LOW_FLOOR",
                    null),
                new DepartureTime(scheduled, estimated, false, t.RealTime, false),
                false,
                null);
        }).ToList();

        return new DepartureBoard(stop, departures, [], now);
    }
}
