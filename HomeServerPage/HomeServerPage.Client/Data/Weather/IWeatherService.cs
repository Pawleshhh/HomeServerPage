using HomeServerPage.Data.Locations;

namespace HomeServerPage.Data.Weather;

public interface IWeatherService
{
    Task<WeatherSnapshot> GetCurrentAsync(
        SavedLocation location,
        CancellationToken cancellationToken = default);

    Task<WeatherForecast> GetForecastAsync(
        SavedLocation location,
        int days = 3,
        CancellationToken cancellationToken = default);

    Task<WeatherAstronomy> GetAstronomyAsync(
        SavedLocation location,
        DateOnly date,
        CancellationToken cancellationToken = default);
}
