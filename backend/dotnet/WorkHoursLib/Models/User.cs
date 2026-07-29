using WorkHoursLib.Services.Interfaces;


namespace WorkHoursLib.Models;

public class User : IEntity
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
