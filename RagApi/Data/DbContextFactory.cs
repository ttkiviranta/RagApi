using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace RagApi.Data
{
    /// <summary>
    /// Factory for creating DbContext instances for background processing
    /// </summary>
    public class DbContextFactory
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public DbContextFactory(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        /// <summary>
        /// Creates a new DbContext instance within a new scope
        /// </summary>
        public ApplicationDbContext CreateDbContext()
        {
            var scope = _serviceScopeFactory.CreateScope();
            return scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        }

        /// <summary>
        /// Creates a new scope that can be used to resolve services
        /// </summary>
        public IServiceScope CreateScope()
        {
            return _serviceScopeFactory.CreateScope();
        }
    }
}
