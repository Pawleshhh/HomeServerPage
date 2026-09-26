namespace HomeServerPage.Data.Weather;

public sealed record WeatherSnapshot(
    WeatherLocation Location,
    DateTimeOffset UpdatedAt,
    decimal TemperatureCelsius,
    decimal FeelsLikeCelsius,
    int HumidityPercent,
    decimal WindSpeedKilometersPerHour,
    string WindDirection,
    decimal GustSpeedKilometersPerHour,
    decimal PressureMillibars,
    decimal PrecipitationMillimeters,
    decimal CloudPercent,
    decimal VisibilityKilometers,
    decimal UvIndex,
    WeatherCondition Condition,
    WeatherAirQuality? AirQuality);

public sealed record WeatherForecast(
    WeatherSnapshot Current,
    IReadOnlyList<WeatherForecastDay> Days);

public sealed record WeatherForecastDay(
    DateOnly Date,
    decimal MaxTemperatureCelsius,
    decimal MinTemperatureCelsius,
    decimal AverageTemperatureCelsius,
    decimal AverageFeelsLikeCelsius,
    decimal MaxWindSpeedKilometersPerHour,
    decimal TotalPrecipitationMillimeters,
    int ChanceOfRainPercent,
    int ChanceOfSnowPercent,
    decimal AverageHumidityPercent,
    decimal AverageVisibilityKilometers,
    decimal UvIndex,
    WeatherCondition Condition,
    WeatherAstronomy Astronomy,
    IReadOnlyList<WeatherHourlyForecast> Hours);

public sealed record WeatherHourlyForecast(
    DateTimeOffset Time,
    decimal TemperatureCelsius,
    decimal FeelsLikeCelsius,
    decimal WindSpeedKilometersPerHour,
    decimal PrecipitationMillimeters,
    int ChanceOfRainPercent,
    int HumidityPercent,
    WeatherCondition Condition);

public sealed record WeatherLocation(
    string Name,
    string Region,
    string Country,
    decimal Latitude,
    decimal Longitude,
    string TimeZoneId,
    DateTimeOffset LocalTime);

public sealed record WeatherCondition(
    string Text,
    string IconUrl,
    int Code);

public sealed record WeatherAstronomy(
    string Sunrise,
    string Sunset,
    string Moonrise,
    string Moonset,
    string MoonPhase,
    int MoonIlluminationPercent);

public sealed record WeatherAirQuality(
    decimal CarbonMonoxide,
    decimal NitrogenDioxide,
    decimal Ozone,
    decimal SulphurDioxide,
    decimal Pm2Point5,
    decimal Pm10,
    int UsgEpaIndex,
    int GbDefraIndex);
