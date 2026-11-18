namespace HomeServerPage.Data.Locations;

public sealed class LocationOperationCoordinator
{
    private readonly SemaphoreSlim gate = new(1, 1);

    public Task WaitAsync(CancellationToken cancellationToken) =>
        this.gate.WaitAsync(cancellationToken);

    public void Release() => this.gate.Release();
}
