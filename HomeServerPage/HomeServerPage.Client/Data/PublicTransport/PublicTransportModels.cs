namespace HomeServerPage.Data.PublicTransport;

public record DepartureBoard(Stop Stop, List<Departure> Departures, List<string> Messages, DateTimeOffset UpdatedAt);

public record Stop(string Name, string Number);

public record Departure(Line Line, Trip Trip, DepartureTime DepartureTime, bool RequestStop, Vehicle? Vehicle);

public record Line(
    int Id,
    string Number,
    string Type,
    string Subtype,
    string VehicleType,
    bool OnDemand,
    bool Disruption);

public record Trip(
    string Service,
    string? GtfsId,
    string StartDate,
    Headsign Headsign,
    int DirectionId,
    int RouteVariantNumber,
    string Accessibility,
    Note? Note);

public record Headsign(string Short, string Long);

public record Note(string Pl, string En, string De, string Uk);

public record DepartureTime(
    DateTimeOffset Scheduled,
    DateTimeOffset Estimated,
    bool DepartingNow,
    bool RealTime,
    bool Canceled);

public record Vehicle(int Id, string Number, string? Model, bool? LowFloor, TicketMachine? TicketMachine, bool Stuck);

public record TicketMachine(bool Cards, bool Coins);
