namespace HomeServerPage.Data;

public interface IDateTimeService
{
    DateTime Now { get; }

    DateTime UtcNow { get; }

    DateOnly Today { get; }

    Task SyncAsync(CancellationToken cancellationToken = default);
}
