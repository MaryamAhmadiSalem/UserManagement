using FluentValidation;
using UserManagement.Application.DTO;

namespace UserManagement.Application.Validators
{
    public class RegisterValidator : AbstractValidator<RegisterDto>
    {
        public RegisterValidator()
        {
            RuleFor(u => u.Email).NotEmpty().EmailAddress();
            RuleFor(u => u.Password).NotEmpty().Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z0-9]).{10}$");
            RuleFor(u => u.ConfirmPassword).NotEmpty().Equal(u=>u.Password);
        }
    }
}
