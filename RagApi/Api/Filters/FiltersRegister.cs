using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RagApi.Api.Filters;

namespace RagApi.Extensions
{
    /// <summary>
    /// Register API filters
    /// </summary>
    public static class FiltersRegister
    {
        /// <summary>
        /// Add API filters to MVC options
        /// </summary>
        public static void AddApiFilters(this MvcOptions options, IServiceCollection services)
        {
            var provider = services.BuildServiceProvider();

            // Add error handling filter
            options.Filters.Add(new ErrorHandlingFilter(
                provider.GetService<ILogger<ErrorHandlingFilter>>()));

            // Add model validation filter
            options.Filters.Add(new ValidateModelAttribute(
                provider.GetService<ILogger<ValidateModelAttribute>>()));

            // Add result handling filters
            options.Filters.Add(new OkResultHandlingFilter());
            options.Filters.Add(new BadRequestResultHandlingFilter());
            options.Filters.Add(new NotFoundResultHandlingFilter());
        }
    }
}
