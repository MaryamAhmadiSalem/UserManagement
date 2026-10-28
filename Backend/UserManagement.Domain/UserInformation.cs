using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace UserManagement.Domain
{
    [Index(nameof(NationalId), IsUnique = true)]
    [Index(nameof(Email), IsUnique = true)] 
    public class UserInformation
    {
        [Key]
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string NationalId { get; set; }
        public DateOnly DateOfBirth { get; set; }
        public string Job {  get; set; }
        public string Education { get; set; }
        public string Email { get; set; }
    }
}