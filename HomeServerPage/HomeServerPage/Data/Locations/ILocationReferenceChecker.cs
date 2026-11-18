namespace HomeServerPage.Data.Locations;

public interface ILocationReferenceChecker
{
    Task<bool> IsReferencedByObservationPointAsync(
        int locationId,
        CancellationToken cancellationToken = default);
}
