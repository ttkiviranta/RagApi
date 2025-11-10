// RagApi/Interfaces/IRepository.cs
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace RagApi.Interfaces
{
    /// <summary>
    /// Generic repository interface for database operations
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    public interface IRepository<T> where T : class
    {
        /// <summary>
        /// Gets all entities
        /// </summary>
        Task<IEnumerable<T>> GetAllAsync();

        /// <summary>
        /// Gets entities by condition
        /// </summary>
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Gets entity by ID
        /// </summary>
        Task<T> GetByIdAsync(string id);

        /// <summary>
        /// Adds a new entity
        /// </summary>
        Task AddAsync(T entity);

        /// <summary>
        /// Updates an entity
        /// </summary>
        Task UpdateAsync(T entity);

        /// <summary>
        /// Removes an entity
        /// </summary>
        Task DeleteAsync(T entity);

        /// <summary>
        /// Saves changes to database
        /// </summary>
        Task SaveChangesAsync();
    }
}
