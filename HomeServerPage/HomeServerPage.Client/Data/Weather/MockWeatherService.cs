using HomeServerPage.Data.Locations;
using HomeServerPage.Data;

namespace HomeServerPage.Data.Weather;

public sealed class MockWeatherService(IDateTimeService dateTimeService) : IWeatherService
{
    public async Task<WeatherSnapshot> GetCurrentAsync(
        SavedLocation location,
        CancellationToken cancellationToken = default)
    {
        await dateTimeService.SyncAsync(cancellationToken);
        return CreateCurrent(location, GetUtcNow());
    }

    public async Task<WeatherForecast> GetForecastAsync(
        SavedLocation location,
        int days = 3,
        CancellationToken cancellationToken = default)
    {
        await dateTimeService.SyncAsync(cancellationToken);
        var utcNow = GetUtcNow();
        var current = CreateCurrent(location, utcNow);
        var timeZone = TimeZoneInfo.FindSystemTimeZoneById(location.TimeZoneId);
        var localDate = DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(utcNow, timeZone).DateTime);
        var forecastDays = Enumerable.Range(0, Math.Clamp(days, 1, 14))
            .Select(offset =>
            {
                var date = localDate.AddDays(offset);
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

        return new WeatherForecast(current, forecastDays);
    }

    public Task<WeatherAstronomy> GetAstronomyAsync(
        SavedLocation location,
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

    private static WeatherSnapshot CreateCurrent(SavedLocation location, DateTimeOffset utcNow)
    {
        var timeZone = TimeZoneInfo.FindSystemTimeZoneById(location.TimeZoneId);
        var weatherLocation = new WeatherLocation(
            location.Name,
            string.Empty,
            string.Empty,
            (decimal)location.Latitude,
            (decimal)location.Longitude,
            location.TimeZoneId,
            TimeZoneInfo.ConvertTime(utcNow, timeZone));

        return new WeatherSnapshot(
            weatherLocation,
            utcNow,
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

    private DateTimeOffset GetUtcNow()
    {
        var utcNow = DateTime.SpecifyKind(dateTimeService.UtcNow, DateTimeKind.Utc);
        return new DateTimeOffset(utcNow);
    }
}
