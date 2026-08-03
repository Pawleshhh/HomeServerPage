using HomeServerPage.Data.PublicTransport;
using Microsoft.AspNetCore.Mvc;

namespace HomeServerPage.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PublicTransportController(IPublicTransportService publicTransportService) : ControllerBase
{
    [HttpGet("departure-boards")]
    public async Task<ActionResult<List<DepartureBoard>>> GetDepartureBoards(
        [FromQuery] int limit = 10,
        CancellationToken cancellationToken = default)
    {
        var boards = await publicTransportService.GetDepartureBoardsAsync(limit, cancellationToken);
        return Ok(boards);
    }

    [HttpGet("departure-boards/{stopNumber:int}")]
    public async Task<ActionResult<DepartureBoard>> GetDepartureBoard(
        int stopNumber,
        [FromQuery] int limit = 10,
        CancellationToken cancellationToken = default)
    {
        var board = await publicTransportService.GetDepartureBoardAsync(stopNumber, limit, cancellationToken);
        return Ok(board);
    }
}
