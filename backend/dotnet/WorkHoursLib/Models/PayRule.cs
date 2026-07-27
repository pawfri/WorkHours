using System;
using System.Collections.Generic;
using System.Text;
using WorkHoursLib.Enums;

namespace WorkHoursLib.Models;

public class PayRule
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public ModifierType ModifierType { get; set; }
    public double ModifierValue { get; set; }
    public DateTime EffectiveFrom { get; set; }
    public DateTime EffectiveTo { get; set; }


    public override string ToString()
    {
        return $"Id: {Id}, Name: {Name}, Description: {Description}, ModifierType: {ModifierType}, ModifierValue: {ModifierValue}, EffectiveFrom: {EffectiveFrom}, EffectiveTo: {EffectiveTo}";
    }
}
