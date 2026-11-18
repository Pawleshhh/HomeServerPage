using System.Net.Http.Json;
using HomeServerPage.Data.Locations;

namespace HomeServerPage.Data.Weather;

public sealed class WeatherHttpService(HttpClient httpClient) : IWeatherService
{
    public async Task<WeatherSnapshot> GetCurrentAsync(
        SavedLocation location,
        CancellationToken cancellationToken = default)
    {
        return (await httpClient.GetFromJsonAsync<WeatherSnapshot>(
            $"api/weather/current?locationId={location.Id}",
            cancellationToken))!;
    }

    public async Task<WeatherForecast> GetForecastAsync(
        SavedLocation location,
        int days = 3,
        CancellationToken cancellationToken = default)
    {
        return (await httpClient.GetFromJsonAsync<WeatherForecast>(
            $"api/weather/forecast?locationId={location.Id}&days={Math.Clamp(days, 1, 14)}",
            cancellationToken))!;
    }

    public async Task<WeatherAstronomy> GetAstronomyAsync(
        SavedLocation location,
        DateOnly date,
        CancellationToken cancellationToken = default)
    {
        return (await httpClient.GetFromJsonAsync<WeatherAstronomy>(
            $"api/weather/astronomy?locationId={location.Id}&date={date:yyyy-MM-dd}",
            cancellationToken))!;
    }
}
