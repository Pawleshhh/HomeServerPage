using HomeServerPage.Data.Astronomy;

namespace HomeServerPage.Data.Astronomy;

public interface IObservationPointService
{
    Task<IReadOnlyList<ObservationPoint>> GetObservationPointsAsync(
        CancellationToken cancellationToken = default);

    Task<ObservationPoint> SaveObservationPointAsync(
        ObservationPoint observationPoint,
        CancellationToken cancellationToken = default);

    Task DeleteObservationPointAsync(
        int observationPointId,
        CancellationToken cancellationToken = default);
}
