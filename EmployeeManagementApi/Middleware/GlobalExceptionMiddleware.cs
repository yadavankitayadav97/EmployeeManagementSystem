using System.Net;
using System.Text.Json;

namespace EmployeeManagementAPI.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;

    public GlobalExceptionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;

        var message = exception.InnerException != null
            ? $"{exception.Message} ({exception.InnerException.Message})"
            : exception.Message;

        var response = new
        {
            statusCode = context.Response.StatusCode,
            message = message
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}