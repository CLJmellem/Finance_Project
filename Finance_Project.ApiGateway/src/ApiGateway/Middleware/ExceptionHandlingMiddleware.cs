using System.Net;
using System.Text.Json;
using Polly.CircuitBreaker;

namespace ApiGateway.Middleware;

/// <summary>
/// Middleware global para tratamento de exceções e respostas padronizadas de erro.
/// </summary>
public sealed class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, response) = exception switch
        {
            BrokenCircuitException => (
                HttpStatusCode.ServiceUnavailable,
                new ErrorResponse(
                    Status: (int)HttpStatusCode.ServiceUnavailable,
                    Title: "Service temporarily unavailable. Circuit breaker is open.")),

            HttpRequestException => (
                HttpStatusCode.BadGateway,
                new ErrorResponse(
                    Status: (int)HttpStatusCode.BadGateway,
                    Title: "Downstream service is unreachable.")),

            TaskCanceledException when exception.InnerException is TimeoutException => (
                HttpStatusCode.GatewayTimeout,
                new ErrorResponse(
                    Status: (int)HttpStatusCode.GatewayTimeout,
                    Title: "Downstream service request timed out.")),

            UnauthorizedAccessException => (
                HttpStatusCode.Unauthorized,
                new ErrorResponse(
                    Status: (int)HttpStatusCode.Unauthorized,
                    Title: "Unauthorized")),

            _ => (
                HttpStatusCode.InternalServerError,
                new ErrorResponse(
                    Status: (int)HttpStatusCode.InternalServerError,
                    Title: "An unexpected error occurred."))
        };

        if (statusCode == HttpStatusCode.InternalServerError)
            logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);
        else
            logger.LogWarning("Handled exception: {Type} - {Message}",
                exception.GetType().Name, exception.Message);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}

/// <summary>
/// Modelo padronizado de resposta de erro.
/// </summary>
public sealed record ErrorResponse(
    int Status,
    string Title,
    List<string>? Errors = null);
