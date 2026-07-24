using System;
using System.Collections.Generic;
using System.Text;

namespace WorkHoursLib.Models;

public class Interval
{

    public int IntervalId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }

    //Foreign Keys
    public int WorkDayId { get; set; }
    public int LocationId { get; set; }


    public Interval(int intervalId, DateTime startTime, DateTime endTime, int workDayId, int locationId)
    {
        IntervalId = intervalId;
        StartTime = startTime;
        EndTime = endTime;
        WorkDayId = workDayId;
        LocationId = locationId;
    }
}
