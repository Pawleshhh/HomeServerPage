using HomeServerPage.Client.Data.Locations;
using HomeServerPage.Data.Locations;
using HomeServerPage.Data.Weather;
using Microsoft.AspNetCore.Mvc;

namespace HomeServerPage.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class WeatherController(
    IWeatherService weatherService,
    ILocationService locationService) : ControllerBase
{
    [HttpGet("current")]
    public async Task<ActionResult<WeatherSnapshot>> GetCurrent(
        [FromQuery] int locationId,
        CancellationToken cancellationToken = default)
    {
        var location = await locationService.GetLocationAsync(locationId, cancellationToken);
        if (location is null)
        {
            return NotFound($"Location {locationId} was not found.");
        }

        try
        {
            return Ok(await weatherService.GetCurrentAsync(location, cancellationToken));
        }
        catch (WeatherApiException exception)
        {
            return StatusCode(exception.StatusCode, new ProblemDetails
            {
                Title = "Weather provider request failed.",
                Detail = exception.Message,
                Status = exception.StatusCode
            });
        }
    }

    [HttpGet("forecast")]
    public async Task<ActionResult<WeatherForecast>> GetForecast(
        [FromQuery] int locationId,
        [FromQuery] int days = 3,
        CancellationToken cancellationToken = default)
    {
        if (days is < 1 or > 14)
        {
            return BadRequest("Forecast days must be between 1 and 14.");
        }

        var location = await locationService.GetLocationAsync(locationId, cancellationToken);
        if (location is null)
        {
            return NotFound($"Location {locationId} was not found.");
        }

        try
        {
            return Ok(await weatherService.GetForecastAsync(location, days, cancellationToken));
        }
        catch (WeatherApiException exception)
        {
            return StatusCode(exception.StatusCode, new ProblemDetails
            {
                Title = "Weather provider request failed.",
                Detail = exception.Message,
                Status = exception.StatusCode
            });
        }
    }

    [HttpGet("astronomy")]
    public async Task<ActionResult<WeatherAstronomy>> GetAstronomy(
        [FromQuery] int locationId,
        [FromQuery] DateOnly date,
        CancellationToken cancellationToken = default)
    {
        var location = await locationService.GetLocationAsync(locationId, cancellationToken);
        if (location is null)
        {
            return NotFound($"Location {locationId} was not found.");
        }

        try
        {
            return Ok(await weatherService.GetAstronomyAsync(location, date, cancellationToken));
        }
        catch (WeatherApiException exception)
        {
            return StatusCode(exception.StatusCode, new ProblemDetails
            {
                Title = "Weather provider request failed.",
                Detail = exception.Message,
                Status = exception.StatusCode
            });
        }
    }
}
