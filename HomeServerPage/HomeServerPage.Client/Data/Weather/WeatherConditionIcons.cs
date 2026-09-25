namespace HomeServerPage.Data.Weather;

public static class WeatherConditionIcons
{
    public static string GetIconClass(string? condition)
    {
        var normalizedCondition = condition?.Trim().ToLowerInvariant();

        return normalizedCondition switch
        {
            "clear" or "clear sky" => "bi-sun-fill",
            "partly cloudy" or "partly-cloudy" => "bi-cloud-sun-fill",
            "cloudy" or "overcast" => "bi-cloud-fill",
            "rain" or "rainy" or "showers" => "bi-cloud-rain-fill",
            _ => "bi-question-circle-fill"
        };
    }
}
