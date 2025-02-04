using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net;

public class CustomExceptionMiddleware : IMiddleware
{
    private readonly ILogger<CustomExceptionMiddleware> _logger;

    public CustomExceptionMiddleware(ILogger<CustomExceptionMiddleware> logger)
    {
        _logger = logger;
    }


    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            string traceId = context.TraceIdentifier;
            _logger.LogError(ex, "Unhandled exception occurred. TraceId: {TraceId}", traceId);

            await HandleExceptionAsync(context, ex, traceId);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception, string traceId)
    {
        HttpStatusCode statusCode = HttpStatusCode.InternalServerError;
        string message = "An unexpected error occurred";

        switch (exception)
        {
            case UnauthorizedAccessException:
                statusCode = HttpStatusCode.Unauthorized;
                message = "Unauthorized access";
                break;

            case ArgumentException:
                statusCode = HttpStatusCode.BadRequest;
                message = exception.Message;
                break;

            case KeyNotFoundException:
                statusCode = HttpStatusCode.NotFound;
                message = "Resource not found";
                break;

            default:
                break;
        }

        context.Response.StatusCode = (int)statusCode;

        ProblemDetails problem = new ()
        {
            Status = (int)statusCode,
            Title = message,
            Detail = exception.StackTrace,
            Instance = context.Request.Path,
            Extensions = { { "traceId", traceId } }
        };

        var jsonResponse = JsonConvert.SerializeObject(problem);

        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync(jsonResponse);
    }
}
