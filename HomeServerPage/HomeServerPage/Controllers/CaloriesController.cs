using HomeServerPage.Data.Calories;
using Microsoft.AspNetCore.Mvc;

namespace HomeServerPage.Controllers;

[ApiController]
[Route("api/calories")]
public sealed class CaloriesController(ICaloriesService caloriesService) : ControllerBase
{
    [HttpGet("foods")]
    public async Task<ActionResult<List<CaloriesFood>>> GetFoods(CancellationToken cancellationToken) =>
        await caloriesService.GetFoodsAsync(cancellationToken);

    [HttpPost("foods")]
    public async Task<ActionResult<CaloriesFood>> AddFood(
        [FromBody] CaloriesFood food,
        CancellationToken cancellationToken)
    {
        var validationError = ValidateFood(food);
        if (validationError is not null)
        {
            return BadRequest(validationError);
        }

        return Ok(await caloriesService.AddFoodAsync(food, cancellationToken));
    }

    [HttpPut("foods/{id:int}")]
    public async Task<IActionResult> UpdateFood(
        int id,
        [FromBody] CaloriesFood food,
        CancellationToken cancellationToken)
    {
        var validationError = ValidateFood(food);
        if (validationError is not null)
        {
            return BadRequest(validationError);
        }

        var updated = await caloriesService.UpdateFoodAsync(food with { Id = id }, cancellationToken);
        return updated ? NoContent() : NotFound();
    }

    [HttpDelete("foods/{id:int}")]
    public async Task<IActionResult> RemoveFood(int id, CancellationToken cancellationToken)
    {
        var removed = await caloriesService.RemoveFoodAsync(id, cancellationToken);
        return removed ? NoContent() : NotFound();
    }

    [HttpGet("entries")]
    public async Task<ActionResult<List<CaloriesLogEntry>>> GetEntries(
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? through,
        CancellationToken cancellationToken)
    {
        if (from is null || through is null || through < from)
        {
            return BadRequest("A valid start and end date are required.");
        }

        return await caloriesService.GetEntriesAsync(from.Value, through.Value, cancellationToken);
    }

    [HttpPost("entries")]
    public async Task<ActionResult<CaloriesLogEntry>> AddEntry(
        [FromBody] CaloriesLogEntry entry,
        CancellationToken cancellationToken)
    {
        var validationError = ValidateEntry(entry);
        if (validationError is not null)
        {
            return BadRequest(validationError);
        }

        var todayAtEntryOffset = DateOnly.FromDateTime(
            DateTimeOffset.UtcNow.ToOffset(entry.LoggedAt.Offset).DateTime);
        if (entry.LocalDate > todayAtEntryOffset ||
            entry.LocalDate != DateOnly.FromDateTime(entry.LoggedAt.DateTime))
        {
            return BadRequest("The entry date and time must be today or earlier and match.");
        }

        return Ok(await caloriesService.AddEntryAsync(entry, cancellationToken));
    }

    [HttpGet("goal")]
    public async Task<ActionResult<CaloriesGoalResponse>> GetDailyGoal(CancellationToken cancellationToken) =>
        Ok(new CaloriesGoalResponse(await caloriesService.GetDailyGoalAsync(cancellationToken)));

    [HttpPut("goal")]
    public async Task<ActionResult<int>> SetDailyGoal(
        [FromBody] int calories,
        CancellationToken cancellationToken)
    {
        if (calories <= 0)
        {
            return BadRequest("The daily calorie goal must be a positive whole number.");
        }

        return await caloriesService.SetDailyGoalAsync(calories, cancellationToken);
    }

    private static string? ValidateFood(CaloriesFood food)
    {
        if (string.IsNullOrWhiteSpace(food.Name) || food.Name.Trim().Length > 160)
        {
            return "A food name of at most 160 characters is required.";
        }

        return food.CaloriesPerServing <= 0 || HasNegativeMacros(
            food.CarbohydratesGrams,
            food.ProteinGrams,
            food.FatGrams)
            ? "Calories must be positive and optional macros cannot be negative."
            : null;
    }

    private static string? ValidateEntry(CaloriesLogEntry entry)
    {
        if (string.IsNullOrWhiteSpace(entry.Name) || entry.Name.Trim().Length > 160)
        {
            return "A food name of at most 160 characters is required.";
        }

        if (entry.Calories <= 0 ||
            HasNegativeMacros(entry.CarbohydratesGrams, entry.ProteinGrams, entry.FatGrams) ||
            entry.Servings is <= 0 ||
            entry.Notes?.Length > 500)
        {
            return "Calories and servings must be positive, optional macros cannot be negative, and notes must be at most 500 characters.";
        }

        return null;
    }

    private static bool HasNegativeMacros(int? carbohydrates, int? protein, int? fat) =>
        carbohydrates < 0 || protein < 0 || fat < 0;
}
