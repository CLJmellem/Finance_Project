using System.Net;
using System.Text.Json;
using CardsService.Application.Exceptions;
using FluentValidation;

namespace CardsService.API.Middleware;

/// <summary>
/// Global Middleware to handle exceptions and return standardized error responses.
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
                Errors: validationEx.Errors
                    .Select(e => e.ErrorMessage)
                    .ToList())),

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

            // Any other unhandled exception is treated as a 500 Internal Server Error
            _ => (
                HttpStatusCode.InternalServerError,
                new ErrorResponse(
                    Status: (int)HttpStatusCode.InternalServerError,
                    Title: "An unexpected error occurred."))
        };

        // log unexpected exceptions as errors, handled ones as warnings
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
/// Padronized error response model for API exceptions.
/// </summary>
/// <seealso cref="System.IEquatable&lt;CardsService.API.Middleware.ErrorResponse&gt;" />
public sealed record ErrorResponse(
    int Status,
    string Title,
    List<string>? Errors = null);
