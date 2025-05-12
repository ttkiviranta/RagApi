using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace RagApi.Api.Filters
{
    /// <summary>
    /// Standardizes successful responses
    /// </summary>
    public class OkResultHandlingFilter : IResultFilter
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
            if (context.Result is ObjectResult objectResult && objectResult.StatusCode == 200)
            {
                var originalResult = objectResult.Value;

                // Check if it's already wrapped
                if (originalResult != null &&
                    (originalResult.GetType().GetProperty("data") != null ||
                     originalResult.GetType().GetProperty("error") != null))
                {
                    return;
                }

                objectResult.Value = new
                {
                    error = false,
                    data = originalResult
                };
            }
            else if (context.Result is OkResult)
            {
                context.Result = new OkObjectResult(new
                {
                    error = false,
                    data = new { }
                });
            }
        }
    }
}
