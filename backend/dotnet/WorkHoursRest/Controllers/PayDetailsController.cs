using Microsoft.AspNetCore.Mvc;
using WorkHoursLib.Models;
using WorkHoursLib.Services.Interfaces;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WorkHoursRest.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PayDetailsController : ControllerBase
{
    private readonly IGenericRepository<PayDetail> _repository;

    public PayDetailsController(IGenericRepository<PayDetail> repository)
    {
        _repository = repository;
    }

    // GET: api/<PayDetailsController>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Get()
    {
        var payDetails = _repository.GetAll();
        return Ok(payDetails);
    }

    // GET api/<PayDetailsController>/5
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Get(int id)
    {
        var payDetail = _repository.GetById(id);
        if (payDetail == null)
        {
            return NotFound();
        }
        return Ok(payDetail);
    }

    // POST api/<PayDetailsController>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult Post([FromBody] PayDetail payDetail)
    {
        if (payDetail == null)
        {
            return BadRequest();
        }

        _repository.Add(payDetail);
        return CreatedAtAction(nameof(Get), new { id = payDetail.Id }, payDetail);
    }

    // PUT api/<PayDetailsController>/5
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Put(int id, [FromBody] PayDetail payDetail)
    {
        if (payDetail == null || id != payDetail.Id)
        {
            return BadRequest();
        }

        if (_repository.GetById(id) == null)
        {
            return NotFound();
        }

        _repository.Update(payDetail);
        return NoContent();

    }

    // DELETE api/<PayDetailsController>/5
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
