using HomeServerPage.Client.Data.Locations;
using HomeServerPage.Data.Locations;
using HomeServerPage.Data.Weather;
using Microsoft.AspNetCore.Mvc;

namespace HomeServerPage.Controllers;

[ApiController]
[Route("api/locations")]
public sealed class LocationController(
    ILocationService locationService,
    ILocationSelectionService locationSelectionService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<SavedLocation>>> GetLocations(
        CancellationToken cancellationToken)
    {
        return Ok(await locationService.GetLocationsAsync(cancellationToken));
    }

    [HttpGet("{locationId:int}")]
    public async Task<ActionResult<SavedLocation>> GetLocation(
        int locationId,
        CancellationToken cancellationToken)
    {
        var location = await locationService.GetLocationAsync(locationId, cancellationToken);
        return location is null ? NotFound() : Ok(location);
    }

    [HttpPost]
    public async Task<ActionResult<SavedLocation>> CreateLocation(
        SavedLocation location,
        CancellationToken cancellationToken)
    {
        if (location.Id != 0)
        {
            return BadRequest("A new location must not have an identifier.");
        }

        try
        {
            var savedLocation = await locationService.SaveLocationAsync(location, cancellationToken);
            return CreatedAtAction(
                nameof(GetLocation),
                new { locationId = savedLocation.Id },
                savedLocation);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(exception.Message);
        }
    }

    [HttpPut("{locationId:int}")]
    public async Task<ActionResult<SavedLocation>> UpdateLocation(
        int locationId,
        SavedLocation location,
        CancellationToken cancellationToken)
    {
        if (locationId != location.Id)
        {
            return BadRequest("The route identifier and location identifier must match.");
        }

        try
        {
            return Ok(await locationService.SaveLocationAsync(location, cancellationToken));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (ArgumentException exception)
        {
            return BadRequest(exception.Message);
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new ProblemDetails
            {
                Title = "The location cannot be updated.",
                Detail = exception.Message,
                Status = StatusCodes.Status409Conflict
            });
        }
    }

    [HttpDelete("{locationId:int}")]
    public async Task<IActionResult> DeleteLocation(
        int locationId,
        CancellationToken cancellationToken)
    {
        try
        {
            await locationService.DeleteLocationAsync(locationId, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (LocationInUseException exception)
        {
            return Conflict(new ProblemDetails
            {
                Title = "The location is in use.",
                Detail = exception.Message,
                Status = StatusCodes.Status409Conflict
            });
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new ProblemDetails
            {
                Title = "The location cannot be deleted.",
                Detail = exception.Message,
                Status = StatusCodes.Status409Conflict
            });
        }
    }

    [HttpGet("selection/{feature}")]
    public async Task<ActionResult<SavedLocation>> GetSelectedLocation(
        LocationFeature feature,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await locationSelectionService.GetSelectedLocationAsync(feature, cancellationToken));
        }
        catch (WeatherApiException exception)
        {
            return StatusCode(exception.StatusCode, new ProblemDetails
            {
                Title = "The initial weather location could not be resolved.",
                Detail = exception.Message,
                Status = exception.StatusCode
            });
        }
    }

    [HttpPut("selection/{feature}")]
    public async Task<ActionResult<SavedLocation>> SetSelectedLocation(
        LocationFeature feature,
        LocationSelectionRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await locationSelectionService.SetSelectedLocationAsync(
                feature,
                request.LocationId,
                cancellationToken));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (ArgumentOutOfRangeException exception)
        {
            return BadRequest(exception.Message);
        }
    }
}
