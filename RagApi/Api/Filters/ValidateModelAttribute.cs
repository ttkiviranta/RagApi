
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace RagApi.Api.Filters
{
    /// <summary>
    /// Validates model state automatically and returns standardized error responses
    /// </summary>
    public class ValidateModelAttribute : ActionFilterAttribute
    {
        private readonly ILogger<ValidateModelAttribute> _logger;

        /// <summary>
        /// Initializes a new instance of the ValidateModelAttribute class
        /// </summary>
        public ValidateModelAttribute(ILogger<ValidateModelAttribute> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Executes before the action method is invoked
        /// </summary>
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                var errors = context.ModelState
                    .Where(e => e.Value.Errors.Count > 0)
                    .Select(e => new
                    {
                        Field = e.Key,
                        Errors = e.Value.Errors.Select(er => er.ErrorMessage).ToArray()
                    })
                    .ToArray();

                _logger.LogWarning("Model validation failed: {Errors}",
                    string.Join(", ", errors.SelectMany(e => e.Errors)));

                context.Result = new BadRequestObjectResult(new
                {
                    error = true,
                    message = "Validation failed",
                    validationErrors = errors
                });
            }
        }
    }
}
