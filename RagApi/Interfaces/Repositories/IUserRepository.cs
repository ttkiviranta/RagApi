// RagApi/Interfaces/Repositories/IUserRepository.cs
using System.Collections.Generic;
using System.Threading.Tasks;
using RagApi.Models;

namespace RagApi.Interfaces.Repositories
{
    public interface IUserRepository : IRepository<User>
    {
        /// <summary>
        /// Gets a user by email
        /// </summary>
        Task<User> GetByEmailAsync(string email);

        /// <summary>
        /// Checks if user is in a specific group
        /// </summary>
        bool IsUserInGroup(string userId, string groupId);

        /// <summary>
        /// Updates user's last login time
        /// </summary>
        Task UpdateLastLoginAsync(string userId);

        /// <summary>
        /// Updates user's admin status
        /// </summary>
        Task UpdateAdminStatusAsync(string userId, bool isAdmin);
    }
}
