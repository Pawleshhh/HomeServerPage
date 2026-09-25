namespace HomeServerPage.Data.Weather;

public sealed class MockWeatherService : IWeatherService
{
    public Task<WeatherSnapshot> GetCurrentAsync(CancellationToken cancellationToken = default)
    {
        var snapshot = new WeatherSnapshot(
            "Szczecin",
            DateTimeOffset.Now,
            18,
            17,
            64,
            12,
            "Partly cloudy");

        return Task.FromResult(snapshot);
    }
}
