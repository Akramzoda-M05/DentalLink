using DentalLink.Application.Common.Interfaces;
using DentalLink.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace DentalLink.Infrastructure.Services;

// -----------------------------------------------------------------------------
// 0. Redis Connection Provider
// -----------------------------------------------------------------------------
public interface IRedisConnectionProvider
{
    IConnectionMultiplexer? Connection { get; }
}

public class RedisConnectionProvider : IRedisConnectionProvider
{
    public IConnectionMultiplexer? Connection { get; }

    public RedisConnectionProvider(IConfiguration configuration, ILogger<RedisConnectionProvider> logger)
    {
        var redisConnection = configuration.GetConnectionString("Redis") ?? "localhost:6379";
        try
        {
            var options = ConfigurationOptions.Parse(redisConnection);
            options.AbortOnConnectFail = false;
            options.ConnectTimeout = 2000;
            Connection = ConnectionMultiplexer.Connect(options);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Could not establish initial connection to Redis ({Connection}). Falling back to memory.", redisConnection);
            Connection = null;
        }
    }
}

// -----------------------------------------------------------------------------
// 1. Password Hasher (BCrypt compatible)
// -----------------------------------------------------------------------------
public class PasswordHasher : IPasswordHasher
{
    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
    }

    public bool VerifyPassword(string password, string passwordHash)
    {
        try
        {
            return BCrypt.Net.BCrypt.Verify(password, passwordHash);
        }
        catch
        {
            return false;
        }
    }
}

// -----------------------------------------------------------------------------
// 2. JWT Token Service
// -----------------------------------------------------------------------------
public class JwtTokenService : IJwtTokenService
{
    private readonly IConfiguration _configuration;

    public JwtTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateAccessToken(User user)
    {
        var secret = _configuration["Jwt:SecretKey"] ?? "DentalLink_Commercial_Super_Secure_Secret_Key_For_Jwt_Signing_2026_Enterprise!";
        var issuer = _configuration["Jwt:Issuer"] ?? "DentalLink.API";
        var audience = _configuration["Jwt:Audience"] ?? "DentalLink.Client";
        var expiryMinutes = int.TryParse(_configuration["Jwt:ExpiryMinutes"], out var m) ? m : 60;

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, user.Name)
        };

        //if (user.DoctorProfile != null)
        //{
        //    claims.Add(new Claim("doctorId", user.DoctorProfile.Id.ToString()));
        //}

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(expiryMinutes),
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = credentials
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    public string HashToken(string token)
    {
        var bytes = Encoding.UTF8.GetBytes(token);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash);
    }
}

// -----------------------------------------------------------------------------
// 3. Distributed Lock Service (Redis with In-Memory Resilient Fallback)
// -----------------------------------------------------------------------------
public class DistributedLockService : IDistributedLockService
{
    private readonly IRedisConnectionProvider _connectionProvider;
    private readonly ILogger<DistributedLockService> _logger;
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> MemoryLocks = new();

    public DistributedLockService(IRedisConnectionProvider connectionProvider, ILogger<DistributedLockService> logger)
    {
        _connectionProvider = connectionProvider;
        _logger = logger;
    }

    public async Task<IDisposable?> TryAcquireLockAsync(
        string resourceKey,
        TimeSpan expiry,
        TimeSpan waitTimeout,
        CancellationToken cancellationToken = default)
    {
        var redis = _connectionProvider.Connection;
        if (redis != null && redis.IsConnected)
        {
            try
            {
                var db = redis.GetDatabase();
                var lockValue = Guid.NewGuid().ToString();
                var startTime = DateTime.UtcNow;

                while (DateTime.UtcNow - startTime < waitTimeout)
                {
                    if (await db.LockTakeAsync(resourceKey, lockValue, expiry))
                    {
                        return new RedisLockReleaser(db, resourceKey, lockValue);
                    }
                    await Task.Delay(50, cancellationToken);
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Redis lock failed for {Key}, falling back to in-memory lock.", resourceKey);
            }
        }

        // In-memory fallback
        var semaphore = MemoryLocks.GetOrAdd(resourceKey, _ => new SemaphoreSlim(1, 1));
        var acquired = await semaphore.WaitAsync(waitTimeout, cancellationToken);
        if (acquired)
        {
            return new MemoryLockReleaser(semaphore);
        }

        return null;
    }

    private sealed class RedisLockReleaser : IDisposable
    {
        private readonly IDatabase _database;
        private readonly string _key;
        private readonly string _value;
        private bool _disposed;

        public RedisLockReleaser(IDatabase database, string key, string value)
        {
            _database = database;
            _key = key;
            _value = value;
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _database.LockRelease(_key, _value);
                _disposed = true;
            }
        }
    }

