using Azure;
using Azure.AI.FormRecognizer.DocumentAnalysis;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Storage.Blobs;
using Microsoft.ApplicationInsights.DependencyCollector;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RagApi.Api.Auth;
using RagApi.Auth;

namespace RagApi.Extensions;

public static class AzureExtensions
{
    public static IServiceCollection AddAzureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Add Application Insights telemetry
        services.AddApplicationInsightsTelemetry(options => {
            options.ConnectionString = configuration["ApplicationInsights:ConnectionString"];
        });

        // Configure Application Insights
        services.Configure<TelemetryConfiguration>((config) => {
            var dependencyCollector = config.TelemetryProcessors.OfType<DependencyTrackingTelemetryModule>().FirstOrDefault();
            if (dependencyCollector != null)
            {
                // Configure dependency tracking
                dependencyCollector.EnableSqlCommandTextInstrumentation = true;
            }
        });

        // Azure Blob Storage for storing PDF files
        services.AddSingleton(x => new BlobServiceClient(
            configuration.GetConnectionString("AzureBlobStorage") ?? throw new InvalidOperationException("AzureBlobStorage connection string not found")));

        // Azure Document Analysis Client for reading PDF files
        services.AddSingleton(x => new DocumentAnalysisClient(
            new Uri(configuration["Azure:FormRecognizer:Endpoint"] ?? throw new InvalidOperationException("FormRecognizer endpoint not found")),
            new AzureKeyCredential(configuration["Azure:FormRecognizer:Key"] ?? throw new InvalidOperationException("FormRecognizer key not found"))));

        // Azure Cognitive Search as vector database
        services.AddSingleton(x => new SearchClient(
            new Uri(configuration["Azure:Search:Endpoint"] ?? throw new InvalidOperationException("Search endpoint not found")),
            configuration["Azure:Search:IndexName"] ?? throw new InvalidOperationException("Search index name not found"),
            new AzureKeyCredential(configuration["Azure:Search:Key"] ?? throw new InvalidOperationException("Search key not found"))));

        // Add Search Index Client for creating indices
        services.AddSingleton(x => new SearchIndexClient(
            new Uri(configuration["Azure:Search:Endpoint"] ?? throw new InvalidOperationException("Search endpoint not found")),
            new AzureKeyCredential(configuration["Azure:Search:Key"] ?? throw new InvalidOperationException("Search key not found"))));

        // Configure Azure AD options
        services.Configure<AzureAdOptions>(configuration.GetSection("AzureAd"));

        // Add Azure AD authentication
        services.AddAzureAdAuthentication(configuration);

        // Add Entra auth service
        services.AddScoped<EntraAuthService>();

        return services;
    }
}

