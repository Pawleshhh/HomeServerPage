using HomeServerPage.Client.Data.Astronomy.Telescopes;
using HomeServerPage.Data.Astronomy;
using Microsoft.AspNetCore.Mvc;

namespace HomeServerPage.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TelescopeController(ITelescopeService telescopeService) : ControllerBase
{
    [HttpGet("status")]
    public async Task<ActionResult<bool>> GetStatus(CancellationToken cancellationToken)
    {
        return Ok(await telescopeService.IsAvailableAsync(cancellationToken));
    }
}
