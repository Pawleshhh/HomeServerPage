namespace HomeServerPage.Data;

public class DateTimeService : IDateTimeService
{
    public DateTime Now => DateTime.Now;

    public DateTime UtcNow => DateTime.UtcNow;

    public DateOnly Today => DateOnly.FromDateTime(DateTime.Now);

    public Task SyncAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
}
