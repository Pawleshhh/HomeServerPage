using HomeServerPage.Client.Data.Astronomy.Telescopes;
using HomeServerPage.Data.Astronomy;
using Microsoft.AspNetCore.Mvc;

namespace HomeServerPage.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TelescopeController(ITelescopeService telescopeService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TelescopeItem>>> GetTelescopes(CancellationToken cancellationToken)
    {
        return Ok(await telescopeService.GetTelescopesAsync(cancellationToken));
    }

    [HttpGet("eyepieces")]
    public async Task<ActionResult<IEnumerable<TelescopeEyepiece>>> GetEyepieces(CancellationToken cancellationToken)
    {
        return Ok(await telescopeService.GetEyepiecesAsync(cancellationToken));
    }

    [HttpGet("lenses")]
    public async Task<ActionResult<IEnumerable<TelescopeLens>>> GetLenses(CancellationToken cancellationToken)
    {
        return Ok(await telescopeService.GetLensesAsync(cancellationToken));
    }

    [HttpGet("sensors")]
    public async Task<ActionResult<IEnumerable<SensorItem>>> GetSensors(CancellationToken cancellationToken)
    {
        return Ok(await telescopeService.GetSensorsAsync(cancellationToken));
    }

    [HttpGet("status")]
    public async Task<ActionResult<bool>> GetStatus(CancellationToken cancellationToken)
    {
        return Ok(await telescopeService.IsAvailableAsync(cancellationToken));
    }
}
