using System.ComponentModel.DataAnnotations;
using WorkHoursLib.Models;

namespace WorkHoursRest.Dtos;

// Used for both creating and updating a location.
public class LocationDto : IValidatableObject
{
    // [Required] rejects null, empty and whitespace-only names.
    [Required]
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? ZipCode { get; set; }

    // The length is checked on the trimmed name, because that is what gets stored.
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Name.Trim().Length > Location.NameMaxLength)
        {
            yield return new ValidationResult(
                $"Name must be at most {Location.NameMaxLength} characters.",
                [nameof(Name)]);
        }
    }
}
