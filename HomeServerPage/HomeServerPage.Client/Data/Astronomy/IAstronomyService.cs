namespace HomeServerPage.Data.Astronomy;

public interface IAstronomyService
{
    Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default);
}
