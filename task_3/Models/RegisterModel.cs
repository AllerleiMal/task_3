using System.ComponentModel.DataAnnotations;

namespace task_3.Models
{
    public class RegisterModel
    {
        [Required(ErrorMessage = "Name can't be empty")]
        public string Name { get; set; }
        [Required(ErrorMessage ="Email can't be empty")]
        public string Email { get; set; }
         
        [Required(ErrorMessage = "Password can't be empty")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}