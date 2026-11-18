using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using HomeServerPage.Data.Locations;
using HomeServerPage.Data.Weather;
using Microsoft.Extensions.Options;

namespace HomeServerPage.Data.Locations;

public sealed class WeatherApiLocationResolver(
    HttpClient httpClient,
    IOptions<WeatherApiOptions> options,
    ILogger<WeatherApiLocationResolver> logger) : IWeatherLocationResolver
{
    public async Task<SavedLocation> ResolveAsync(
        string query,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(options.Value.ApiKey))
        {
            throw new WeatherApiException(
                (int)HttpStatusCode.InternalServerError,
                "The weather API token is not configured, so the initial location cannot be resolved.");
        }

        HttpResponseMessage response;
        try
        {
            response = await httpClient.GetAsync(
                $"search.json?key={Uri.EscapeDataString(options.Value.ApiKey)}&q={Uri.EscapeDataString(query)}",
                cancellationToken);
        }
        catch (HttpRequestException)
        {
            logger.LogWarning("The weather provider could not be reached while resolving the initial location.");
            throw new WeatherApiException(
                (int)HttpStatusCode.BadGateway,
                "The weather provider could not be reached while resolving the configured initial location.");
        }

        using (response)
        {
            if (!response.IsSuccessStatusCode)
            {
                throw new WeatherApiException(
                    (int)response.StatusCode,
                    "The weather API could not resolve the configured initial location.");
            }

            List<WeatherApiSearchResult>? matches;
            try
            {
                matches = await response.Content.ReadFromJsonAsync<List<WeatherApiSearchResult>>(
                    cancellationToken: cancellationToken);
            }
            catch (System.Text.Json.JsonException)
            {
                logger.LogWarning("The weather provider returned an invalid location lookup response.");
                throw new WeatherApiException(
                    (int)HttpStatusCode.BadGateway,
                    "The weather provider returned an invalid response while resolving the configured initial location.");
            }

            var match = matches?.FirstOrDefault()
                ?? throw new WeatherApiException(
                    (int)HttpStatusCode.BadGateway,
                    "The weather API returned no matching location for the configured initial query.");

            return new SavedLocation
            {
                Name = match.Name,
                Latitude = match.Latitude,
                Longitude = match.Longitude,
                ElevationMeters = 0,
                TimeZoneId = match.TimeZoneId
            };
        }
    }

    private sealed class WeatherApiSearchResult
    {
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("lat")]
        public double Latitude { get; set; }

        [JsonPropertyName("lon")]
        public double Longitude { get; set; }

        [JsonPropertyName("tz_id")]
        public string TimeZoneId { get; set; } = string.Empty;
    }
}
