using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using WorkHoursLib.Models;
using WorkHoursLib.Services.Interfaces;
using WorkHoursRest.Dtos;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WorkHoursRest.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ShiftsController : ControllerBase
{
    private readonly IGenericRepository<Shift> _repository;
    private readonly IGenericRepository<Location> _locationRepository;

    public ShiftsController(IGenericRepository<Shift> repository, IGenericRepository<Location> locationRepository)
    {
        _repository = repository;
        _locationRepository = locationRepository;
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

    // GET api/<ShiftsController>/day/2026-01-05
    [HttpGet("day/{date}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult GetDay(DateOnly date)
    {
        var shifts = _repository.GetAll()
            .Where(s => s.Date() == date)
            .OrderBy(s => s.StartTime)
            .ToList();

        return Ok(DaySummaryDto.From(date, shifts));
    }

    // POST api/<ShiftsController>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public IActionResult Post([FromBody] ShiftDto request)
    {
        var shift = new Shift
        {
            StartTime = request.StartTime!.Value,
            EndTime = request.EndTime!.Value,
            Note = request.Note,
            LocationId = request.LocationId!.Value
        };

        if (!shift.IsValid())
        {
            return Problem(statusCode: StatusCodes.Status400BadRequest, detail: "End time must be after start time.");
        }

        if (_locationRepository.GetById(shift.LocationId) == null)
        {
            return Problem(statusCode: StatusCodes.Status400BadRequest, detail: $"Location {shift.LocationId} does not exist.");
        }

        var conflict = _repository.GetAll().FirstOrDefault(s => s.IsOverlapping(shift));
        if (conflict != null)
        {
            return Problem(statusCode: StatusCodes.Status409Conflict, detail: string.Create(CultureInfo.InvariantCulture,
                $"Shift overlaps with shift {conflict.Id} from {conflict.StartTime:yyyy-MM-dd HH:mm} to {conflict.EndTime:yyyy-MM-dd HH:mm}."));
        }

        _repository.Add(shift);
        _repository.Save();
        return CreatedAtAction(nameof(Get), new { id = shift.Id }, ShiftResponseDto.From(shift));
    }

    // PUT api/<ShiftsController>/5
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public IActionResult Put(int id, [FromBody] ShiftDto request)
    {
        var shift = new Shift
        {
            Id = id,
            StartTime = request.StartTime!.Value,
            EndTime = request.EndTime!.Value,
            Note = request.Note,
            LocationId = request.LocationId!.Value
        };

        if (_repository.GetById(id) == null)
        {
            return NotFound();
        }

        if (!shift.IsValid())
        {
            return Problem(statusCode: StatusCodes.Status400BadRequest, detail: "End time must be after start time.");
        }

        if (_locationRepository.GetById(shift.LocationId) == null)
        {
            return Problem(statusCode: StatusCodes.Status400BadRequest, detail: $"Location {shift.LocationId} does not exist.");
        }

        var conflict = _repository.GetAll().FirstOrDefault(s => s.Id != id && s.IsOverlapping(shift));
        if (conflict != null)
        {
            return Problem(statusCode: StatusCodes.Status409Conflict, detail: string.Create(CultureInfo.InvariantCulture,
                $"Shift overlaps with shift {conflict.Id} from {conflict.StartTime:yyyy-MM-dd HH:mm} to {conflict.EndTime:yyyy-MM-dd HH:mm}."));
        }

        _repository.Update(shift);
        _repository.Save();
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
