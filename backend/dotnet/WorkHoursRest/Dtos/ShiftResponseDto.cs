using WorkHoursLib.Models;

namespace WorkHoursRest.Dtos;

public class ShiftResponseDto
{
    public int Id { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string? Note { get; set; }
    public int LocationId { get; set; }
    public string Duration { get; set; } = string.Empty;
    public double DurationHours { get; set; }

    public static ShiftResponseDto From(Shift shift)
    {
        int totalMinutes = (int)Math.Round(shift.Duration().TotalMinutes);

        return new ShiftResponseDto
        {
            Id = shift.Id,
            StartTime = shift.StartTime,
            EndTime = shift.EndTime,
            Note = shift.Note,
            LocationId = shift.LocationId,
            Duration = $"{totalMinutes / 60}:{totalMinutes % 60:D2}",
            DurationHours = Math.Round(totalMinutes / 60.0, 2)
        };
    }

}
