namespace WorkHoursLib.Models;

public class WorkDay
{
    public int WorkDayId { get; set; }
    public DateTime Date { get; set; }

    //Foreign Keys
    public int UserId { get; set; }


    public WorkDay(int workDayId, DateTime date, int userId)
    {
        WorkDayId = workDayId;
        Date = date;
        UserId = userId;
    }

}
