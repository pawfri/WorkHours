using Microsoft.AspNetCore.Mvc;
using WorkHoursLib.Models;
using WorkHoursLib.Services.Interfaces;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WorkHoursRest.Controllers;

[Route("api/[controller]")]
[ApiController]
public class WorkDaysController : ControllerBase
{
    private readonly IGenericRepository<WorkDay> _repository;

    public WorkDaysController(IGenericRepository<WorkDay> repository)
    {
        _repository = repository;
    }

    // GET: api/<WorkDaysController>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Get()
    {
        return Ok(_repository.GetAll());
    }

    // GET api/<WorkDaysController>/5
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Get(int id)
    {
        var workDay = _repository.GetById(id);
        if (workDay == null)
        {
            return NotFound();
        }
        return Ok(workDay);
    }

    // POST api/<WorkDaysController>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult Post([FromBody] WorkDay workDay)
    {
        if (workDay == null)
        {
            return BadRequest();
        }

        _repository.Add(workDay);
        return CreatedAtAction(nameof(Get), new { id = workDay.Id }, workDay);
    }

    // PUT api/<WorkDaysController>/5
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Put(int id, [FromBody] WorkDay workDay)
    {
        if (workDay == null || id != workDay.Id)
        {
            return BadRequest();
        }

        var existingWorkDay = _repository.GetById(id);

        if (existingWorkDay == null)
        {
            return NotFound();
        }

        _repository.Update(workDay);
        return NoContent();
    }

    // DELETE api/<WorkDaysController>/5
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Delete(int id)
    {
        var existingWorkDay = _repository.GetById(id);

        if (existingWorkDay == null)
        {
            return NotFound();
        }

        _repository.Delete(id);
        return NoContent();
    }
}
