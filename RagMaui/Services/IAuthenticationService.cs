using System;
using System.Threading.Tasks;

namespace RagMaui.Services
{
    /// <summary>
    /// Interface for authentication service
    /// </summary>
    public interface IAuthenticationService
    {
        /// <summary>
        /// Event that fires when authentication state changes
        /// </summary>
        event Action<bool> AuthenticationStateChanged;

        /// <summary>
        /// Gets the current authentication state
        /// </summary>
        bool IsAuthenticated { get; }

        /// <summary>
        /// Gets the current user's ID
        /// </summary>
        string UserId { get; }

        /// <summary>
        /// Gets the current user's display name
        /// </summary>
        string UserDisplayName { get; }

        /// <summary>
        /// Gets the current access token
        /// </summary>
        string AccessToken { get; }

        /// <summary>
        /// Initializes the authentication service
        /// </summary>
        Task InitializeAsync();

        /// <summary>
        /// Signs in the user with SSO
        /// </summary>
        Task<bool> SignInAsync();

        /// <summary>
        /// Signs out the user
        /// </summary>
        Task SignOutAsync();

        /// <summary>
        /// Gets a new access token (refreshes if needed)
        /// </summary>
        Task<string> GetAccessTokenAsync();
    }
}