using OrderService.Domain.Exceptions;
using ValidationException = Shared.Core.Exceptions.ValidationException;

namespace OrderService.Api.Middleware
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

        private Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            context.Response.ContentType = "application/json";

            object responseBody;
            int statusCode;

            switch (ex)
            {
                case ValidationException validationEx:
                    statusCode = StatusCodes.Status400BadRequest;
                    responseBody = new
                    {
                        success = false,
                        message = validationEx.Message,
                        errors = validationEx.Errors
                    };
                    break;

                case OrderNotFoundException:
                    statusCode = StatusCodes.Status404NotFound;
                    responseBody = new { success = false, message = ex.Message };
                    break;

                case OrderDomainException:
                    statusCode = StatusCodes.Status400BadRequest;
                    responseBody = new { success = false, message = ex.Message };
                    break;

                default:
                    statusCode = StatusCodes.Status500InternalServerError;
                    responseBody = new { success = false, message = "An internal server error occurred." };
                    break;
            }

            _logger.LogError(ex, "Exception occurred: {Message}", ex.Message);

            context.Response.StatusCode = statusCode;
            return context.Response.WriteAsJsonAsync(responseBody);
        }
    }
}
