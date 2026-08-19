using HomeServerPage.Client.Data.Astronomy.Telescopes;

namespace HomeServerPage.Data.Astronomy.Telescopes;

public class TelescopeService : ITelescopeService
{

    public Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(true);
    }

}
