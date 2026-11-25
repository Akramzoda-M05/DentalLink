using DentalLink.API.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using Serilog;
using System.Security.Claims;
using System.Text;
using System.Threading.RateLimiting;

namespace DentalLink.API;

public static class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // 1. Serilog Structured Logging
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration)
            .Enrich.FromLogContext()
            .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
            .WriteTo.File("logs/dentallink-.log", rollingInterval: RollingInterval.Day)
            .CreateLogger();

        builder.Host.UseSerilog();

        // 2. Dependency Injection
        builder.Services.AddAppDI(builder.Configuration);
        builder.Services.AddProblemDetails();
        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

        // 3. Authentication & Authorization Policies
        var jwtSecret = builder.Configuration["Jwt:SecretKey"] ?? "DentalLink_Commercial_Super_Secure_Secret_Key_For_Jwt_Signing_2026_Enterprise!";
        var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "DentalLink.API";
        var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "DentalLink.Client";

        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = false;
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
                ValidateIssuer = true,
                ValidIssuer = jwtIssuer,
                ValidateAudience = true,
                ValidAudience = jwtAudience,
                ClockSkew = TimeSpan.FromMinutes(1),
                RoleClaimType = ClaimTypes.Role,
                NameClaimType = ClaimTypes.Name
            };
        });

        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy("AdminOnly", policy => policy.RequireRole("admin"));
            options.AddPolicy("StaffOnly", policy => policy.RequireRole("admin", "receptionist"));
            options.AddPolicy("DoctorOrStaff", policy => policy.RequireRole("admin", "receptionist", "doctor"));
        });

        // 4. Health Checks
        builder.Services.AddHealthChecks()
            .AddCheck<DatabaseHealthCheck>("postgresql", tags: ["ready"]);

        // 5. CORS
        var allowedOrigins = builder.Configuration
            .GetSection("Cors:AllowedOrigins")
            .Get<string[]>() ?? [];

        if (builder.Environment.IsDevelopment())
        {
            allowedOrigins = allowedOrigins
                .Concat(["http://localhost:5173", "http://127.0.0.1:5173", "http://localhost:3000"])
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }

        builder.Services.AddCors(options =>
            options.AddPolicy("Frontend", policy =>
            {
                if (allowedOrigins.Length > 0)
                {
                    policy.WithOrigins(allowedOrigins)
                        .WithMethods("GET", "POST", "PUT", "PATCH", "DELETE", "OPTIONS")
                        .WithHeaders("Authorization", "Content-Type", "X-CSRF", "Idempotency-Key", "Accept")
                        .AllowCredentials();
                }
                else
                {
                    policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
                }
            }));

        // 6. Rate Limiter
        builder.Services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 120,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0,
                        AutoReplenishment = true
                    }));
        });

        builder.Services.AddControllers();
        builder.Services.AddOpenApi();
        builder.Services.AddEndpointsApiExplorer();

        var app = builder.Build();

        // Database seeding and verification
        await DatabaseInitializer.InitializeDatabaseAsync(app.Services);

        // Middleware Pipeline
        app.UseExceptionHandler();

        app.Use(async (context, next) =>
        {
            context.Response.Headers["X-Content-Type-Options"] = "nosniff";
            context.Response.Headers["X-Frame-Options"] = "DENY";
            context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
            context.Response.Headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";
            await next();
        });

        if (builder.Configuration.GetValue<bool>("HttpsRedirection:Enabled"))
        {
            app.UseHsts();
            app.UseHttpsRedirection();
        }

        if (app.Environment.IsDevelopment())
        {
             app.MapOpenApi();
            app.MapScalarApiReference(options =>
            {
                options.WithTitle("DentalLink Clinic Web API")
                       .WithTheme(ScalarTheme.BluePlanet)
                       .WithDefaultHttpClient(ScalarTarget.JavaScript, ScalarClient.Axios);
            });

        }
        app.UseCors("Frontend");
        app.UseRateLimiter();


        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        app.MapHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = _ => false
        }).DisableRateLimiting();

        app.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("ready")
        }).DisableRateLimiting();

        await app.RunAsync();
    }
}
