using System.ComponentModel.DataAnnotations;

namespace FlowerAura.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;


        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]

        public string Password { get; set; } = string.Empty;

        public string? OTP { get; set; }
    }
}