using System;
using System.Collections.Generic;
using System.Text;

namespace WorkHoursLib.Models;

public class User
{
    public int UserId { get; set; }
    public string UserName { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }


    public User(int userId, string userName, string email, string password)
    {
        UserId = userId;
        UserName = userName;
        Email = email;
        Password = password;
    }

}
