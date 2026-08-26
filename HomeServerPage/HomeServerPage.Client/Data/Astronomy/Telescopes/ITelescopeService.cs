namespace HomeServerPage.Client.Data.Astronomy.Telescopes;

public interface ITelescopeService
{
    Task<IEnumerable<TelescopeItem>> GetTelescopesAsync(CancellationToken cancellationToken = default);

    Task<IEnumerable<TelescopeEyepiece>> GetEyepiecesAsync(CancellationToken cancellationToken = default);

    Task<IEnumerable<TelescopeLens>> GetLensesAsync(CancellationToken cancellationToken = default);

    Task<IEnumerable<SensorItem>> GetSensorsAsync(CancellationToken cancellationToken = default);

    Task<IEnumerable<DeepSkyObject>> GetDeepSkyObjectsAsync(CancellationToken cancellationToken = default);

    Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default);

}
