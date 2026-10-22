using HomeServerPage.Data.Exercise;
using Microsoft.AspNetCore.Mvc;

namespace HomeServerPage.Controllers;

[ApiController]
[Route("api/exercise")]
public sealed class ExerciseController(IExerciseService exerciseService) : ControllerBase
{
    [HttpGet("entries")]
    public async Task<ActionResult<List<ExerciseEntry>>> GetEntries(
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? through,
        CancellationToken cancellationToken)
    {
        if (from is null || through is null || through < from)
        {
            return BadRequest("A valid start and end date are required.");
        }

        return await exerciseService.GetEntriesAsync(from.Value, through.Value, cancellationToken);
    }

    [HttpPost("entries")]
    public async Task<ActionResult<ExerciseEntry>> AddEntry(
        [FromBody] ExerciseEntry entry,
        CancellationToken cancellationToken)
    {
        var validationError = Validate(entry);
        return validationError is not null
            ? BadRequest(validationError)
            : Ok(await exerciseService.AddEntryAsync(entry, cancellationToken));
    }

    [HttpPut("entries/{id:int}")]
    public async Task<IActionResult> UpdateEntry(
        int id,
        [FromBody] ExerciseEntry entry,
        CancellationToken cancellationToken)
    {
        var validationError = Validate(entry);
        if (validationError is not null)
        {
            return BadRequest(validationError);
        }

        var updated = await exerciseService.UpdateEntryAsync(entry with { Id = id }, cancellationToken);
        return updated ? NoContent() : NotFound();
    }

    [HttpDelete("entries/{id:int}")]
    public async Task<IActionResult> RemoveEntry(int id, CancellationToken cancellationToken)
    {
        var removed = await exerciseService.RemoveEntryAsync(id, cancellationToken);
        return removed ? NoContent() : NotFound();
    }

    private static string? Validate(ExerciseEntry entry)
    {
        if (!ExerciseCalorieCalculator.IsValid(entry.Type, entry.DistanceKm))
        {
            return "Gym has no distance; running and biking need a distance above 0 with at most two decimals.";
        }

        var todayAtEntryOffset = DateOnly.FromDateTime(
            DateTimeOffset.UtcNow.ToOffset(entry.LoggedAt.Offset).DateTime);
        return entry.LocalDate <= todayAtEntryOffset &&
            entry.LocalDate == DateOnly.FromDateTime(entry.LoggedAt.DateTime)
                ? null
                : "The exercise date and time must be today or earlier and match.";
    }
}