    private sealed class MemoryLockReleaser : IDisposable
    {
        private readonly SemaphoreSlim _semaphore;
        private bool _disposed;

        public MemoryLockReleaser(SemaphoreSlim semaphore)
        {
            _semaphore = semaphore;
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _semaphore.Release();
                _disposed = true;
            }
        }
    }
}

// -----------------------------------------------------------------------------
// 4. Redis Cache Service (with In-Memory Fallback)
// -----------------------------------------------------------------------------
public class RedisCacheService : IRedisCacheService
{
    private readonly IRedisConnectionProvider _connectionProvider;
    private readonly ILogger<RedisCacheService> _logger;
    private static readonly ConcurrentDictionary<string, (object Value, DateTimeOffset ExpiresAt)> MemoryCache = new();

    public RedisCacheService(IRedisConnectionProvider connectionProvider, ILogger<RedisCacheService> logger)
    {
        _connectionProvider = connectionProvider;
        _logger = logger;
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        var redis = _connectionProvider.Connection;
        if (redis != null && redis.IsConnected)
        {
            try
            {
                var db = redis.GetDatabase();
                var data = await db.StringGetAsync(key);
                if (data.HasValue)
                {
                    return JsonSerializer.Deserialize<T>(data.ToString()!);
                }
                return default;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to read cache key {Key} from Redis.", key);
            }
        }

        if (MemoryCache.TryGetValue(key, out var cached))
        {
            if (cached.ExpiresAt > DateTimeOffset.UtcNow)
            {
                return (T)cached.Value;
            }
            MemoryCache.TryRemove(key, out _);
        }

        return default;
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken cancellationToken = default)
    {
        var exp = expiry ?? TimeSpan.FromMinutes(10);
        var redis = _connectionProvider.Connection;

        if (redis != null && redis.IsConnected)
        {
            try
            {
                var db = redis.GetDatabase();
                var json = JsonSerializer.Serialize(value);
                await db.StringSetAsync(key, json, exp);
                return;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to set cache key {Key} in Redis.", key);
            }
        }

        MemoryCache[key] = (value!, DateTimeOffset.UtcNow.Add(exp));
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        var redis = _connectionProvider.Connection;
        if (redis != null && redis.IsConnected)
        {
            try
            {
                var db = redis.GetDatabase();
                await db.KeyDeleteAsync(key);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to delete cache key {Key} from Redis.", key);
            }
        }

        MemoryCache.TryRemove(key, out _);
    }
}

// -----------------------------------------------------------------------------
// 5. Telegram Notification Service
// -----------------------------------------------------------------------------
public class TelegramNotificationService : ITelegramNotificationService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<TelegramNotificationService> _logger;

    public TelegramNotificationService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<TelegramNotificationService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<bool> SendMessageAsync(Guid chatId, string text, CancellationToken cancellationToken = default)
    {
        var botToken = _configuration["Telegram:BotToken"];
        if (string.IsNullOrWhiteSpace(botToken) || botToken.Contains("YOUR_"))
        {
            _logger.LogInformation("[TELEGRAM MOCK] Message to Chat {ChatId}: {Text}", chatId, text);
            return true;
        }

        try
        {
            var url = $"https://api.telegram.org/bot{botToken}/sendMessage";
            var payload = new
            {
                chat_id = chatId,
                text = text,
                parse_mode = "HTML"
            };

            var json = JsonSerializer.Serialize(payload);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(url, content, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning("Telegram API returned {StatusCode}: {ErrorBody}", response.StatusCode, errorBody);
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error dispatching Telegram message to {ChatId}", chatId);
            return false;
        }
    }
}
