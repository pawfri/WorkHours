using WorkHoursLib.Enums;
using WorkHoursLib.Services.Interfaces;

namespace WorkHoursLib.Models;

public class PayDetail : IEntity
{
    public int Id { get; set; }
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


    public override string ToString()
    {
        return $"Id: {Id}, HourlyRate: {HourlyRate}, MonthlySalary: {MonthlySalary}, TaxRate: {TaxRate}, OvertimeRate: {OvertimeRate}, Bonus: {Bonus}, DeductionRate: {DeductionRate}, EmployeePensionRate: {EmployeePensionRate}, EmployerPensionRate: {EmployerPensionRate}, VacationRate: {VacationRate}, EffectiveFrom: {EffectiveFrom}, EffectiveTo: {EffectiveTo}, SalaryType: {SalaryType}, UserId: {UserId}, LocationId: {LocationId}";
    }
}
