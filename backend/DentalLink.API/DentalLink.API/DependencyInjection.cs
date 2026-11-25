using DentalLink.API.Infrastructure;
using DentalLink.Application;
using DentalLink.Application.Common.Interfaces;
using DentalLink.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DentalLink.API;

public static class DependencyInjection
{
    public static IServiceCollection AddAppDI(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        services.AddApplicationDI();
        services.AddInfrastructureDI(configuration);

        return services;
    }
}
