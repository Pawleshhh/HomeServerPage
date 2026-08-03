using System.Text.Json.Serialization;

namespace HomeServerPage.Data.PublicTransport;

internal record DepartureBoardResponse([property: JsonPropertyName("data")] DepartureBoardData Data);

internal record DepartureBoardData(
    [property: JsonPropertyName("stop")] StopDto Stop,
    [property: JsonPropertyName("departures")] List<DepartureDto> Departures,
    [property: JsonPropertyName("messages")] List<string> Messages,
    [property: JsonPropertyName("updated_at")] DateTimeOffset UpdatedAt);

internal record StopDto(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("number")] string Number);

internal record DepartureDto(
    [property: JsonPropertyName("line")] LineDto Line,
    [property: JsonPropertyName("trip")] TripDto Trip,
    [property: JsonPropertyName("departure_time")] DepartureTimeDto DepartureTime,
    [property: JsonPropertyName("request_stop")] bool RequestStop,
    [property: JsonPropertyName("vehicle")] VehicleDto? Vehicle);

internal record LineDto(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("number")] string Number,
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("subtype")] string Subtype,
    [property: JsonPropertyName("vehicle_type")] string VehicleType,
    [property: JsonPropertyName("on_demand")] bool OnDemand,
    [property: JsonPropertyName("disruption")] bool Disruption);

internal record TripDto(
    [property: JsonPropertyName("service")] string Service,
    [property: JsonPropertyName("gtfs_id")] string? GtfsId,
    [property: JsonPropertyName("start_date")] string StartDate,
    [property: JsonPropertyName("headsign")] HeadsignDto Headsign,
    [property: JsonPropertyName("direction_id")] int DirectionId,
    [property: JsonPropertyName("route_variant_number")] int RouteVariantNumber,
    [property: JsonPropertyName("accessibility")] string Accessibility,
    [property: JsonPropertyName("note")] NoteDto? Note);

internal record HeadsignDto(
    [property: JsonPropertyName("short")] string Short,
    [property: JsonPropertyName("long")] string Long);

internal record NoteDto(
    [property: JsonPropertyName("pl")] string Pl,
    [property: JsonPropertyName("en")] string En,
    [property: JsonPropertyName("de")] string De,
    [property: JsonPropertyName("uk")] string Uk);

internal record DepartureTimeDto(
    [property: JsonPropertyName("scheduled")] DateTimeOffset Scheduled,
    [property: JsonPropertyName("estimated")] DateTimeOffset Estimated,
    [property: JsonPropertyName("departing_now")] bool DepartingNow,
    [property: JsonPropertyName("real_time")] bool RealTime,
    [property: JsonPropertyName("canceled")] bool Canceled);

internal record VehicleDto(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("number")] string Number,
    [property: JsonPropertyName("model")] string? Model,
    [property: JsonPropertyName("low_floor")] bool? LowFloor,
    [property: JsonPropertyName("ticket_machine")] TicketMachineDto? TicketMachine,
    [property: JsonPropertyName("stuck")] bool Stuck);

internal record TicketMachineDto(
    [property: JsonPropertyName("cards")] bool Cards,
    [property: JsonPropertyName("coins")] bool Coins);
