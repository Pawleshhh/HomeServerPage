using HomeServerPage.Data.Fridge;
using Microsoft.AspNetCore.Mvc;

namespace HomeServerPage.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FridgeController(IFridgeService fridgeService) : ControllerBase
{

    [HttpGet("{id}")]
    public async Task<ActionResult<FridgeItem>> GetItem(int id)
    {
        var item = await fridgeService.GetItemAsync(id);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpGet]
    public async Task<ActionResult<List<FridgeItem>>> GetItems()
    {
        var items = await fridgeService.GetItemsAsync();
        return Ok(items);
    }

    [HttpGet("templates")]
    public async Task<ActionResult<List<FridgeItemTemplate>>> GetTemplates()
    {
        var templates = await fridgeService.GetTemplatesAsync();
        return Ok(templates);
    }

    [HttpPost]
    public async Task<ActionResult<FridgeItem>> AddItem(
        [FromBody] FridgeItem item,
        [FromQuery] bool saveAsTemplate = false)
    {
        var validationError = ValidateItem(item);
        if (validationError is not null)
        {
            return BadRequest(validationError);
        }

        var created = await fridgeService.AddItemAsync(item, saveAsTemplate);
        return Ok(created);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateItem(int id, [FromBody] FridgeItem item)
    {
        var validationError = ValidateItem(item);
        if (validationError is not null)
        {
            return BadRequest(validationError);
        }

        var updated = await fridgeService.UpdateItemAsync(item with { Id = id });
        if (!updated)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> RemoveItem(int id)
    {
        var removed = await fridgeService.RemoveItemAsync(id);
        if (!removed)
        {
            return NotFound();
        }

        return NoContent();
    }

    private static string? ValidateItem(FridgeItem item)
    {
        if (string.IsNullOrWhiteSpace(item.Name))
        {
            return "Item name is required.";
        }

        if (!Enum.IsDefined(item.QuantityType))
        {
            return "A valid quantity type is required.";
        }

        if (!double.IsFinite(item.QuantityValue) || item.QuantityValue < 0)
        {
            return "Quantity must be a non-negative number.";
        }

        if (item.QuantityType == QuantityType.Unit &&
            item.QuantityValue != Math.Truncate(item.QuantityValue))
        {
            return "Unit quantities must be whole numbers.";
        }

        if (item.ExpirationDate.Date < item.AddedDate.Date)
        {
            return "Expiration date cannot be earlier than added date.";
        }

        if (item.OpenDate is DateTime openDate && openDate.Date < item.AddedDate.Date)
        {
            return "Open date cannot be earlier than added date.";
        }

        return null;
    }
}
