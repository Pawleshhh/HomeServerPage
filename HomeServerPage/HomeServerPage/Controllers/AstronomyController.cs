using AstroCalc.Core;
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
    public async Task<ActionResult> GetPlanet(int planetId, [FromQuery] GeographicCoordinate location)
    {
        if (!Enum.IsDefined(typeof(Planet), planetId))
        {
            return BadRequest($"Unknown planet identifier: {planetId}.");
        }

        await astronomyService.GetPlanetRiseAndSetTime(location, (Planet)planetId);
        return Ok();
    }
}
