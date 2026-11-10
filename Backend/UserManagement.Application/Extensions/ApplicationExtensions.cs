using Microsoft.Extensions.DependencyInjection;
using UserManagement.Application.Validators;
using FluentValidation;
using UserManagement.Application.Mappings;

namespace UserManagement.Application.Extensions
{
    public static class ApplicationExtensions
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddValidatorsFromAssemblyContaining<UserModelValidator>();
            services.AddValidatorsFromAssemblyContaining<RegisterValidator>();
            services.AddValidatorsFromAssemblyContaining<LoginValidator>();
            services.AddAutoMapper(cfg => { }, typeof(UserMappingProfile));
            return services;
        }
    }
}