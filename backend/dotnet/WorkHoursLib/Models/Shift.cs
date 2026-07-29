using WorkHoursLib.Services.Interfaces;

namespace WorkHoursLib.Models;

public class Shift : IEntity
{
    public int Id { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }

    //Foreign Keys
    public int UserId { get; set; }
    public int LocationId { get; set; }

    public override string ToString()
    {
        return $"Id: {Id}, StartTime: {StartTime}, EndTime: {EndTime}, UserId: {UserId}, LocationId: {LocationId}";
    }

}
