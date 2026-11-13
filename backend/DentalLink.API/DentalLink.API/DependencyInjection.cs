using DentalLink.Application;
using DentalLink.Infrastructure;
using Microsoft.Extensions.Configuration;

namespace DentalLink.API
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddAppDI(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddApplicationDI();
            services.AddInfrastructureDI(configuration);
            return services;
        }
    }
}
