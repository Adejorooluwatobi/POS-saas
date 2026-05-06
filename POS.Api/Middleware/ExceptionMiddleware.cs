using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace POS.Api.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
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
        catch (UnauthorizedAccessException ex)
        {
            await HandleExceptionAsync(context, ex, HttpStatusCode.Forbidden, ex.Message);
        }
        catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
        {
            var message = GetUserFriendlyUniqueConstraintMessage(ex);
            await HandleExceptionAsync(context, ex, HttpStatusCode.Conflict, message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred.");
            await HandleExceptionAsync(context, ex, HttpStatusCode.InternalServerError, ex.Message);
        }
    }

    private static bool IsUniqueConstraintViolation(DbUpdateException ex)
    {
        // PostgreSQL error code 23505 = unique_violation
        return ex.InnerException?.Message.Contains("23505") == true ||
               ex.InnerException?.Message.Contains("unique constraint") == true ||
               ex.InnerException?.Message.Contains("duplicate key") == true;
    }

    private static string GetUserFriendlyUniqueConstraintMessage(DbUpdateException ex)
    {
        var inner = ex.InnerException?.Message ?? string.Empty;

        if (inner.Contains("IX_Staff_EmployeeNo") || inner.Contains("EmployeeNo"))
            return "An employee with this Employee Number already exists. Please use a different Employee Number.";

        if (inner.Contains("IX_Staff_Email") || inner.Contains("Email"))
            return "An employee with this email address already exists.";

        if (inner.Contains("IX_Terminals") || inner.Contains("TerminalCode"))
            return "A terminal with this code already exists.";

        return "A record with these details already exists. Please check for duplicates.";
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception, HttpStatusCode statusCode, string message)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var response = new
        {
            status = context.Response.StatusCode,
            message,
            detail = exception.InnerException?.Message
        };

        return context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}
