using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AwesomeBackend.Controllers;

[Authorize]
public class EventsController : ControllerBase
{
    /// <summary>
    /// Get the events list
    /// </summary>
    /// <response code="200">The last 42 events</response>
    [HttpGet]
    [AllowAnonymous]
    public IEnumerable<Event> GetEvents()
        => Array.Empty<Event>();

    /// <summary>
    /// Get a specific event
    /// </summary>
    /// <param name="id">The id of the event</param>
    /// <response code="200">The event</response>
    /// <response code="404">Event not found</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(Event), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesDefaultResponseType]
    public IActionResult Get(Guid id)
    {
        if (id != Guid.Empty)
        {
            return Ok(new Event(id, "Frullino event", DateTime.Now, Priority.Standard));
        }

        return NotFound();
    }

    [HttpPost]
    public IActionResult Save(Event @event)
        => NoContent();
}

public record class Event(Guid Id, string Name, DateTime StartAt, Priority? Priority);

public enum Priority
{
    Low,
    Standard,
    High
}