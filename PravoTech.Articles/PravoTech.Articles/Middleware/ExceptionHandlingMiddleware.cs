using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json;

namespace PravoTech.Articles.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate? _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate? next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                if (_next != null)
                {
                    await _next(context);
                }
                else
                {
                    _logger.LogWarning("ExceptionHandlingMiddleware is the last middleware in the pipeline");
                    context.Response.StatusCode = StatusCodes.Status404NotFound;
                    await context.Response.WriteAsJsonAsync(new { error = "Endpoint not found" });
                }
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var response = context.Response;
            response.ContentType = "application/json";

            var (statusCode, message) = exception switch
            {
                ValidationException => (HttpStatusCode.BadRequest, exception.Message),
                KeyNotFoundException => (HttpStatusCode.NotFound, "Resource not found"),
                UnauthorizedAccessException => (HttpStatusCode.Unauthorized, "Unauthorized access"),
                _ => (HttpStatusCode.InternalServerError, "An unexpected error occurred")
            };

            _logger.LogError(exception, "An error occurred: {Message}", exception.Message);

            response.StatusCode = (int)statusCode;
            var result = JsonSerializer.Serialize(new { error = message });
            await response.WriteAsync(result);
        }
    }
} 