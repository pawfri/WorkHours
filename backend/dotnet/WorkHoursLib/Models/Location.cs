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


    public override string ToString()
    {
        return $"Id: {Id}, Name: {Name}, Address: {Address}, City: {City}, ZipCode: {ZipCode}";
    }
}
