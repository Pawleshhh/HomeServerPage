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
        return Ok(item);
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
        if (string.IsNullOrWhiteSpace(item.Name))
        {
            return BadRequest("Item name is required.");
        }

        if (item.ExpirationDate.Date < item.AddedDate.Date)
        {
            return BadRequest("Expiration date cannot be earlier than added date.");
        }

        var created = await fridgeService.AddItemAsync(item, saveAsTemplate);
        return Ok(created);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateItem(int id, [FromBody] FridgeItem item)
    {
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
}
