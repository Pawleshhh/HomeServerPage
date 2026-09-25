namespace HomeServerPage.Data.Weather;

public record WeatherSnapshot(
    string Location,
    DateTimeOffset UpdatedAt,
    int TemperatureCelsius,
    int FeelsLikeCelsius,
    int HumidityPercent,
    int WindSpeedKilometersPerHour,
    string Condition);
