namespace HomeServerPage.Data.PublicTransport;

public class PublicTransportService(HttpClient httpClient) : IPublicTransportService
{
    private static readonly int[] StopNumbers = [12811, 12812];

    public async Task<DepartureBoard> GetDepartureBoardAsync(int stopNumber, int limit = 10, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.GetFromJsonAsync<DepartureBoardResponse>(
            $"departure-boards/{stopNumber}?limit={limit}&format=json",
            cancellationToken);

        if (response is null)
        {
            throw new InvalidOperationException($"No data returned for stop {stopNumber}.");
        }

        return MapToDepartureBoard(response.Data);
    }

    public async Task<List<DepartureBoard>> GetDepartureBoardsAsync(int limit = 10, CancellationToken cancellationToken = default)
    {
        var boards = new List<DepartureBoard>();
        foreach (var stopNumber in StopNumbers)
        {
            boards.Add(await GetDepartureBoardAsync(stopNumber, limit, cancellationToken));
        }

        return boards;
    }

    private static DepartureBoard MapToDepartureBoard(DepartureBoardData data)
    {
        var stop = new Stop(data.Stop.Name, data.Stop.Number);

        var departures = data.Departures.ConvertAll(d => new Departure(
            new Line(
                d.Line.Id,
                d.Line.Number,
                d.Line.Type,
                d.Line.Subtype,
                d.Line.VehicleType,
                d.Line.OnDemand,
                d.Line.Disruption),
            new Trip(
                d.Trip.Service,
                d.Trip.GtfsId,
                d.Trip.StartDate,
                new Headsign(d.Trip.Headsign.Short, d.Trip.Headsign.Long),
                d.Trip.DirectionId,
                d.Trip.RouteVariantNumber,
                d.Trip.Accessibility,
                d.Trip.Note is null ? null : new Note(d.Trip.Note.Pl, d.Trip.Note.En, d.Trip.Note.De, d.Trip.Note.Uk)),
            new DepartureTime(
                d.DepartureTime.Scheduled,
                d.DepartureTime.Estimated,
                d.DepartureTime.DepartingNow,
                d.DepartureTime.RealTime,
                d.DepartureTime.Canceled),
            d.RequestStop,
            d.Vehicle is null
                ? null
                : new Vehicle(
                    d.Vehicle.Id,
                    d.Vehicle.Number,
                    d.Vehicle.Model,
                    d.Vehicle.LowFloor,
                    d.Vehicle.TicketMachine is null
                        ? null
                        : new TicketMachine(d.Vehicle.TicketMachine.Cards, d.Vehicle.TicketMachine.Coins),
                    d.Vehicle.Stuck)));

        return new DepartureBoard(stop, departures, data.Messages, data.UpdatedAt);
    }
}
