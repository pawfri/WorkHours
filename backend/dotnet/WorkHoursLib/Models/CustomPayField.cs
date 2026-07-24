using System;
using System.Collections.Generic;
using System.Text;

namespace WorkHoursLib.Models;

public class CustomPayField
{
    public int CustomPayFieldId { get; set; }
    public int PayDetailsId { get; set; }
    public string FieldName { get; set; }
    public string FieldType { get; set; }
    public double Value { get; set; }
    public DateTime EffectiveFrom { get; set; }
    public DateTime EffectiveTo { get; set; }


    public CustomPayField(int customPayFieldId, int payDetailsId, string fieldName, string fieldType, double value, DateTime effectiveFrom, DateTime effectiveTo)
    {
        CustomPayFieldId = customPayFieldId;
        PayDetailsId = payDetailsId;
        FieldName = fieldName;
        FieldType = fieldType;
        Value = value;
        EffectiveFrom = effectiveFrom;
        EffectiveTo = effectiveTo;
    }
}
