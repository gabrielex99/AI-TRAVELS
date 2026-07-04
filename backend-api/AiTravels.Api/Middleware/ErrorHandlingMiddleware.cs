using System.Net;
using System.Text.Json;

namespace AiTravels.Api.Middleware;

/// <summary>
/// Global exception handling middleware.
/// Catches unhandled exceptions and returns structured JSON error responses.
/// </summary>
public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Unhandled exception caught by error handling middleware. " +
                "Path: {Path}, Method: {Method}",
                context.Request.Path, context.Request.Method);

            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, message) = exception switch
        {
            ArgumentException => (HttpStatusCode.BadRequest, exception.Message),
            KeyNotFoundException => (HttpStatusCode.NotFound, exception.Message),
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized, "Unauthorized access."),
            TimeoutException => (HttpStatusCode.GatewayTimeout, "The request timed out."),
            HttpRequestException httpEx => (
                httpEx.StatusCode ?? HttpStatusCode.BadGateway,
                $"External service error: {httpEx.Message}"),
            InvalidOperationException => (HttpStatusCode.InternalServerError, exception.Message),
            _ => (HttpStatusCode.InternalServerError, "An unexpected error occurred.")
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var errorResponse = new
        {
            error = message,
            status = (int)statusCode,
            timestamp = DateTime.UtcNow.ToString("o"),
            path = context.Request.Path.ToString()
        };

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
        };

        var json = JsonSerializer.Serialize(errorResponse, jsonOptions);
        await context.Response.WriteAsync(json);
    }
}

/// <summary>
/// Extension method for registering the error handling middleware.
/// </summary>
public static class ErrorHandlingMiddlewareExtensions
{
    public static IApplicationBuilder UseErrorHandling(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ErrorHandlingMiddleware>();
    }
}
