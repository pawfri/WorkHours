using System.ComponentModel.DataAnnotations;
using WorkHoursLib.Services.Interfaces;

namespace WorkHoursLib.Models;

public class Location : IEntity
{
    public const int NameMaxLength = 100;

    public int Id { get; set; }

    [MaxLength(NameMaxLength)]
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? ZipCode { get; set; }

    /// <summary>
    /// Normalizes the string properties by trimming whitespace and setting empty strings to null.
    /// </summary>
    public void Normalize()
    {
        Name = Name?.Trim() ?? string.Empty;
        Address = string.IsNullOrWhiteSpace(Address) ? null : Address.Trim();
        City = string.IsNullOrWhiteSpace(City) ? null : City.Trim();
        ZipCode = string.IsNullOrWhiteSpace(ZipCode) ? null : ZipCode.Trim();
    }

    public override string ToString()
    {
        return $"Id: {Id}, Name: {Name}, Address: {Address}, City: {City}, ZipCode: {ZipCode}";
    }
}
