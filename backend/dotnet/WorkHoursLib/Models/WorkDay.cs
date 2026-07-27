using System.Xml.Linq;
using WorkHoursLib.Services.Interfaces;

namespace WorkHoursLib.Models;

public class WorkDay : IEntity
{
    public int Id { get; set; }
    public DateTime Date { get; set; }

    //Foreign Keys
    public int UserId { get; set; }

    public WorkDay() { }

    public override string ToString()
    {
        return $"Id: {Id}, Date: {Date}, UserId: {UserId}";
    }
}
