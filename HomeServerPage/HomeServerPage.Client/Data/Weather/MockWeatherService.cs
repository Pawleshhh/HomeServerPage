namespace HomeServerPage.Data.Weather;

public sealed class MockWeatherService : IWeatherService
{
    private static readonly WeatherLocation Location = new(
        "Szczecin",
        "West Pomeranian",
        "Poland",
        53.4285m,
        14.5528m,
        "Europe/Warsaw",
        DateTimeOffset.Now);

    public Task<WeatherSnapshot> GetCurrentAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(CreateCurrent());
    }

    public Task<WeatherForecast> GetForecastAsync(
        int days = 3,
        CancellationToken cancellationToken = default)
    {
        var current = CreateCurrent();
        var forecastDays = Enumerable.Range(0, Math.Clamp(days, 1, 14))
            .Select(offset =>
            {
                var date = DateOnly.FromDateTime(DateTime.Today.AddDays(offset));
                return new WeatherForecastDay(
                    date,
                    20m + offset,
                    11m + offset,
                    15.5m + offset,
                    15m + offset,
                    22m,
                    offset % 2 == 0 ? 0.4m : 1.8m,
                    offset % 2 == 0 ? 20 : 55,
                    0,
                    65m,
                    10m,
                    4m,
                    new WeatherCondition("Partly cloudy", "https://cdn.weatherapi.com/weather/64x64/day/116.png", 1003),
                    new WeatherAstronomy("05:20 AM", "08:40 PM", "01:10 AM", "10:25 AM", "Waxing crescent", 35),
                    []);
            })
            .ToArray();

        return Task.FromResult(new WeatherForecast(current, forecastDays));
    }

    public Task<WeatherAstronomy> GetAstronomyAsync(
        DateOnly date,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new WeatherAstronomy(
            "05:20 AM",
            "08:40 PM",
            "01:10 AM",
            "10:25 AM",
            "Waxing crescent",
            35));
    }

    private static WeatherSnapshot CreateCurrent() =>
        new(
            Location,
            DateTimeOffset.Now,
            18.4m,
            17.8m,
            64,
            12.3m,
            "SW",
            18.5m,
            1014.2m,
            0m,
            42m,
            10m,
            3m,
            new WeatherCondition("Partly cloudy", "https://cdn.weatherapi.com/weather/64x64/day/116.png", 1003),
            new WeatherAirQuality(224.5m, 18.2m, 72.1m, 3.4m, 8.6m, 14.2m, 1, 2));
}
