using Microsoft.Identity.Client;
using RagMaui.Config;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace RagMaui.Services
{
    /// <summary>
    /// Implementation of the authentication service using MSAL
    /// </summary>
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IPublicClientApplication _pca;
        private IAccount _userAccount;
        private AuthenticationResult _authResult;

        // Use configuration values from AuthConfig
        private readonly string[] _scopes = new[] { "User.Read", AuthConfig.ApiScope };
        
        /// <summary>
        /// Event that fires when authentication state changes
        /// </summary>
        public event Action<bool> AuthenticationStateChanged;

        /// <summary>
        /// Gets the current authentication state
        /// </summary>
        public bool IsAuthenticated => _authResult != null;

        /// <summary>
        /// Gets the current user's ID
        /// </summary>
        public string UserId => _authResult?.UniqueId ?? string.Empty;

        /// <summary>
        /// Gets the current user's display name
        /// </summary>
        public string UserDisplayName => _authResult?.Account?.Username ?? string.Empty;

        /// <summary>
        /// Gets the current access token
        /// </summary>
        public string AccessToken => _authResult?.AccessToken ?? string.Empty;

        /// <summary>
        /// Initializes a new instance of the AuthenticationService
        /// </summary>
        public AuthenticationService()
        {
            // Initialize the MSAL client using AuthConfig values
            _pca = PublicClientApplicationBuilder
                .Create(AuthConfig.ClientId)
                .WithAuthority(AuthConfig.Authority)
                .WithRedirectUri($"msal{AuthConfig.ClientId}://auth")
                .WithIosKeychainSecurityGroup("com.microsoft.adalcache")
                .Build();
        }

        /// <summary>
        /// Initializes the authentication service
        /// </summary>
        public async Task InitializeAsync()
        {
            // Try to get an existing account
            var accounts = await _pca.GetAccountsAsync();
            _userAccount = accounts.FirstOrDefault();

            if (_userAccount != null)
            {
                // Try silent authentication if we have an account
                await SilentAuthenticateAsync();
            }
        }

        /// <summary>
        /// Signs in the user with SSO
        /// </summary>
        public async Task<bool> SignInAsync()
        {
            try
            {
                // Start interactive authentication
                _authResult = await _pca.AcquireTokenInteractive(_scopes)
                    .WithAccount(_userAccount)
                    .WithPrompt(Prompt.SelectAccount)
                    .ExecuteAsync();

                _userAccount = _authResult.Account;
                AuthenticationStateChanged?.Invoke(true);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Authentication failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Signs out the user
        /// </summary>
        public async Task SignOutAsync()
        {
            if (_userAccount != null)
            {
                await _pca.RemoveAsync(_userAccount);
                _userAccount = null;
                _authResult = null;
                AuthenticationStateChanged?.Invoke(false);
            }
        }

        /// <summary>
        /// Gets a new access token (refreshes if needed)
        /// </summary>
        public async Task<string> GetAccessTokenAsync()
        {
            // Try silent authentication if token is expired
            if (_authResult == null || _authResult.ExpiresOn <= DateTimeOffset.UtcNow.AddMinutes(5))
            {
                await SilentAuthenticateAsync();
            }

            return _authResult?.AccessToken ?? string.Empty;
        }

        /// <summary>
        /// Attempts to authenticate silently
        /// </summary>
        private async Task SilentAuthenticateAsync()
        {
            try
            {
                if (_userAccount != null)
                {
                    _authResult = await _pca.AcquireTokenSilent(_scopes, _userAccount).ExecuteAsync();
                    AuthenticationStateChanged?.Invoke(true);
                }
            }
            catch (MsalUiRequiredException)
            {
                // Silent authentication failed - user needs to sign in interactively
                _authResult = null;
                AuthenticationStateChanged?.Invoke(false);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Silent authentication failed: {ex.Message}");
                _authResult = null;
                AuthenticationStateChanged?.Invoke(false);
            }
        }
    }
}