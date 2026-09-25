namespace HomeServerPage.Data.Weather;

public sealed class WeatherApiException(int statusCode, string message) : Exception(message)
{
    public int StatusCode { get; } = statusCode;
}
