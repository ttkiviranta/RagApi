using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace RagApi.Api.Filters
{
    /// <summary>
    /// Standardizes BadRequest responses
    /// </summary>
    public class BadRequestResultHandlingFilter : IResultFilter
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
            if (context.Result is BadRequestObjectResult badRequestResult)
            {
                var originalResult = badRequestResult.Value;

                // Check if it's already wrapped
                if (originalResult != null &&
                    (originalResult.GetType().GetProperty("error") != null))
                {
                    return;
                }

                // If it's a validation error, let ValidateModelAttribute handle it
                if (context.ModelState.ErrorCount > 0 && !context.ModelState.IsValid)
                {
                    return;
                }

                badRequestResult.Value = new
                {
                    error = true,
                    message = originalResult?.ToString() ?? "Bad request"
                };
            }
            else if (context.Result is BadRequestResult)
            {
                context.Result = new BadRequestObjectResult(new
                {
                    error = true,
                    message = "Bad request"
                });
            }
        }
    }
}
