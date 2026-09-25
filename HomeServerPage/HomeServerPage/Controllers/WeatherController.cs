using HomeServerPage.Data.Weather;
using Microsoft.AspNetCore.Mvc;

namespace HomeServerPage.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class WeatherController(IWeatherService weatherService) : ControllerBase
{
    [HttpGet("current")]
    public async Task<ActionResult<WeatherSnapshot>> GetCurrent(
        CancellationToken cancellationToken = default)
    {
        try
        {
            return Ok(await weatherService.GetCurrentAsync(cancellationToken));
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
        [FromQuery] int days = 3,
        CancellationToken cancellationToken = default)
    {
        if (days is < 1 or > 14)
        {
            return BadRequest("Forecast days must be between 1 and 14.");
        }

        try
        {
            return Ok(await weatherService.GetForecastAsync(days, cancellationToken));
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
        [FromQuery] DateOnly date,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return Ok(await weatherService.GetAstronomyAsync(date, cancellationToken));
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
