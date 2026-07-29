using Microsoft.AspNetCore.Mvc;
using WorkHoursLib.Models;
using WorkHoursLib.Services.Interfaces;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WorkHoursRest.Controllers;

[Route("api/[controller]")]
[ApiController]
public class LocationsController : ControllerBase
{
    private readonly IGenericRepository<Location> _repository;

    public LocationsController(IGenericRepository<Location> repository)
    {
        _repository = repository;
    }

    // GET: api/<LocationsController>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Get()
    {
        var locations = _repository.GetAll();
        return Ok(locations);
    }

    // GET api/<LocationsController>/5
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Get(int id)
    {
        var location = _repository.GetById(id);
        if (location == null)
        {
            return NotFound();
        }
        return Ok(location);
    }

    // POST api/<LocationsController>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult Post([FromBody] Location location)
    {
        if (location == null)
        {
            return BadRequest();
        }

        _repository.Add(location);
        return CreatedAtAction(nameof(Get), new { id = location.Id }, location);
    }

    // PUT api/<LocationsController>/5
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Put(int id, [FromBody] Location location)
    {
        if (location == null || id != location.Id)
        {
            return BadRequest();
        }

        if (_repository.GetById(id) == null)
        {
            return NotFound();
        }

        _repository.Update(location);
        return NoContent();

    }

    // DELETE api/<LocationsController>/5
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Delete(int id)
    {
        if (_repository.GetById(id) == null)
        {
            return NotFound();
        }

        _repository.Delete(id);
        return NoContent();
    }
}
