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
            "mist" or "fog" or "freezing fog" => "bi-cloud-haze-fill",
            "patchy rain nearby" or "patchy light rain" or "light rain" or "moderate rain"
                or "heavy rain" or "rain" or "rainy" or "showers" => "bi-cloud-rain-fill",
            "patchy snow nearby" or "light snow" or "moderate snow" or "heavy snow"
                or "snow" or "blowing snow" => "bi-cloud-snow-fill",
            "thundery outbreaks possible" or "patchy light rain with thunder"
                or "moderate or heavy rain with thunder" or "thunderstorm" => "bi-cloud-lightning-rain-fill",
            _ => "bi-question-circle-fill"
        };
    }
}
