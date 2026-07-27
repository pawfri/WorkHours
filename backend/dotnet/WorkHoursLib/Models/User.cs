using System;
using System.Collections.Generic;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace WorkHoursLib.Models;

public class User
{
    public int Id { get; set; }
    public string UserName { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }


    public override string ToString()
    {
        return $"Id: {Id}, UserName: {UserName}, Email: {Email}";
    }

}
