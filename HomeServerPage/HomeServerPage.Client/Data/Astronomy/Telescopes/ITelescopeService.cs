namespace HomeServerPage.Client.Data.Astronomy.Telescopes;

public interface ITelescopeService
{
    Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default);

}
