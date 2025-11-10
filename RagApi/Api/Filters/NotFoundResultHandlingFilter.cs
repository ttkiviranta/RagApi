using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace RagApi.Api.Filters
{
    /// <summary>
    /// Standardizes NotFound responses
    /// </summary>
    public class NotFoundResultHandlingFilter : IResultFilter
    {
        /// <summary>
        /// Executes after the action result is executed
        /// </summary>
        public void OnResultExecuted(ResultExecutedContext context)
        {
            // Do nothing after execution
        }

        /// <summary>
        /// Executes before the action result is executed
        /// </summary>
        public void OnResultExecuting(ResultExecutingContext context)
        {
            if (context.Result is NotFoundObjectResult notFoundObjectResult)
            {
                var originalResult = notFoundObjectResult.Value;

                // Check if it's already wrapped
                if (originalResult != null &&
                    (originalResult.GetType().GetProperty("error") != null))
                {
                    return;
                }

                notFoundObjectResult.Value = new
                {
                    error = true,
                    message = originalResult?.ToString() ?? "Resource not found"
                };
            }
            else if (context.Result is NotFoundResult)
            {
                context.Result = new NotFoundObjectResult(new
                {
                    error = true,
                    message = "Resource not found"
                });
            }
        }
    }
}