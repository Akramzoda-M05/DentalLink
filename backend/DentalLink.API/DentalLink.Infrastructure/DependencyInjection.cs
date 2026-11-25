using DentalLink.Application.Common.Interfaces;
using DentalLink.Infrastructure.BackgroundServices;
using DentalLink.Infrastructure.Data;
using DentalLink.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DentalLink.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureDI(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // 1. PostgreSQL EF Core DbContext
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            connectionString = "Host=localhost;Port=5432;Database=dentallink_db;Username=postgres;Password=postgres";
        }

        services.AddDbContext<DentalLinkDbContext>(options =>
            options.UseNpgsql(connectionString, npgsql =>
                npgsql.EnableRetryOnFailure(3)));

        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<DentalLinkDbContext>());

        // 2. Redis Connection Provider (resilient: falls back to in-memory if disconnected)
        services.AddSingleton<IRedisConnectionProvider, RedisConnectionProvider>();

        // 3. Infrastructure services
        services.AddSingleton<IDistributedLockService, DistributedLockService>();
        services.AddSingleton<IRedisCacheService, RedisCacheService>();
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<IJwtTokenService, JwtTokenService>();

        // 4. Telegram Notification Service + HttpClient
        services.AddHttpClient<ITelegramNotificationService, TelegramNotificationService>();

        // 5. Background Hosted Service for Telegram Reminders
        services.AddHostedService<TelegramReminderWorker>();

        return services;
    }
}
