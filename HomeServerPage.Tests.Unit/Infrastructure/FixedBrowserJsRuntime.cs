using System.Text.Json;
using Microsoft.JSInterop;

namespace HomeServerPage.Tests.Unit.Infrastructure;

internal sealed class FixedBrowserJsRuntime(
    DateOnly date,
    TimeOnly time,
    int utcOffsetMinutes = 0) : IJSRuntime
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly DateOnly date = date;
    private readonly TimeOnly time = time;
    private readonly int utcOffsetMinutes = utcOffsetMinutes;

    public ValueTask<TValue> InvokeAsync<TValue>(
        string identifier,
        object?[]? args) =>
        this.InvokeAsync<TValue>(identifier, CancellationToken.None, args);

    public ValueTask<TValue> InvokeAsync<TValue>(
        string identifier,
        CancellationToken cancellationToken,
        object?[]? args)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (identifier == "import")
        {
            return ValueTask.FromResult((TValue)(object)new FixedBrowserJsObjectReference(
                this.date,
                this.time,
                utcOffsetMinutes));
        }

        if (identifier == "homeServerPageCalories.getLocalClock")
        {
            return ValueTask.FromResult(this.DeserializeClock<TValue>());
        }

        throw new JSException($"Unexpected JavaScript invocation '{identifier}'.");
    }

    private TValue DeserializeClock<TValue>()
    {
        var json = JsonSerializer.Serialize(
            new ClockPayload(
                this.date.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture),
                this.time.ToString("HH:mm", System.Globalization.CultureInfo.InvariantCulture)),
            JsonOptions);
        return JsonSerializer.Deserialize<TValue>(json, JsonOptions)!;
    }

    private sealed record ClockPayload(string Date, string Time);

    private sealed class FixedBrowserJsObjectReference(
        DateOnly date,
        TimeOnly time,
        int utcOffsetMinutes) : IJSObjectReference
    {
        public ValueTask<TValue> InvokeAsync<TValue>(
            string identifier,
            object?[]? args) =>
            this.InvokeAsync<TValue>(identifier, CancellationToken.None, args);

        public ValueTask<TValue> InvokeAsync<TValue>(
            string identifier,
            CancellationToken cancellationToken,
            object?[]? args)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (identifier == "getLocalClock")
            {
                var json = JsonSerializer.Serialize(
                    new ClockPayload(
                        date.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture),
                        time.ToString("HH:mm", System.Globalization.CultureInfo.InvariantCulture)),
                    JsonOptions);
                return ValueTask.FromResult(JsonSerializer.Deserialize<TValue>(json, JsonOptions)!);
            }

            if (identifier == "getOffsetMinutes")
            {
                return ValueTask.FromResult((TValue)(object)utcOffsetMinutes);
            }

            throw new JSException($"Unexpected module invocation '{identifier}'.");
        }

        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
}
