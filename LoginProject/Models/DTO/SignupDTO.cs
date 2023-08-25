using System.ComponentModel.DataAnnotations;

namespace LoginProject.Models.DTO
{
    public class SignupDTO
    {
        [Required]
        public string Name { get; set; } = null!;
        [Required]
        public string LastName { get; set; } = null!;
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;
        [Required]
        public string Password { get; set; } = null!;
        [Required]
        public long Phone { get; set; }
    }
}
