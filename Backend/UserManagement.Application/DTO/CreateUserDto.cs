namespace UserManagement.Application.DTO
{
    public class CreateUserDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string NationalId { get; set; }
        public DateOnly DateOfBirth { get; set; }
        public string Job { get; set; }
        public string Education { get; set; }
        public string Email { get; set; }
    }
}