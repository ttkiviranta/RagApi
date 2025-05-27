using Microsoft.Extensions.DependencyInjection;
using RagApi.Data.Repositories;
using RagApi.Interfaces.Repositories;
using RagApi.Interfaces;
using RagApi.Services;
using RagApi.Api.Auth;
using AutoMapper;
using RagApi.Data;

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

        // Add AutoMapper                                   
        services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

        // HTTP Client for OpenAI API calls
        services.AddHttpClient();

        // Add HTTP context accessor for authentication
        services.AddHttpContextAccessor();

        // Add in-memory cache for user data
        services.AddMemoryCache();

        return services;
    }
}

