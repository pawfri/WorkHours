using System;
using System.Collections.Generic;
using System.Text;
using WorkHoursLib.Enums;

namespace WorkHoursLib.Models;

public class Location
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
