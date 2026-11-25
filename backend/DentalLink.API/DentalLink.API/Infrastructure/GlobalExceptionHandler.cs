using DentalLink.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace DentalLink.API.Infrastructure;

public sealed class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger,
    IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (httpContext.Response.HasStarted)
        {
            return false;
        }

        var (statusCode, title, detail, extensions) = MapException(exception);

        if (statusCode >= StatusCodes.Status500InternalServerError)
        {
            logger.LogError(exception, "Unhandled server error on {Path}", httpContext.Request.Path);
        }
        else
        {
            logger.LogWarning(exception, "Client request error ({StatusCode}) on {Path}: {Message}",
                statusCode, httpContext.Request.Path, exception.Message);
        }

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = httpContext.Request.Path
        };

        if (extensions != null)
        {
            foreach (var (key, value) in extensions)
            {
                problem.Extensions[key] = value;
            }
        }

        httpContext.Response.StatusCode = statusCode;
        await problemDetailsService.WriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problem
        });

        return true;
    }

    private static (int StatusCode, string Title, string Detail, IDictionary<string, object?>? Extensions) MapException(Exception exception)
    {
        return exception switch
        {
            ValidationException valEx => (
                StatusCodes.Status400BadRequest,
                "Validation Error",
                valEx.Message,
                new Dictionary<string, object?> { ["errors"] = valEx.Errors }
            ),
            NotFoundException notFoundEx => (
                StatusCodes.Status404NotFound,
                "Resource Not Found",
                notFoundEx.Message,
                null
            ),
            ConflictException conflictEx => (
                StatusCodes.Status409Conflict,
                "Conflict",
                conflictEx.Message,
                null
            ),
            UnauthorizedException unauthEx => (
                StatusCodes.Status401Unauthorized,
                "Unauthorized",
                unauthEx.Message,
                null
            ),
            ForbiddenException forbidEx => (
                StatusCodes.Status403Forbidden,
                "Forbidden",
                forbidEx.Message,
                null
            ),
            _ => (
                StatusCodes.Status500InternalServerError,
                "Internal Server Error",
                "An unexpected server error occurred. Please contact clinic administration.",
                null
            )
        };
    }
}
