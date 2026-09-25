namespace HomeServerPage.Data.Weather;

public sealed class WeatherApiOptions
{
    public string ApiKey { get; set; } = string.Empty;

    public string Location { get; set; } = "Szczecin";

    public int ForecastDays { get; set; } = 3;
}
