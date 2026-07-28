using Microsoft.AspNetCore.Mvc;
using WorkHoursLib.Models;
using WorkHoursLib.Services.Interfaces;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WorkHoursRest.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ShiftsController : ControllerBase
{
    private readonly IGenericRepository<Shift> _repository;

    public ShiftsController(IGenericRepository<Shift> repository)
    {
        _repository = repository;
    }

    // GET: api/<ShiftsController>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Get()
    {
        return Ok(_repository.GetAll());
    }

    // GET api/<ShiftsController>/5
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Get(int id)
    {
        var shift = _repository.GetById(id);
        if (shift == null)
        {
            return NotFound();
        }
        return Ok(shift);
    }

    // POST api/<ShiftsController>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult Post([FromBody] Shift shift)
    {
        if (shift == null)
        {
            return BadRequest();
        }

        _repository.Add(shift);
        return CreatedAtAction(nameof(Get), new { id = shift.Id }, shift);
    }

    // PUT api/<ShiftsController>/5
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Put(int id, [FromBody] Shift shift)
    {
        if (shift == null || id != shift.Id)
        {
            return BadRequest();
        }

        var existingShift = _repository.GetById(id);

        if (existingShift == null)
        {
            return NotFound();
        }

        _repository.Update(shift);
        return NoContent();
    }

    // DELETE api/<ShiftsController>/5
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Delete(int id)
    {
        var existingShift = _repository.GetById(id);

        if (existingShift == null)
        {
            return NotFound();
        }

        _repository.Delete(id);
        return NoContent();
    }
}
