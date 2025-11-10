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

            // Register existing view models
            builder.Services.AddTransient<ChatViewModel>();
            builder.Services.AddTransient<ConversationListViewModel>();
            builder.Services.AddTransient<DocumentUploadViewModel>();

            // Register new view models
            builder.Services.AddTransient<JobPostingsViewModel>();
            builder.Services.AddTransient<CandidatesViewModel>();
            builder.Services.AddTransient<SystemPromptsViewModel>();
            builder.Services.AddTransient<ApplicationsViewModel>();
            builder.Services.AddTransient<InterviewsViewModel>();

            // Register existing pages
            builder.Services.AddTransient<ChatPage>();
            builder.Services.AddTransient<ConversationListPage>();
            builder.Services.AddTransient<DocumentUploadPage>();

            // Register new pages
            builder.Services.AddTransient<JobPostingsPage>();
            builder.Services.AddTransient<CandidatesPage>();
            builder.Services.AddTransient<SystemPromptsPage>();
            builder.Services.AddTransient<ApplicationsPage>();
            builder.Services.AddTransient<InterviewsPage>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}