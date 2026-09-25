using System.Net.Http.Json;

namespace HomeServerPage.Data.Weather;

public sealed class WeatherHttpService(HttpClient httpClient) : IWeatherService
{
    public async Task<WeatherSnapshot> GetCurrentAsync(CancellationToken cancellationToken = default)
    {
        return (await httpClient.GetFromJsonAsync<WeatherSnapshot>(
            "api/weather/current",
            cancellationToken))!;
    }

    public async Task<WeatherForecast> GetForecastAsync(
        int days = 3,
        CancellationToken cancellationToken = default)
    {
        return (await httpClient.GetFromJsonAsync<WeatherForecast>(
            $"api/weather/forecast?days={Math.Clamp(days, 1, 14)}",
            cancellationToken))!;
    }

    public async Task<WeatherAstronomy> GetAstronomyAsync(
        DateOnly date,
        CancellationToken cancellationToken = default)
    {
        return (await httpClient.GetFromJsonAsync<WeatherAstronomy>(
            $"api/weather/astronomy?date={date:yyyy-MM-dd}",
            cancellationToken))!;
    }
}
