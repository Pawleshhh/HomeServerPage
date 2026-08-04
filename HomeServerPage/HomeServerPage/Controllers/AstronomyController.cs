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
}
