using Microsoft.Extensions.DependencyInjection;

namespace DentalLink.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationDI(this IServiceCollection services)
        {
            // MediatR, FluentValidation, AutoMapper и другие сервисы прикладного слоя
            return services;
        }
    }
}
