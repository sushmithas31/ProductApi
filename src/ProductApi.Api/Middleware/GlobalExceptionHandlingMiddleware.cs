using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text.Json;

namespace ProductApi.Api.Middleware;

public class GlobalExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;

    public GlobalExceptionHandlingMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlingMiddleware> logger)
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
            _logger.LogError(ex, "An unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var problemDetails = CreateProblemDetails(context, exception);
        
        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = problemDetails.Status ?? (int)HttpStatusCode.InternalServerError;

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        var json = JsonSerializer.Serialize(problemDetails, jsonOptions);
        await context.Response.WriteAsync(json);
    }

    private static ProblemDetails CreateProblemDetails(HttpContext context, Exception exception)
    {
        var statusCode = GetStatusCode(exception);
        var problemDetails = new ProblemDetails
        {
            Type = GetProblemType(exception),
            Title = GetTitle(exception),
            Status = statusCode,
            Detail = GetDetail(exception),
            Instance = context.Request.Path
        };

        // Add trace ID for debugging
        problemDetails.Extensions.Add("traceId", context.TraceIdentifier);

        // Add timestamp
        problemDetails.Extensions.Add("timestamp", DateTime.UtcNow);

        return problemDetails;
    }

    private static int GetStatusCode(Exception exception) =>
        exception switch
        {
            ArgumentException or ArgumentNullException or InvalidOperationException => (int)HttpStatusCode.BadRequest,
            UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized,
            NotImplementedException => (int)HttpStatusCode.NotImplemented,
            TimeoutException => (int)HttpStatusCode.RequestTimeout,
            _ => (int)HttpStatusCode.InternalServerError
        };

    private static string GetTitle(Exception exception) =>
        exception switch
        {
            ArgumentException or ArgumentNullException or InvalidOperationException => "Bad Request",
            UnauthorizedAccessException => "Unauthorized",
            NotImplementedException => "Not Implemented",
            TimeoutException => "Request Timeout",
            _ => "Internal Server Error"
        };

    private static string GetDetail(Exception exception) =>
        exception switch
        {
            ArgumentException or ArgumentNullException or InvalidOperationException => exception.Message,
            UnauthorizedAccessException => "Access to the requested resource is unauthorized",
            NotImplementedException => "The requested functionality is not implemented",
            TimeoutException => "The request timed out",
            _ => "An error occurred while processing your request"
        };

    private static string GetProblemType(Exception exception) =>
        exception switch
        {
            ArgumentException or ArgumentNullException or InvalidOperationException => "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            UnauthorizedAccessException => "https://tools.ietf.org/html/rfc7235#section-3.1",
            NotImplementedException => "https://tools.ietf.org/html/rfc7231#section-6.6.2",
            TimeoutException => "https://tools.ietf.org/html/rfc7231#section-6.5.7",
            _ => "https://tools.ietf.org/html/rfc7231#section-6.6.1"
        };
}

