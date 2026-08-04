using AstroCalc.Core;
using AstroCalc.Observation;
using AstroCalc.SolarSystem;
using HomeServerPage.Data.Astronomy;
using Microsoft.AspNetCore.Mvc;

namespace HomeServerPage.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AstronomyController(IAstronomyService astronomyService) : ControllerBase
{
    [HttpGet("status")]
    public async Task<ActionResult<bool>> GetStatus(CancellationToken cancellationToken)
    {
        return Ok(await astronomyService.IsAvailableAsync(cancellationToken));
    }

    [HttpGet("solarsystem/{planetId:int}")]
    public async Task<ActionResult<RiseTransitSetResult>> GetPlanet(
        int planetId,
        [FromQuery] DateTime dateTime,
        [FromQuery] double latitude,
        [FromQuery] double longitude,
        [FromQuery] double elevationMeters)
    {
        if (!Enum.IsDefined(typeof(Planet), planetId))
        {
            return BadRequest($"Unknown planet identifier: {planetId}.");
        }

        var location = GeographicCoordinate.FromDegrees(latitude, longitude, elevationMeters);
        var result = await astronomyService.GetPlanetRiseAndSetTime(dateTime, location, (Planet)planetId);
        return Ok(result);
    }
}
