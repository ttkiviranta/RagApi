using System;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace RagApi.Api.Filters
{
    /// <summary>
    /// Global exception handler filter
    /// </summary>
    public class ErrorHandlingFilter : IExceptionFilter
    {
        private readonly ILogger<ErrorHandlingFilter> _logger;

        /// <summary>
        /// Initializes a new instance of the ErrorHandlingFilter class
        /// </summary>
        public ErrorHandlingFilter(ILogger<ErrorHandlingFilter> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Handles exceptions that occur during action execution
        /// </summary>
        public void OnException(ExceptionContext context)
        {
            HttpStatusCode statusCode = HttpStatusCode.InternalServerError;
            string message = "An unexpected error occurred";

            var exception = context.Exception;

            // Log the exception with details
            _logger.LogError(exception,
                "Unhandled exception in {ControllerName}.{ActionName}: {Message}",
                context.RouteData.Values["controller"],
                context.RouteData.Values["action"],
                exception.Message);

            // Customize response based on exception type
            if (exception is ArgumentException)
            {
                statusCode = HttpStatusCode.BadRequest;
                message = exception.Message;
            }
            else if (exception is UnauthorizedAccessException)
            {
                statusCode = HttpStatusCode.Unauthorized;
                message = "You are not authorized to perform this action";
            }
            else if (exception is InvalidOperationException)
            {
                statusCode = HttpStatusCode.BadRequest;
                message = exception.Message;
            }
            else if (exception is NotImplementedException)
            {
                statusCode = HttpStatusCode.NotImplemented;
                message = "This feature is not yet implemented";
            }

            // Create the response
            var result = new ObjectResult(new
            {
                error = true,
                message = message,
                timestamp = DateTime.UtcNow,
                path = context.HttpContext.Request.Path
            })
            {
                StatusCode = (int)statusCode
            };

            // Set the result
            context.Result = result;
            context.ExceptionHandled = true;
        }
    }
}
