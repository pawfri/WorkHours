using WorkHoursLib.Models;

namespace WorkHoursLib.Services.Interfaces;

public interface IIntervalRepository
{
    Interval Add(Interval interval);
    Interval? Delete(int id);
    List<Interval> GetAll();
    Interval? GetById(int id);
    Interval Update(int id, Interval interval);
}