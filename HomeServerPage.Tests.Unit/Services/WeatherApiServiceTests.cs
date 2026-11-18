using System.Net;
using System.Text;
using HomeServerPage.Data.Locations;
using HomeServerPage.Data.Weather;
using HomeServerPage.Tests.Unit.Infrastructure;
using Microsoft.Extensions.Options;

namespace HomeServerPage.Tests.Unit.Services;

public sealed class WeatherApiServiceTests
{
    private const string CurrentResponse = """
        {
          "location": {
            "name": "Szczecin",
            "region": "West Pomerania",
            "country": "Poland",
            "lat": 53.4,
            "lon": 14.5,
            "tz_id": "Europe/Warsaw",
            "localtime": "2026-01-15 12:00"
          },
          "current": {
            "last_updated": "2026-01-15 11:00",
            "temp_c": 1.5,
            "feelslike_c": 0.5,
            "humidity": 77,
            "wind_kph": 6.2,
            "wind_dir": "NW",
            "gust_kph": 9.1,
            "pressure_mb": 1001.1,
            "precip_mm": 0.2,
            "cloud": 50,
            "vis_km": 10,
            "uv": 1,
            "condition": {
              "text": "Partly cloudy",
              "icon": "//cdn.test/icon.png",
              "code": 1003
            },
            "air_quality": {
              "co": 0.1,
              "no2": 0.2,
              "o3": 0.3,
              "so2": 0.4,
              "pm2_5": 1.2,
              "pm10": 2.3,
              "us-epa-index": 1,
              "gb-defra-index": 2
            }
          }
        }
        """;

    [Test]
    public async Task CurrentWeatherMapsProviderFieldsAndEscapesCoordinatesAndKey()
    {
        HttpRequestMessage? capturedRequest = null;
        using var client = CreateClient((request, _) =>
        {
            capturedRequest = request;
            return Task.FromResult(JsonResponse(CurrentResponse));
        });
        var service = CreateService(client);

        var result = await service.GetCurrentAsync(CreateLocation());

        Assert.That(result.TemperatureCelsius, Is.EqualTo(1.5m));
        Assert.That(result.Condition.IconUrl, Is.EqualTo("https://cdn.test/icon.png"));
        Assert.That(result.AirQuality?.Pm2Point5, Is.EqualTo(1.2m));
        Assert.That(capturedRequest!.RequestUri!.Query, Does.Contain("key=test%20key"));
        Assert.That(capturedRequest.RequestUri.Query, Does.Contain("q=53.4%2C14.5"));
        Assert.That(capturedRequest.RequestUri.Query, Does.Contain("aqi=yes"));
    }

    [Test]
    public async Task ForecastUsesConfiguredDefaultAndClampsExplicitDays()
    {
        var requests = new List<Uri>();
        using var client = CreateClient((request, _) =>
        {
            requests.Add(request.RequestUri!);
            return Task.FromResult(JsonResponse(CurrentResponse));
        });
        var service = CreateService(client, forecastDays: 6);

        var defaultForecast = await service.GetForecastAsync(CreateLocation());
        var clampedForecast = await service.GetForecastAsync(CreateLocation(), 20);

        Assert.That(defaultForecast.Days, Is.Empty);
        Assert.That(clampedForecast.Days, Is.Empty);
        Assert.That(requests[0].Query, Does.Contain("days=6"));
        Assert.That(requests[1].Query, Does.Contain("days=14"));
    }

    [TestCase(HttpStatusCode.Unauthorized)]
    [TestCase(HttpStatusCode.Forbidden)]
    [TestCase(HttpStatusCode.BadRequest)]
    public void ProviderErrorsAreReportedAsWeatherApiExceptions(HttpStatusCode statusCode)
    {
        using var client = CreateClient((_, _) =>
            Task.FromResult(new HttpResponseMessage(statusCode)));
        var service = CreateService(client);

        Assert.ThrowsAsync<WeatherApiException>(
            async () => await service.GetCurrentAsync(CreateLocation()));
    }

    [Test]
    public void JsonNullResponseIsReportedAsAnEmptyProviderResponse()
    {
        using var client = CreateClient((_, _) => Task.FromResult(JsonResponse("null")));
        var service = CreateService(client);

        Assert.ThrowsAsync<WeatherApiException>(
            async () => await service.GetCurrentAsync(CreateLocation()));
    }

    private static WeatherApiService CreateService(HttpClient client, int forecastDays = 3) =>
        new(
            client,
            Options.Create(new WeatherApiOptions
            {
                ApiKey = "test key",
                ForecastDays = forecastDays
            }));

    private static SavedLocation CreateLocation() => new()
    {
        Name = "Szczecin",
        Latitude = 53.4,
        Longitude = 14.5,
        TimeZoneId = "Europe/Warsaw"
    };

    private static HttpClient CreateClient(
        Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> handler) =>
        new(new StubHttpMessageHandler(handler))
        {
            BaseAddress = new Uri("https://weather.test/v1/")
        };

    private static HttpResponseMessage JsonResponse(string json) => new(HttpStatusCode.OK)
    {
        Content = new StringContent(json, Encoding.UTF8, "application/json")
    };
}
