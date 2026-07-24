using System;
using System.Collections.Generic;
using System.Text;

namespace WorkHoursLib.Models;

public class Location
{
    public int LocationId { get; set; }
    public string Name { get; set; }
    public string Address { get; set; }
    public string City { get; set; }
    public string ZipCode { get; set; }


    public Location(int locationId, string name, string address, string city, string zipCode)
    {
        LocationId = locationId;
        Name = name;
        Address = address;
        City = city;
        ZipCode = zipCode;
    }
}
