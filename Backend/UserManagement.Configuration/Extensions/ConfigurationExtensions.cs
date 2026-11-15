using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace UserManagement.Configuration.Extensions
{
    public static class ConfigurationExtensions
    {
        public static IServiceCollection AddConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<DatabaseSetting>(
              configuration.GetSection("ConnectionStrings")
            );
            services.Configure<InitialManagerSetting>(
               configuration.GetSection("InitialManager")
            );
            return services;

        }
    }
}