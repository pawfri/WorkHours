using System.ComponentModel.DataAnnotations;
using WorkHoursLib.Models;

namespace WorkHoursRest.Dtos;

public class CreateLocationRequest
{
    // [Required] rejects null, empty and whitespace-only names.
    [Required]
    [MaxLength(Location.NameMaxLength)]
    public string? Name { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? ZipCode { get; set; }
}
