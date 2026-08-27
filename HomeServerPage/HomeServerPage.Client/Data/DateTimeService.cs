using System.Diagnostics;
using System.Net.Http.Json;

namespace HomeServerPage.Data;

/// <summary>
/// Client-side implementation of <see cref="IDateTimeService"/> that reflects the server's local time
/// rather than the browser's local clock. It syncs with the server once and then extrapolates elapsed
/// time locally using a <see cref="Stopwatch"/> to avoid a network round-trip per access.
/// </summary>
public class ClientDateTimeService(HttpClient httpClient) : IDateTimeService
{
    private readonly SemaphoreSlim syncLock = new(1, 1);
    private readonly Stopwatch stopwatch = new();
    private DateTime? serverNowAtSync;
    private DateTime? serverUtcNowAtSync;

    public DateTime Now => GetSyncedValue(this.serverNowAtSync);

    public DateTime UtcNow => GetSyncedValue(this.serverUtcNowAtSync);

    public DateOnly Today => DateOnly.FromDateTime(Now);

    public async Task SyncAsync(CancellationToken cancellationToken = default)
    {
        await this.syncLock.WaitAsync(cancellationToken);
        try
        {
            var result = await httpClient.GetFromJsonAsync<DateTimeSyncResult>(
                "api/datetime/sync",
                cancellationToken);

            if (result is null)
            {
                return;
            }

            this.serverNowAtSync = result.ServerNow;
            this.serverUtcNowAtSync = result.ServerUtcNow;
            this.stopwatch.Restart();
        }
        finally
        {
            this.syncLock.Release();
        }
    }

    private DateTime GetSyncedValue(DateTime? syncedValue)
    {
        if (syncedValue is null)
        {
            throw new InvalidOperationException(
                $"{nameof(ClientDateTimeService)} has not been synced with the server yet. Call {nameof(SyncAsync)} first.");
        }

        return syncedValue.Value + this.stopwatch.Elapsed;
    }
}
