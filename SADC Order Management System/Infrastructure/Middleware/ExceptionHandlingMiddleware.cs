using Microsoft.EntityFrameworkCore;
using SADC_Order_Management_System.Helpers;
using System.Text.Json;

namespace SADC_Order_Management_System.Infrastructure.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Resource not found.");
                await WriteProblemAsync(context, 404, "Not Found", ex.Message);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogWarning(ex, "Concurrency conflict.");
                await WriteProblemAsync(context, 409, "Concurrency Conflict", "The resource was updated by another process.");
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Business rule violation.");
                await WriteProblemAsync(context, 400, "Bad Request", ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception.");
                await WriteProblemAsync(context, 500, "Internal Server Error", "An unexpected error occurred.");
            }
        }

        private static async Task WriteProblemAsync(HttpContext context, int statusCode, string title, string detail)
        {
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/problem+json";
            var problem = ProblemDetailsHelper.Create(statusCode, title, detail);
            await context.Response.WriteAsync(JsonSerializer.Serialize(problem));
        }
    }
}