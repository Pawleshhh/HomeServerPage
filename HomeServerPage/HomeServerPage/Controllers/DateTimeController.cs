using HomeServerPage.Data;
using Microsoft.AspNetCore.Mvc;

namespace HomeServerPage.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DateTimeController(IDateTimeService dateTimeService) : ControllerBase
{
    [HttpGet("now")]
    public ActionResult<DateTime> GetNow()
    {
        return Ok(dateTimeService.Now);
    }

    [HttpGet("utcnow")]
    public ActionResult<DateTime> GetUtcNow()
    {
        return Ok(dateTimeService.UtcNow);
    }

    [HttpGet("today")]
    public ActionResult<DateOnly> GetToday()
    {
        return Ok(dateTimeService.Today);
    }

    [HttpGet("sync")]
    public ActionResult<DateTimeSyncResult> GetSync()
    {
        return Ok(new DateTimeSyncResult(dateTimeService.Now, dateTimeService.UtcNow));
    }
}
