using Microsoft.Identity.Client;
using RagMaui.Config;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;

namespace RagMaui.Services
{
    /// <summary>
    /// Implementation of the authentication service using MSAL with development bypass
    /// </summary>
    public class AuthenticationService : IAuthenticationService
 {
        private readonly IPublicClientApplication? _pca;
        private IAccount? _userAccount;
   private AuthenticationResult? _authResult;
        private bool _isDevelopmentBypass;

        // Use configuration values from AuthConfig
        private readonly string[] _scopes = new[] { "User.Read", AuthConfig.ApiScope };

        /// <summary>
        /// Event that fires when authentication state changes
  /// </summary>
        public event Action<bool>? AuthenticationStateChanged;

        /// <summary>
        /// Gets the current authentication state
        /// </summary>
        public bool IsAuthenticated => _isDevelopmentBypass || _authResult != null;

        /// <summary>
        /// Gets the current user's ID
    /// </summary>
        public string UserId => _isDevelopmentBypass ? AuthConfig.DevelopmentUserId : (_authResult?.UniqueId ?? string.Empty);

        /// <summary>
    /// Gets the current user's display name
  /// </summary>
        public string UserDisplayName => _isDevelopmentBypass ? AuthConfig.DevelopmentUserName : (_authResult?.Account?.Username ?? string.Empty);

        /// <summary>
        /// Gets the current access token
 /// </summary>
   public string AccessToken => _isDevelopmentBypass ? AuthConfig.DevelopmentAccessToken : (_authResult?.AccessToken ?? string.Empty);

        /// <summary>
        /// Initializes a new instance of the AuthenticationService
        /// </summary>
        public AuthenticationService()
        {
     if (AuthConfig.UseDevelopmentBypass)
    {
        _isDevelopmentBypass = true;
        Debug.WriteLine("MSAL: Using development bypass mode - no Azure AD authentication required");
  return;
         }

            try
    {
            // Initialize the MSAL client for production use
      var builder = PublicClientApplicationBuilder
  .Create(AuthConfig.ClientId)
          .WithAuthority(AuthConfig.Authority);

#if WINDOWS
     // For Windows desktop - use embedded web view to avoid SPA redirect issues
   builder = builder
                  .WithRedirectUri("https://login.microsoftonline.com/common/oauth2/nativeclient")
          .WithDefaultRedirectUri();
#elif IOS
       // For iOS, use the custom redirect URI with keychain security
  builder = builder
 .WithIosKeychainSecurityGroup("com.microsoft.adalcache")
        .WithRedirectUri("msalc4dae6fe://auth");
#elif ANDROID
       // For Android, use standard redirect
     builder = builder.WithRedirectUri("msalc4dae6fe://auth");
#else
// Default fallback for desktop
         builder = builder.WithDefaultRedirectUri();
#endif

    _pca = builder.Build();
      Debug.WriteLine("MSAL client initialized successfully for production use");
            }
    catch (Exception ex)
   {
        Debug.WriteLine($"Error initializing MSAL: {ex.GetType().Name} - {ex.Message}");
   // In case of error, fall back to development mode for local testing
      _isDevelopmentBypass = true;
         Debug.WriteLine("Falling back to development bypass mode due to MSAL initialization error");
       }
        }

  /// <summary>
     /// Initializes the authentication service
    /// </summary>
        public async Task InitializeAsync()
        {
  if (_isDevelopmentBypass)
            {
      AuthenticationStateChanged?.Invoke(true);
         Debug.WriteLine("Development bypass: User automatically authenticated");
         return;
            }

  if (_pca == null) return;

try
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
         catch (Exception ex)
      {
         Debug.WriteLine($"Error in InitializeAsync: {ex.Message}");
       // Don't throw - just log and continue without authentication
            }
 }

        /// <summary>
   /// Signs in the user with SSO
   /// </summary>
        public async Task<bool> SignInAsync()
    {
            if (_isDevelopmentBypass)
   {
     AuthenticationStateChanged?.Invoke(true);
       Debug.WriteLine("Development bypass: Sign-in successful (simulated)");
       return true;
       }

      if (_pca == null) return false;

   try
            {
   // Start interactive authentication with embedded web view for desktop
     Debug.WriteLine("Starting interactive authentication for desktop");
         var builder = _pca.AcquireTokenInteractive(_scopes)
      .WithPrompt(Prompt.SelectAccount);

     if (_userAccount != null)
       builder = builder.WithAccount(_userAccount);

#if WINDOWS
 // Use embedded web view to avoid SPA redirect issues
  builder = builder.WithUseEmbeddedWebView(true);
#endif

        _authResult = await builder.ExecuteAsync();

    _userAccount = _authResult.Account;
             Debug.WriteLine($"Authentication successful for user: {_userAccount?.Username}");
 AuthenticationStateChanged?.Invoke(true);
       return true;
   }
      catch (MsalClientException ex)
       {
     // Handle client-side errors
   Debug.WriteLine($"MSAL Client Exception: {ex.ErrorCode} - {ex.Message}");
        return false;
    }
     catch (MsalServiceException ex)
        {
       // Handle service-side errors
           Debug.WriteLine($"MSAL Service Exception: {ex.ErrorCode} - {ex.Message}");
  return false;
         }
         catch (Exception ex)
  {
         // Handle other exceptions
       Debug.WriteLine($"Authentication failed: {ex.GetType().Name} - {ex.Message}");
    return false;
      }
    }

   /// <summary>
  /// Signs out the user
   /// </summary>
      public async Task SignOutAsync()
        {
 if (_isDevelopmentBypass)
     {
       AuthenticationStateChanged?.Invoke(false);
          Debug.WriteLine("Development bypass: Sign-out successful (simulated)");
       return;
            }

            if (_pca != null && _userAccount != null)
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
   if (_isDevelopmentBypass)
  {
         return AuthConfig.DevelopmentAccessToken;
   }

    if (_pca == null) return string.Empty;

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
      if (_isDevelopmentBypass || _pca == null) return;

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
    Debug.WriteLine($"Silent authentication failed: {ex.Message}");
         _authResult = null;
         AuthenticationStateChanged?.Invoke(false);
     }
        }
    }
}