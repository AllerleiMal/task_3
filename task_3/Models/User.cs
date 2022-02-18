using System;

namespace task_3.Models
{
    public enum Status
    {
        Active = 1,
        Blocked = 2
    }
    
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public Status State { get; set; }
        public DateTime LastLoginDate { get; set; }
        public DateTime RegistrationDate { get; set; }
        public string Password { get; set; }

        public User(string name, string email, string password)
        {
            Name = name;
            Email = email;
            Password = password;
            RegistrationDate = DateTime.Now;
            LastLoginDate = DateTime.Now;
            State = Status.Active;
        }
    }
}