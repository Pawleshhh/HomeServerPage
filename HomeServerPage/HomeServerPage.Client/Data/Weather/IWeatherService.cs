namespace HomeServerPage.Data.Weather;

public interface IWeatherService
{
    Task<WeatherSnapshot> GetCurrentAsync(CancellationToken cancellationToken = default);
}
