using RagMaui.Services;
using System;
using System.Threading.Tasks;

namespace RagMaui
{
    /// <summary>
    /// Main application class
    /// </summary>
    public partial class App : Application
    {
        private readonly IAuthenticationService _authService;

        /// <summary>
        /// Initializes a new instance of the App
        /// </summary>
        public App(IAuthenticationService authService)
        {
            InitializeComponent();
            _authService = authService;

            MainPage = new AppShell();
            
            // Initialize authentication on app startup
            InitializeAuthentication();
        }

        private async void InitializeAuthentication()
        {
            await _authService.InitializeAsync();
            
            // If user is not authenticated, show login page
            if (!_authService.IsAuthenticated)
            {
                await ShowLoginPage();
            }
        }

        private async Task ShowLoginPage()
        {
            bool result = await _authService.SignInAsync();
            if (!result)
            {
                // Show error message to user
                await Current.MainPage.DisplayAlert(
                    "Login Error",
                    "Authentication failed. Please try again.",
                    "OK");
            }
        }

        /// <summary>
        /// Override to handle window creation for SSO redirects
        /// </summary>
        protected override Window CreateWindow(IActivationState activationState)
        {
            var window = base.CreateWindow(activationState);

            // Handle returning to the app after SSO authentication in browser
            window.Created += (s, e) => 
            {
                // Window created
                Console.WriteLine("Window created");
            };

            window.Activated += (s, e) => 
            {
                // Window activated (also on re-activation)
                Console.WriteLine("Window activated");
            };

            return window;
        }
    }
}
