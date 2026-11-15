using DentalLink.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DentalLink.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureDI(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException(
                    "ConnectionStrings:DefaultConnection must be configured through the environment.");
            }

            services.AddDbContext<DentalLinkDbContext>(options =>
                options.UseNpgsql(connectionString, npgsql =>
                    npgsql.EnableRetryOnFailure(3)));

            return services;
        }
    }
}
