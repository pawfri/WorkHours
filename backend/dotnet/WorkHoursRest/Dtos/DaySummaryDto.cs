using WorkHoursLib.Models;

namespace WorkHoursRest.Dtos;

public class DaySummaryDto
{
    public DateOnly Date { get; set; }
    public List<ShiftResponseDto> Shifts { get; set; } = [];
    public string Duration { get; set; } = string.Empty;
    public double DurationHours { get; set; }

    public static DaySummaryDto From(DateOnly date, List<Shift> shifts)
    {
        int totalMinutes = (int)Math.Round(Shift.TotalDuration(shifts).TotalMinutes);

        return new DaySummaryDto
        {
            Date = date,
            Shifts = shifts.Select(ShiftResponseDto.From).ToList(),
            Duration = $"{totalMinutes / 60}:{totalMinutes % 60:D2}",
            DurationHours = Math.Round(totalMinutes / 60.0, 2)
        };
    }

}
