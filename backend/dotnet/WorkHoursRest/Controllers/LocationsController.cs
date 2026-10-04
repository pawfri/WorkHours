using Microsoft.AspNetCore.Mvc;
using WorkHoursLib.Models;
using WorkHoursLib.Services.Interfaces;
using WorkHoursRest.Dtos;

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
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public IActionResult Post([FromBody] LocationDto request)
    {
        var location = new Location
        {
            Name = request.Name,
            Address = request.Address,
            City = request.City,
            ZipCode = request.ZipCode
        };
        location.Normalize();

        bool nameTaken = _repository.GetAll()
            .Any(l => string.Equals(l.Name, location.Name, StringComparison.OrdinalIgnoreCase));

        if (nameTaken)
        {
            return Problem(statusCode: StatusCodes.Status409Conflict, detail: $"A location named '{location.Name}' already exists.");
        }

        _repository.Add(location);
        _repository.Save();
        return CreatedAtAction(nameof(Get), new { id = location.Id }, location);
    }

    // PUT api/<LocationsController>/5
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public IActionResult Put(int id, [FromBody] LocationDto request)
    {
        if (_repository.GetById(id) == null)
        {
            return NotFound();
        }

        var location = new Location
        {
            Id = id,
            Name = request.Name,
            Address = request.Address,
            City = request.City,
            ZipCode = request.ZipCode
        };
        location.Normalize();

        bool nameTaken = _repository.GetAll()
            .Any(l => l.Id != id && string.Equals(l.Name, location.Name, StringComparison.OrdinalIgnoreCase));

        if (nameTaken)
        {
            return Problem(statusCode: StatusCodes.Status409Conflict, detail: $"A location named '{location.Name}' already exists.");
        }

        _repository.Update(location);
        _repository.Save();
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
        _repository.Save();
        return NoContent();
    }
}
