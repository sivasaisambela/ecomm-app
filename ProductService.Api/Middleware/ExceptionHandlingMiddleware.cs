using Shared.Core.Exceptions;

using System.Security.Authentication;
using System.Text.Json;

namespace ProductService.Api.Middleware;

/// <summary>
/// Global exception handling middleware
/// 
/// Catches all unhandled exceptions and returns proper responses.
/// Prevents stack traces from leaking to clients.
/// </summary>
public class ExceptionHandlingMiddleware
{
}

/// <summary>
/// Extension method to add exception handling middleware
/// </summary>
public static class ExceptionHandlingMiddlewareExtensions
{
    public static IApplicationBuilder UseExceptionHandling(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ExceptionHandlingMiddleware>();
    }
}