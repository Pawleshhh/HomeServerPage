using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;

namespace HomeServerPage.Data.Weather;

public sealed class WeatherApiService(
    HttpClient httpClient,
    IOptions<WeatherApiOptions> options) : IWeatherService
{
    private readonly WeatherApiOptions options = options.Value;

    public async Task<WeatherSnapshot> GetCurrentAsync(CancellationToken cancellationToken = default)
    {
        var response = await GetAsync(
            $"current.json?key={Uri.EscapeDataString(options.ApiKey)}&q={Uri.EscapeDataString(options.Location)}&aqi=yes",
            cancellationToken);

        return MapCurrent(response.Location, response.Current);
    }

    public async Task<WeatherForecast> GetForecastAsync(
        int days = 3,
        CancellationToken cancellationToken = default)
    {
        var forecastDays = Math.Clamp(
            days == 3 ? options.ForecastDays : days,
            1,
            14);
        var response = await GetAsync(
            $"forecast.json?key={Uri.EscapeDataString(options.ApiKey)}&q={Uri.EscapeDataString(options.Location)}&days={forecastDays}&aqi=yes&alerts=no",
            cancellationToken);

        var current = MapCurrent(response.Location, response.Current);
        var daysResult = response.Forecast?.ForecastDay?
            .Select(day => MapForecastDay(response.Location, day))
            .ToArray() ?? [];

        return new WeatherForecast(current, daysResult);
    }

    public async Task<WeatherAstronomy> GetAstronomyAsync(
        DateOnly date,
        CancellationToken cancellationToken = default)
    {
        var response = await GetAsync(
            $"astronomy.json?key={Uri.EscapeDataString(options.ApiKey)}&q={Uri.EscapeDataString(options.Location)}&dt={date:yyyy-MM-dd}",
            cancellationToken);

        return MapAstronomy(response.Astronomy?.Astro ?? new WeatherApiAstronomy());
    }

    private async Task<WeatherApiResponse> GetAsync(
        string requestUri,
        CancellationToken cancellationToken)
    {
        using var response = await httpClient.GetAsync(requestUri, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var statusCode = (int)response.StatusCode;
            var message = response.StatusCode switch
            {
                HttpStatusCode.Unauthorized => "The weather API token is invalid.",
                HttpStatusCode.Forbidden => "The weather API quota or access permissions do not allow this request.",
                HttpStatusCode.BadRequest => "The weather API rejected the configured location or request.",
                _ => "The weather API request failed."
            };

            throw new WeatherApiException(statusCode, message);
        }

        return (await response.Content.ReadFromJsonAsync<WeatherApiResponse>(
            cancellationToken: cancellationToken)) ?? throw new WeatherApiException(
                (int)response.StatusCode,
                "The weather API returned an empty response.");
    }

    private static WeatherSnapshot MapCurrent(
        WeatherApiLocation location,
        WeatherApiCurrent current) =>
        new(
            MapLocation(location),
            ParseDateTime(current.LastUpdated),
            current.TempC,
            current.FeelsLikeC,
            current.Humidity,
            current.WindKph,
            current.WindDir,
            current.GustKph,
            current.PressureMb,
            current.PrecipMm,
            current.Cloud,
            current.VisKm,
            current.Uv,
            MapCondition(current.Condition),
            MapAirQuality(current.AirQuality));

    private static WeatherForecastDay MapForecastDay(
        WeatherApiLocation location,
        WeatherApiForecastDay day) =>
        new(
            DateOnly.Parse(day.Date, CultureInfo.InvariantCulture),
            day.Day.MaxTempC,
            day.Day.MinTempC,
            day.Day.AvgTempC,
            day.Hour is { Count: > 0 }
                ? day.Hour.Average(hour => hour.FeelsLikeC)
                : day.Day.AvgTempC,
            day.Day.MaxWindKph,
            day.Day.TotalPrecipMm,
            day.Day.DailyChanceOfRain,
            day.Day.DailyChanceOfSnow,
            day.Day.AvgHumidity,
            day.Day.AvgVisKm,
            day.Day.Uv,
            MapCondition(day.Day.Condition),
            new WeatherAstronomy(
                day.Astro.Sunrise,
                day.Astro.Sunset,
                day.Astro.Moonrise,
                day.Astro.Moonset,
                day.Astro.MoonPhase,
                day.Astro.MoonIllumination),
            day.Hour?.Select(hour => new WeatherHourlyForecast(
                ParseDateTime(hour.Time),
                hour.TempC,
                hour.FeelsLikeC,
                hour.WindKph,
                hour.PrecipMm,
                hour.ChanceOfRain,
                hour.Humidity,
                MapCondition(hour.Condition))).ToArray() ?? []);

    private static WeatherLocation MapLocation(WeatherApiLocation location) =>
        new(
            location.Name,
            location.Region,
            location.Country,
            location.Lat,
            location.Lon,
            location.TimeZoneId,
            ParseDateTime(location.LocalTime));

    private static WeatherCondition MapCondition(WeatherApiCondition condition) =>
        new(condition.Text, NormalizeIconUrl(condition.Icon), condition.Code);

    private static WeatherAstronomy MapAstronomy(WeatherApiAstronomy astronomy) =>
        new(
            astronomy.Sunrise,
            astronomy.Sunset,
            astronomy.Moonrise,
            astronomy.Moonset,
            astronomy.MoonPhase,
            astronomy.MoonIllumination);

    private static WeatherAirQuality? MapAirQuality(WeatherApiAirQuality? airQuality) =>
        airQuality is null
            ? null
            : new(
                airQuality.CarbonMonoxide,
                airQuality.NitrogenDioxide,
                airQuality.Ozone,
                airQuality.SulphurDioxide,
                airQuality.Pm2Point5,
                airQuality.Pm10,
                airQuality.UsgEpaIndex,
                airQuality.GbDefraIndex);

    private static DateTimeOffset ParseDateTime(string value) =>
        DateTimeOffset.Parse(value, CultureInfo.InvariantCulture);

    private static string NormalizeIconUrl(string iconUrl) =>
        iconUrl.StartsWith("//", StringComparison.Ordinal)
            ? $"https:{iconUrl}"
            : iconUrl;

    private sealed class WeatherApiResponse
    {
        public WeatherApiLocation Location { get; set; } = new();
        public WeatherApiCurrent Current { get; set; } = new();
        public WeatherApiForecast? Forecast { get; set; }
        public WeatherApiAstronomyResponse? Astronomy { get; set; }
    }

    private sealed class WeatherApiAstronomyResponse
    {
        public WeatherApiAstronomy Astro { get; set; } = new();
    }

    private sealed class WeatherApiLocation
    {
        public string Name { get; set; } = string.Empty;
        public string Region { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public decimal Lat { get; set; }
        public decimal Lon { get; set; }
        [JsonPropertyName("tz_id")]
        public string TimeZoneId { get; set; } = string.Empty;
        [JsonPropertyName("localtime")]
        public string LocalTime { get; set; } = string.Empty;
    }

    private sealed class WeatherApiCurrent
    {
        [JsonPropertyName("last_updated")]
        public string LastUpdated { get; set; } = string.Empty;
        [JsonPropertyName("temp_c")]
        public decimal TempC { get; set; }
        [JsonPropertyName("feelslike_c")]
        public decimal FeelsLikeC { get; set; }
        public int Humidity { get; set; }
        [JsonPropertyName("wind_kph")]
        public decimal WindKph { get; set; }
        [JsonPropertyName("wind_dir")]
        public string WindDir { get; set; } = string.Empty;
        [JsonPropertyName("gust_kph")]
        public decimal GustKph { get; set; }
        [JsonPropertyName("pressure_mb")]
        public decimal PressureMb { get; set; }
        [JsonPropertyName("precip_mm")]
        public decimal PrecipMm { get; set; }
        public decimal Cloud { get; set; }
        [JsonPropertyName("vis_km")]
        public decimal VisKm { get; set; }
        public decimal Uv { get; set; }
        public WeatherApiCondition Condition { get; set; } = new();
        [JsonPropertyName("air_quality")]
        public WeatherApiAirQuality? AirQuality { get; set; }
    }

    private sealed class WeatherApiCondition
    {
        public string Text { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public int Code { get; set; }
    }

    private sealed class WeatherApiAirQuality
    {
        public decimal Co { get; set; }
        public decimal No2 { get; set; }
        public decimal O3 { get; set; }
        public decimal So2 { get; set; }
        [JsonPropertyName("pm2_5")]
        public decimal Pm2Point5 { get; set; }
        public decimal Pm10 { get; set; }
        [JsonPropertyName("us-epa-index")]
        public int UsgEpaIndex { get; set; }
        [JsonPropertyName("gb-defra-index")]
        public int GbDefraIndex { get; set; }
        public decimal CarbonMonoxide => Co;
        public decimal NitrogenDioxide => No2;
        public decimal Ozone => O3;
        public decimal SulphurDioxide => So2;
    }

    private sealed class WeatherApiForecast
    {
        [JsonPropertyName("forecastday")]
        public List<WeatherApiForecastDay> ForecastDay { get; set; } = [];
    }

    private sealed class WeatherApiForecastDay
    {
        public string Date { get; set; } = string.Empty;
        public WeatherApiDay Day { get; set; } = new();
        public WeatherApiAstronomy Astro { get; set; } = new();
        public List<WeatherApiHour> Hour { get; set; } = [];
    }

    private sealed class WeatherApiDay
    {
        [JsonPropertyName("maxtemp_c")]
        public decimal MaxTempC { get; set; }
        [JsonPropertyName("mintemp_c")]
        public decimal MinTempC { get; set; }
        [JsonPropertyName("avgtemp_c")]
        public decimal AvgTempC { get; set; }
        [JsonPropertyName("maxwind_kph")]
        public decimal MaxWindKph { get; set; }
        [JsonPropertyName("totalprecip_mm")]
        public decimal TotalPrecipMm { get; set; }
        [JsonPropertyName("daily_chance_of_rain")]
        public int DailyChanceOfRain { get; set; }
        [JsonPropertyName("daily_chance_of_snow")]
        public int DailyChanceOfSnow { get; set; }
        [JsonPropertyName("avghumidity")]
        public decimal AvgHumidity { get; set; }
        [JsonPropertyName("avgvis_km")]
        public decimal AvgVisKm { get; set; }
        public decimal Uv { get; set; }
        public WeatherApiCondition Condition { get; set; } = new();
    }

    private sealed class WeatherApiAstronomy
    {
        public string Sunrise { get; set; } = string.Empty;
        public string Sunset { get; set; } = string.Empty;
        public string Moonrise { get; set; } = string.Empty;
        public string Moonset { get; set; } = string.Empty;
        [JsonPropertyName("moon_phase")]
        public string MoonPhase { get; set; } = string.Empty;
        [JsonPropertyName("moon_illumination")]
        public int MoonIllumination { get; set; }
    }

    private sealed class WeatherApiHour
    {
        public string Time { get; set; } = string.Empty;
        [JsonPropertyName("temp_c")]
        public decimal TempC { get; set; }
        [JsonPropertyName("feelslike_c")]
        public decimal FeelsLikeC { get; set; }
        [JsonPropertyName("wind_kph")]
        public decimal WindKph { get; set; }
        [JsonPropertyName("precip_mm")]
        public decimal PrecipMm { get; set; }
        [JsonPropertyName("chance_of_rain")]
        public int ChanceOfRain { get; set; }
        public int Humidity { get; set; }
        public WeatherApiCondition Condition { get; set; } = new();
    }
}
