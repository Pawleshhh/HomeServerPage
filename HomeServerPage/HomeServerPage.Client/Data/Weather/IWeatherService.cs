namespace HomeServerPage.Data.Weather;

public interface IWeatherService
{
    Task<WeatherSnapshot> GetCurrentAsync(CancellationToken cancellationToken = default);

    Task<WeatherForecast> GetForecastAsync(
        int days = 3,
        CancellationToken cancellationToken = default);

    Task<WeatherAstronomy> GetAstronomyAsync(
        DateOnly date,
        CancellationToken cancellationToken = default);
}
