using Microsoft.Extensions.Logging;
using RagMaui.Services;
using RagMaui.ViewModels;
using RagMaui.Views;
using Microsoft.Maui.Hosting;

namespace RagMaui
{
    /// <summary>
    /// Main entry point for the MAUI application
    /// </summary>
    public static class MauiProgram
    {
        /// <summary>
        /// Creates and configures the MAUI application
        /// </summary>
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // Register authentication service
            builder.Services.AddSingleton<IAuthenticationService, AuthenticationService>();
            
            // Register services
            builder.Services.AddSingleton<IRagApiService, RagApiService>();

            // Register view models
            builder.Services.AddTransient<ChatViewModel>();
            builder.Services.AddTransient<ConversationListViewModel>();
            builder.Services.AddTransient<DocumentUploadViewModel>();

            // Register pages
            builder.Services.AddTransient<ChatPage>();
            builder.Services.AddTransient<ConversationListPage>();
            builder.Services.AddTransient<DocumentUploadPage>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}