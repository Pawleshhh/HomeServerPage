namespace HomeServerPage.Data.PublicTransport;

public interface IPublicTransportService
{
    Task<DepartureBoard> GetDepartureBoardAsync(int stopNumber, int limit = 10, CancellationToken cancellationToken = default);

    Task<List<DepartureBoard>> GetDepartureBoardsAsync(int limit = 10, CancellationToken cancellationToken = default);
}
