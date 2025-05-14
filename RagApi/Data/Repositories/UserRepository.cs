// RagApi/Data/Repositories/UserRepository.cs
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RagApi.Interfaces.Repositories;
using RagApi.Models;

namespace RagApi.Data.Repositories
{
    public class UserRepository : Repository<User>, IUserRepository
    {
        public UserRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<User> GetByEmailAsync(string email)
        {
            return await _dbSet.FirstOrDefaultAsync(u => u.Email == email);
        }

        public bool IsUserInGroup(string userId, string groupId)
        {
            // This is a placeholder - in a real implementation, this would 
            // check group membership in a user_groups table or similar
            // For this example, we'll return false as this is handled at service level
            return false;
        }

        public async Task UpdateLastLoginAsync(string userId)
        {
            var user = await GetByIdAsync(userId);
            if (user != null)
            {
                user.LastLogin = DateTime.UtcNow;
                await SaveChangesAsync();
            }
        }

        public async Task UpdateAdminStatusAsync(string userId, bool isAdmin)
        {
            var user = await GetByIdAsync(userId);
            if (user != null && user.IsAdmin != isAdmin)
            {
                user.IsAdmin = isAdmin;
                user.LastRoleSync = DateTime.UtcNow;
                await SaveChangesAsync();
            }
        }
    }
}

