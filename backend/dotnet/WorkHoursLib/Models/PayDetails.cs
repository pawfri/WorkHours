using System;
using System.Collections.Generic;
using System.Text;
using WorkHoursLib.Enums;

namespace WorkHoursLib.Models;

public class PayDetails
{
    public int PayDetailsId { get; set; }
    public double HourlyRate { get; set; }
    public double MonthlySalary { get; set; }
    public double TaxRate { get; set; }
    public double OvertimeRate { get; set; }
    public double Bonus { get; set; }
    public double DeductionRate { get; set; }
    public double EmployeePensionRate { get; set; }
    public double EmployerPensionRate { get; set; }
    public double VacationRate { get; set; }
    public DateTime EffectiveFrom { get; set; }
    public DateTime EffectiveTo { get; set; }
    public SalaryType SalaryType { get; set; }
    
    //Foreign Keys
    public int UserId { get; set; }
    public int LocationId { get; set; }


    public PayDetails(int payDetailsId, double hourlyRate, double monthlySalary, double taxRate, double overtimeRate, 
                      double bonus, double deductionRate, double employeePensionRate, double employerPensionRate, double vacationRate, 
                      DateTime effectiveFrom, DateTime effectiveTo, SalaryType salaryType, int userId, int locationId)
    {
        PayDetailsId = payDetailsId;
        HourlyRate = hourlyRate;
        MonthlySalary = monthlySalary;
        TaxRate = taxRate;
        OvertimeRate = overtimeRate;
        Bonus = bonus;
        DeductionRate = deductionRate;
        EmployeePensionRate = employeePensionRate;
        EmployerPensionRate = employerPensionRate;
        VacationRate = vacationRate;
        EffectiveFrom = effectiveFrom;
        EffectiveTo = effectiveTo;
        SalaryType = salaryType;
        UserId = userId;
        LocationId = locationId;
    }

}
