using WorkHoursLib.Services.Interfaces;

namespace WorkHoursLib.Models;

public class Location : IEntity
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Address { get; set; }
    public string City { get; set; }
    public string ZipCode { get; set; }


    public override string ToString()
    {
        return $"Id: {Id}, Name: {Name}, Address: {Address}, City: {City}, ZipCode: {ZipCode}";
    }
}
