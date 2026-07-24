using System;
using System.Collections.Generic;
using System.Text;

namespace WorkHoursLib.Enums;

public enum ModifierType
{
    HourlyFlatBonus,    //Add X per hour worked
    HourlyMultiplier,   //Multiply hourly rate by X
    FixedShiftBonus,    //Add X for each shift worked
}
