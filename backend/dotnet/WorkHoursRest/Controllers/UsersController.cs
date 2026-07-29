using Microsoft.AspNetCore.Mvc;
using WorkHoursLib.Models;
using WorkHoursLib.Services.Interfaces;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WorkHoursRest.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UsersController : ControllerBase
{
    private readonly IGenericRepository<User> _repository;

    public UsersController(IGenericRepository<User> repository)
    {
        _repository = repository;
    }

    // GET: api/<UsersController>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Get()
    {
        var users = _repository.GetAll();
        return Ok(users);
    }

    // GET api/<UsersController>/5
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Get(int id)
    {
        var user = _repository.GetById(id);
        if (user == null)
        {
            return NotFound();
        }
        return Ok(user);
    }

    // POST api/<UsersController>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult Post([FromBody] User user)
    {
        if (user == null) 
        { 
            return BadRequest();
        }

        _repository.Add(user);
        return CreatedAtAction(nameof(Get), new { id = user.Id }, user);
    }

    // PUT api/<UsersController>/5
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Put(int id, [FromBody] User user)
    {
        if (User == null || id != user.Id)
        {
            return BadRequest();
        }

        if (_repository.GetById(id) == null)
        {
            return NotFound();
        }

        _repository.Update(user);
        return NoContent();

    }

    // DELETE api/<UsersController>/5
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
