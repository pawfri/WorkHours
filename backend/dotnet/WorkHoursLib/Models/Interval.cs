using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using WorkHoursLib.Enums;

namespace WorkHoursLib.Models;

public class Interval
{

    public int Id { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }

    //Foreign Keys
    public int WorkDayId { get; set; }
    public int LocationId { get; set; }


    public override string ToString()
    {
        return $"Id: {Id}, StartTime: {StartTime}, EndTime: {EndTime}, WorkDayId: {WorkDayId}, LocationId: {LocationId}";
    }
}
