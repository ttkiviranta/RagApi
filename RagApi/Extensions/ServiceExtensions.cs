using Microsoft.Extensions.DependencyInjection;
using RagApi.Data.Repositories;
using RagApi.Interfaces.Repositories;
using RagApi.Interfaces;
using RagApi.Services;
using RagApi.Api.Auth;
using RagApi.Data;
using RagApi.Mapping;
using RagApi.Interfaces.Agents;
using RagApi.Services.Agents;

namespace RagApi.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Register RequestContext                          
        services.AddScoped<IRequestContext, RequestContext>();

        // Register the generic Repository
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

        // Register specific repositories
        services.AddScoped<ICandidateRepository, CandidateRepository>();
        services.AddScoped<IDocumentRepository, DocumentRepository>();
        services.AddScoped<IJobPostingRepository, JobPostingRepository>();
        services.AddScoped<IApplicationRepository, ApplicationRepository>();
        services.AddScoped<IUserRepository, UserRepository>();

        // Register Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Register application services
        services.AddScoped<IPdfService, PdfService>();
        services.AddScoped<IVectorSearchService, VectorSearchService>();
        services.AddScoped<ISystemPromptService, SystemPromptService>();
        services.AddScoped<IOpenAIService, OpenAIService>();
        services.AddScoped<IConversationService, ConversationService>();
        services.AddScoped<IRagService, RagService>();

        // Register Service Bus -service
        services.AddScoped<IMessageBusService, MessageBusService>();

        // Register recruitment application services
        services.AddScoped<ICandidateService, CandidateService>();
        services.AddScoped<IJobPostingService, JobPostingService>();
        services.AddScoped<IApplicationService, ApplicationService>();
        services.AddScoped<IInterviewService, InterviewService>();
        services.AddScoped<IDocumentService, DocumentService>();
        services.AddScoped<IJobMatchingService, JobMatchingService>();
        services.AddScoped<IDocumentIntelligenceService, DocumentIntelligenceService>();
        services.AddScoped<IBlobStorageService, BlobStorageService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IApplicationInsightsService, ApplicationInsightsService>();

        // Register Agent services
        services.AddAgentServices();

        // Register Mapperly mappers
        services.AddSingleton<ApplicationMapper>();
        services.AddSingleton<CandidateMapper>();
        services.AddSingleton<InterviewMapper>();
        services.AddSingleton<JobPostingMapper>();
        services.AddSingleton<SystemPromptMapper>();
        services.AddSingleton<UserMapper>();

        // HTTP Client for OpenAI API calls
        services.AddHttpClient();

        // HTTP Client for Application Insights API
        services.AddHttpClient("ApplicationInsights");

        // Add HTTP context accessor for authentication
        services.AddHttpContextAccessor();

        // Add in-memory cache for user data
        services.AddMemoryCache();

        return services;
    }

    /// <summary>
    /// Register AI Agent services and orchestrator
    /// </summary>
    public static IServiceCollection AddAgentServices(this IServiceCollection services)
    {
        // Register the main orchestrator
        services.AddSingleton<IAgentOrchestrator, AgentOrchestrator>();

        // Register individual agents
        services.AddScoped<DocumentAnalysisAgent>();
        services.AddScoped<CandidateCreationAgent>();
        services.AddScoped<JobMatchingAgent>();

        // Register agents with the orchestrator using a hosted service
        services.AddHostedService<AgentRegistrationService>();

        return services;
    }
}

