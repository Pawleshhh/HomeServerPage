using HomeServerPage.Data.Astronomy;

namespace HomeServerPage.Data.Astronomy;

public class AstronomyService : IAstronomyService
{
    public Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(true);
    }
}
