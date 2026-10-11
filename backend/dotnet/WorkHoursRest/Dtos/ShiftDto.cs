using System.ComponentModel.DataAnnotations;
using WorkHoursLib.Models;

namespace WorkHoursRest.Dtos;

public class ShiftDto
{
    [Required]
    public int? LocationId { get; set; }
    [Required]
    public DateTime? StartTime { get; set; }
    [Required]
    public DateTime? EndTime { get; set; }
    [MaxLength(Shift.NoteMaxLength)]
    public string? Note { get; set; }

}
