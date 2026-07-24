using System;
using System.Collections.Generic;
using System.Text;
using WorkHoursLib.Models;
using WorkHoursLib.Services.Interfaces;

namespace WorkHoursLib.Services.Repositories;

public class IntervalRepository : IIntervalRepository
{
    private readonly List<Interval> _intervals = new();
    private int _nextId = 1;

    public List<Interval> GetAll()
    {
        return _intervals;
    }

    public Interval? GetById(int id)
    {
        return _intervals.FirstOrDefault(i => i.IntervalId == id);
    }

    public Interval Add(Interval interval)
    {
        interval.IntervalId = _nextId++;
        _intervals.Add(interval);
        return interval;
    }

    public Interval? Delete(int id)
    {
        var interval = GetById(id);
        if (interval != null)
        {
            _intervals.Remove(interval);
        }
        return interval;
    }

    public Interval Update(int id, Interval interval)
    {
        Interval? existingInterval = GetById(id);

        if (existingInterval == null)
        {
            return null;
        }

        existingInterval.StartTime = interval.StartTime;
        existingInterval.EndTime = interval.EndTime;

        return existingInterval;
    }

}