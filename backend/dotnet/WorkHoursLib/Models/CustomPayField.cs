using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using WorkHoursLib.Enums;

namespace WorkHoursLib.Models;

public class CustomPayField
{
    public int Id { get; set; }
    public int PayDetailsId { get; set; }
    public string FieldName { get; set; }
    public string FieldType { get; set; }
    public double Value { get; set; }
    public DateTime EffectiveFrom { get; set; }
    public DateTime EffectiveTo { get; set; }


    public override string ToString()
    {
        return $"Id: {Id}, PayDetailsId: {PayDetailsId}, FieldName: {FieldName}, FieldType: {FieldType}, Value: {Value}, EffectiveFrom: {EffectiveFrom}, EffectiveTo: {EffectiveTo}";
    }
}
