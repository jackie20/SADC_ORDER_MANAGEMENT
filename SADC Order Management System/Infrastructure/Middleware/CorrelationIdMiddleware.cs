using SADC_Order_Management_System.Helpers;

namespace SADC_Order_Management_System.Infrastructure.Middleware
{
    public class CorrelationIdMiddleware
    {
        private readonly RequestDelegate _next;

        public CorrelationIdMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            var correlationId = context.Request.Headers[CorrelationHelper.HeaderName].FirstOrDefault();
            if (string.IsNullOrWhiteSpace(correlationId))
            {
                correlationId = Guid.NewGuid().ToString("N");
            }

            context.Items[CorrelationHelper.HeaderName] = correlationId;
            context.Response.Headers[CorrelationHelper.HeaderName] = correlationId;

            await _next(context);
        }
    }
}