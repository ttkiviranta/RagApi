// IUserService.cs - Interface defining user management operations
using RagApi.Api.Models;
using RagApi.Models;

public interface IUserService
{
    /// <summary>
    /// Gets the current logged-in user based on token, creates if not exists
    /// </summary>
    /// <param name="adminGroupId">The ID of the admin group to check membership against</param>
    /// <returns>The user object for the current authenticated user</returns>
    Task<User> GetOrCreateCurrentUserAsync(string adminGroupId);

    /// <summary>
    /// Gets a user by ID, or creates a new user if not found and claims contain necessary information
    /// </summary>
    /// <param name="userId">The ID of the user to find or create</param>
    /// <param name="adminGroupId">The ID of the admin group to check membership against</param>
    /// <returns>The found or newly created user object</returns>
    Task<User> GetOrCreateUserByIdAsync(string userId, string adminGroupId);

    /// <summary>
    /// Gets all users in the system
    /// </summary>
    /// <returns>Collection of all users</returns>
    Task<IEnumerable<User>> GetAllUsersAsync();

    /// <summary>
    /// Gets a specific user by ID
    /// </summary>
    /// <param name="id">The ID of the user to retrieve</param>
    /// <returns>The user if found, null otherwise</returns>
    Task<User?> GetUserByIdAsync(string id);

    /// <summary>
    /// Creates a new user with the provided information
    /// </summary>
    /// <param name="request">User creation request containing user details</param>
    /// <returns>The newly created user</returns>
    Task<User> CreateUserAsync(CreateUserRequest request);

    /// <summary>
    /// Updates an existing user with new information
    /// </summary>
    /// <param name="id">The ID of the user to update</param>
    /// <param name="request">User update request containing new user details</param>
    /// <returns>The updated user if found, null otherwise</returns>
    Task<User?> UpdateUserAsync(string id, CreateUserRequest request);

    /// <summary>
    /// Deletes a user from the system
    /// </summary>
    /// <param name="id">The ID of the user to delete</param>
    /// <returns>Task representing the asynchronous operation</returns>
    Task DeleteUserAsync(string id);
}
