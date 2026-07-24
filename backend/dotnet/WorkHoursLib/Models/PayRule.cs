using System;
using System.Collections.Generic;
using System.Text;
using WorkHoursLib.Enums;

namespace WorkHoursLib.Models;

public class PayRule
{
    public int PayRuleId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public ModifierType ModifierType { get; set; }
    public double ModifierValue { get; set; }
    public DateTime EffectiveFrom { get; set; }
    public DateTime EffectiveTo { get; set; }


    public PayRule(int payRuleId, string name, string description, ModifierType modifierType, double modifierValue, DateTime effectiveFrom, DateTime effectiveTo)
    {
        PayRuleId = payRuleId;
        Name = name;
        Description = description;
        ModifierType = modifierType;
        ModifierValue = modifierValue;
        EffectiveFrom = effectiveFrom;
        EffectiveTo = effectiveTo;
    }
}
