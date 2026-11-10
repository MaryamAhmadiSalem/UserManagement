using FluentValidation;
using UserManagement.Application.DTO;

namespace UserManagement.Application.Validators
{
    public class UserModelValidator : AbstractValidator<CreateUserDto>
    {
        public UserModelValidator()
        {
            RuleFor(f => f.FirstName).NotEmpty().MaximumLength(20);
            RuleFor(l => l.LastName).NotEmpty().MaximumLength(30);
            RuleFor(n => n.NationalId).NotEmpty().Matches(@"^[0-9]{10}$");
            //RuleFor(d => d.DateOfBirth).NotEmpty().LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.Today));
            RuleFor(d => d.DateOfBirth).NotEmpty().LessThanOrEqualTo(new DateOnly(2005, 12, 31));
            RuleFor(j => j.Job).NotEmpty().MaximumLength(50);
            RuleFor(e => e.Education).NotEmpty();
            RuleFor(e => e.Email).NotEmpty().EmailAddress();
        }
    }
}