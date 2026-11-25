namespace DentalLink.Application.Common.Interfaces;

public interface IDistributedLockService
{
    Task<IDisposable?> TryAcquireLockAsync(
        string resourceKey,
        TimeSpan expiry,
        TimeSpan waitTimeout,
        CancellationToken cancellationToken = default);
}

public interface IRedisCacheService
{
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);
    Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken cancellationToken = default);
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
}

public interface IJwtTokenService
{
    string GenerateAccessToken(DentalLink.Domain.Entities.User user);
    string GenerateRefreshToken();
    string HashToken(string token);
}

public interface IPasswordHasher
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string passwordHash);
}

public interface ICurrentUserService
{
    Guid? UserId { get; }
    string? Email { get; }
    string? Role { get; }
    bool IsAuthenticated { get; }
}

public interface ITelegramNotificationService
{
    Task<bool> SendMessageAsync(Guid chatId, string text, CancellationToken cancellationToken = default);
}
