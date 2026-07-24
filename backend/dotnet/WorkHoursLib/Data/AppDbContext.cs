using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using WorkHoursLib.Models;

namespace WorkHoursLib.Data;

public class AppDbContext : DbContext
{
    public DbSet<Interval> Intervals { get; set; }
}
