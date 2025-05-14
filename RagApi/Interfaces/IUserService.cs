// IUserService.cs
using RagApi.Api.Models;
using RagApi.Models;

public interface IUserService
{
    /// <summary>
    /// Get the current logged-in user based on token, create if not exists
    /// </summary>
    Task<User> GetOrCreateCurrentUserAsync(string adminGroupId);

    // Muut metodit pysyvät samoina
    Task<IEnumerable<User>> GetAllUsersAsync();
    Task<User?> GetUserByIdAsync(string id);
    Task<User> CreateUserAsync(CreateUserRequest request);
    Task<User?> UpdateUserAsync(string id, CreateUserRequest request);
    Task DeleteUserAsync(string id);
}
