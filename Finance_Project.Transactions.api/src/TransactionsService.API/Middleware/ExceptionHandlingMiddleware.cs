using System.Net;
using System.Text.Json;
using FluentValidation;
using TransactionsService.Application.Exceptions;

namespace TransactionsService.API.Middleware;

/// <summary>
/// Global middleware that catches all unhandled exceptions and returns standardized JSON error responses.
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
            ValidationException validationEx => (
                HttpStatusCode.BadRequest,
                new ErrorResponse(
                    Status: (int)HttpStatusCode.BadRequest,
                    Title: "Validation Error",
                    Errors: validationEx.Errors.Select(e => e.ErrorMessage).ToList())),

            NotFoundException notFoundEx => (
                HttpStatusCode.NotFound,
                new ErrorResponse(
                    Status: (int)HttpStatusCode.NotFound,
                    Title: notFoundEx.Message)),

            DomainException domainEx => (
                HttpStatusCode.UnprocessableEntity,
                new ErrorResponse(
                    Status: (int)HttpStatusCode.UnprocessableEntity,
                    Title: domainEx.Message)),

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

public sealed record ErrorResponse(int Status, string Title, List<string>? Errors = null);
