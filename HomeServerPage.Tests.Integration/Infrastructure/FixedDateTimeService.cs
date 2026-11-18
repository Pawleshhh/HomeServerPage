using HomeServerPage.Data;

namespace HomeServerPage.Tests.Integration.Infrastructure;

internal sealed class FixedDateTimeService : IDateTimeService
{
    public DateTime UtcNow { get; set; } = new(2026, 11, 18, 15, 0, 0, DateTimeKind.Utc);

    public DateTime Now => this.UtcNow.ToLocalTime();

    public DateOnly Today => DateOnly.FromDateTime(this.Now);

    public Task SyncAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
}
