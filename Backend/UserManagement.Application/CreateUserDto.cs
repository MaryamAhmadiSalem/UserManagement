using System.ComponentModel.DataAnnotations;

namespace UserManagement.Application
{
    public class CreateUserDto
    {
        [Required, StringLength(20)]
        public string FirstName { get; set; }

        [Required, StringLength(30)]
        public string LastName { get; set; }

        [Required, StringLength(10, MinimumLength = 10)]
        [RegularExpression(@"^[0-9]{10}$", ErrorMessage = "The national ID must be exactly 10 digits.")]
        public string NationalId { get; set; }

        [Required]
        public DateOnly DateOfBirth { get; set; }

        [StringLength(50)]
        public string Job { get; set; }

        [StringLength(50)]
        public string Education { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }
    }
}