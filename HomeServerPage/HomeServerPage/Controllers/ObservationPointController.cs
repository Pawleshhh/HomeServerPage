using HomeServerPage.Data.Astronomy;
using Microsoft.AspNetCore.Mvc;

namespace HomeServerPage.Controllers;

[ApiController]
[Route("api/observation-points")]
public sealed class ObservationPointController(IObservationPointService observationPointService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ObservationPoint>>> GetObservationPoints(
        CancellationToken cancellationToken)
    {
        return Ok(await observationPointService.GetObservationPointsAsync(cancellationToken));
    }

    [HttpPost]
    public async Task<ActionResult<ObservationPoint>> CreateObservationPoint(
        ObservationPoint observationPoint,
        CancellationToken cancellationToken)
    {
        if (observationPoint.Id != 0)
        {
            return BadRequest("A new observation point must not have an identifier.");
        }

        try
        {
            var savedPoint = await observationPointService.SaveObservationPointAsync(
                observationPoint,
                cancellationToken);

            return CreatedAtAction(
                nameof(GetObservationPoint),
                new { id = savedPoint.Id },
                savedPoint);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(exception.Message);
        }
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ObservationPoint>> GetObservationPoint(
        int id,
        CancellationToken cancellationToken)
    {
        var observationPoints = await observationPointService.GetObservationPointsAsync(cancellationToken);
        var observationPoint = observationPoints.FirstOrDefault(point => point.Id == id);

        return observationPoint is null
            ? NotFound()
            : Ok(observationPoint);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ObservationPoint>> UpdateObservationPoint(
        int id,
        ObservationPoint observationPoint,
        CancellationToken cancellationToken)
    {
        if (id != observationPoint.Id)
        {
            return BadRequest("The route identifier and observation point identifier must match.");
        }

        try
        {
            return Ok(await observationPointService.SaveObservationPointAsync(
                observationPoint,
                cancellationToken));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (ArgumentException exception)
        {
            return BadRequest(exception.Message);
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteObservationPoint(
        int id,
        CancellationToken cancellationToken)
    {
        try
        {
            await observationPointService.DeleteObservationPointAsync(id, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}
