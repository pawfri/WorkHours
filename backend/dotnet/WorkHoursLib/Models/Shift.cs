using System.ComponentModel.DataAnnotations;
using WorkHoursLib.Services.Interfaces;

namespace WorkHoursLib.Models;

public class Shift : IEntity
{
   public const int NoteMaxLength = 500;

    public int Id { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }

    [MaxLength(NoteMaxLength)]
    public string? Note { get; set; }

    //Foreign Keys
    public int UserId { get; set; }
    public int LocationId { get; set; }

    public override string ToString()
    {
        return $"Id: {Id}, StartTime: {StartTime}, EndTime: {EndTime}, UserId: {UserId}, LocationId: {LocationId}";
    }

    public double TotalHours()
    {
        return (EndTime - StartTime).TotalHours;
    }

    public TimeSpan Duration()
    {
        return EndTime - StartTime;
    }

    public bool IsValid()
    {
        return EndTime > StartTime;
    }

    public bool IsOverlapping(Shift otherShift)
    {
        return StartTime < otherShift.EndTime && EndTime > otherShift.StartTime;
    }

    public DateOnly Date()
    {
        return DateOnly.FromDateTime(StartTime);
    }

    public static TimeSpan TotalDuration(IEnumerable<Shift> shifts)
    {
        return shifts.Aggregate(TimeSpan.Zero, (total, shift) => total + shift.Duration());
    }

}
