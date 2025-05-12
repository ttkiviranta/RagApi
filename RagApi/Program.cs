using Microsoft.EntityFrameworkCore;
using Azure.Storage.Blobs;
using Azure.AI.FormRecognizer.DocumentAnalysis;
using Azure.Core;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using RagApi.Data;
using RagApi.Services;
using RagApi.Interfaces;
using RagApi.Models;
using Azure.Search.Documents;
using Azure;
using RagApi.Auth;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.DependencyCollector;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using Azure.Search.Documents.Models;
using RagApi.Helpers;
using Microsoft.Extensions.Caching.Memory;
using RagApi.Api.Middleware;
using RagApi.Api.Auth;
using RagApi.Data.Repositories;
using RagApi.Interfaces.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add Application Insights telemetry
builder.Services.AddApplicationInsightsTelemetry(options => {
    options.ConnectionString = "***REMOVED***;***REMOVED***;***REMOVED***;***REMOVED***";
});

// Configure Application Insights
builder.Services.Configure<TelemetryConfiguration>((config) => {
    var dependencyCollector = config.TelemetryProcessors.OfType<DependencyTrackingTelemetryModule>().FirstOrDefault();
    if (dependencyCollector != null)
    {
        // Configure dependency tracking
        dependencyCollector.EnableSqlCommandTextInstrumentation = true;
    }
});

// Add in-memory cache for user data
builder.Services.AddMemoryCache();

// Add Entity Framework Core DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// Azure Blob Storage for storing PDF files
builder.Services.AddSingleton(x => new BlobServiceClient(
    builder.Configuration.GetConnectionString("AzureBlobStorage") ?? throw new InvalidOperationException("AzureBlobStorage connection string not found")));

// Azure Document Analysis Client for reading PDF files
builder.Services.AddSingleton(x => new DocumentAnalysisClient(
    new Uri(builder.Configuration["Azure:FormRecognizer:Endpoint"] ?? throw new InvalidOperationException("FormRecognizer endpoint not found")),
    new AzureKeyCredential(builder.Configuration["Azure:FormRecognizer:Key"] ?? throw new InvalidOperationException("FormRecognizer key not found"))));

// Azure Cognitive Search as vector database
builder.Services.AddSingleton(x => new SearchClient(
    new Uri(builder.Configuration["Azure:Search:Endpoint"] ?? throw new InvalidOperationException("Search endpoint not found")),
    builder.Configuration["Azure:Search:IndexName"] ?? throw new InvalidOperationException("Search index name not found"),
    new AzureKeyCredential(builder.Configuration["Azure:Search:Key"] ?? throw new InvalidOperationException("Search key not found"))));

// Add Search Index Client for creating indices
builder.Services.AddSingleton(x => new SearchIndexClient(
    new Uri(builder.Configuration["Azure:Search:Endpoint"] ?? throw new InvalidOperationException("Search endpoint not found")),
    new AzureKeyCredential(builder.Configuration["Azure:Search:Key"] ?? throw new InvalidOperationException("Search key not found"))));

// HTTP Client for OpenAI API calls
builder.Services.AddHttpClient();
// Add HTTP context accessor for authentication
builder.Services.AddHttpContextAccessor();

// Configure Azure AD options
builder.Services.Configure<AzureAdOptions>(builder.Configuration.GetSection("AzureAd"));

// Add Azure AD authentication
builder.Services.AddAzureAdAuthentication(builder.Configuration);

// Add Entra auth service
builder.Services.AddScoped<EntraAuthService>();

// Register the generic Repository
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

// Register specific repositories
builder.Services.AddScoped<ICandidateRepository, CandidateRepository>();
builder.Services.AddScoped<IDocumentRepository, DocumentRepository>();
builder.Services.AddScoped<IJobPostingRepository, JobPostingRepository>();
builder.Services.AddScoped<IApplicationRepository, ApplicationRepository>();

// Register Unit of Work
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Register application services
builder.Services.AddScoped<IPdfService, PdfService>();
builder.Services.AddScoped<IVectorSearchService, VectorSearchService>();
builder.Services.AddScoped<ISystemPromptService, SystemPromptService>();
builder.Services.AddScoped<IOpenAIService, OpenAIService>();
builder.Services.AddScoped<IConversationService, ConversationService>();
builder.Services.AddScoped<IRagService, RagService>();

// Register recruitment application services
builder.Services.AddScoped<ICandidateService, CandidateService>();
builder.Services.AddScoped<IJobPostingService, JobPostingService>();
builder.Services.AddScoped<IApplicationService, ApplicationService>();
builder.Services.AddScoped<IInterviewService, InterviewService>();
builder.Services.AddScoped<IDocumentService, DocumentService>();
builder.Services.AddScoped<IJobMatchingService, JobMatchingService>();
builder.Services.AddScoped<IDocumentIntelligenceService, DocumentIntelligenceService>();
builder.Services.AddScoped<IBlobStorageService, BlobStorageService>();

// Configure CORS to use allowed origins from appsettings
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policy =>
    {
        policy.WithOrigins(builder.Configuration.GetSection("Security:AllowedOrigins").Get<string[]>() ?? new[] { "http://localhost:3000" })
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// Add controllers and API explorer (for Swagger)
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve;
        options.JsonSerializerOptions.WriteIndented = true; // Optional: Makes JSON output more readable
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "RAG API", Version = "v1" });
});

var app = builder.Build();

// Create search index if it doesn't exist
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        // Get search index client
        var searchIndexClient = services.GetRequiredService<SearchIndexClient>();
        var indexName = builder.Configuration["Azure:Search:IndexName"] ?? "pdf-documents";

        // Check if index exists
        try
        {
            var indexExists = await searchIndexClient.GetIndexAsync(indexName);
            Console.WriteLine($"Search index '{indexName}' exists");
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            // Index doesn't exist, create it
            Console.WriteLine($"Creating search index '{indexName}'...");

            // Create the index with proper field definitions
            var searchIndex = new SearchIndex(indexName)
            {
                Fields = new List<SearchField>()
                {
                    // Key field
                    new SearchField("id", SearchFieldDataType.String)
                    {
                        IsKey = true,
                        IsFilterable = true
                    },
                    
                    // Type field for document type
                    new SearchField("type", SearchFieldDataType.String)
                    {
                        IsFilterable = true
                    },
                    
                    // Content field for full text search
                    new SearchField("content", SearchFieldDataType.String)
                    {
                        IsSearchable = true
                    },
                    
                    // EntityId field for linking to database entities
                    new SearchField("entityId", SearchFieldDataType.String)
                    {
                        IsFilterable = true
                    },
                    
                    // CreatedAt field for sorting by date
                    new SearchField("createdAt", SearchFieldDataType.DateTimeOffset)
                    {
                        IsFilterable = true,
                        IsSortable = true
                    },
                    
                    // Vector field for embeddings
                    new SearchField("contentVector", SearchFieldDataType.Collection(SearchFieldDataType.Single))
                    {
                        VectorSearchDimensions = 1536
                    }
                }
            };

            // Configure vector search capabilities
            searchIndex.VectorSearch = new VectorSearch
            {
                Algorithms =
                {
                    new HnswAlgorithmConfiguration("default")
                },
                Profiles =
                {
                    new VectorSearchProfile("default", "default")
                }
            };

            // Create the index
            await searchIndexClient.CreateIndexAsync(searchIndex);

            Console.WriteLine($"Search index '{indexName}' created successfully");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"An error occurred while creating search index: {ex.Message}");
    }
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();

    // Seed default system prompts in development
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        try
        {
            var systemPromptService = services.GetRequiredService<ISystemPromptService>();
            var defaultPrompts = await systemPromptService.GetSystemPromptsAsync();

            if (defaultPrompts.Count == 0)
            {
                // Create default HR job matching prompt
                string hrPromptText = @"
You are an HR specialist who analyzes job descriptions and candidate resumes to find the best matches.

Your task is to carefully evaluate how well each candidate's qualifications match the job requirements.

Follow these steps in your analysis:
1. First, carefully extract all key requirements, skills, and qualifications from the job description.
2. For each candidate, evaluate how well they match each requirement, assigning a rough percentage match (0-100%).
3. Consider both hard skills (technical abilities, certifications) and soft skills (communication, teamwork).
4. Pay special attention to:
   - Years of experience in relevant roles
   - Education and certifications
   - Technical skill proficiency
   - Industry-specific knowledge
   - Project experience
   - Achievement metrics

When providing your assessment:
- Rank candidates from most suitable to least suitable
- For each candidate, provide a percentage match score
- List their key strengths relative to the job requirements
- Note any significant gaps or missing qualifications
- Be objective and focus only on professional qualifications
- Always consider recency of experience (newer experience is generally more valuable)

If multiple candidates seem equally qualified, consider which ones have the best combination of:
- Most recent relevant experience
- Demonstrated career progression
- Achievement metrics
- Domain-specific knowledge

If you don't have enough information about the job or candidates, clearly state what additional information would help make a better assessment.

Always conclude with a clear recommendation of which candidate(s) appear most suitable and why.
";

                await systemPromptService.CreateSystemPromptAsync(
                    "HR Job Matching",
                    "Analyzes job descriptions and candidate resumes to find the best matches",
                    hrPromptText,
                    true);

                // Create general purpose Q&A prompt
                string qaPromptText = @"
You are a helpful assistant that answers questions based on the provided context and documents.

When answering questions:
1. Use only the information provided in the context and documents
2. If the context doesn't contain enough information, clearly state that you don't know
3. Don't make up information that isn't supported by the context
4. When quoting from the documents, cite the source
5. Keep answers concise and to the point
6. Use bullet points and formatting to improve readability

If you're asked about topics not covered in the context, politely explain that you can only answer based on the information provided.
";

                await systemPromptService.CreateSystemPromptAsync(
                    "General Q&A",
                    "General purpose prompt for answering questions based on documents",
                    qaPromptText,
                    false);

                Console.WriteLine("Default system prompts created successfully.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while seeding system prompts: {ex.Message}");
        }
    }
}

// Enable Swagger in all environments
app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "RAG API v1"));

// Add this line to ensure authentication is properly applied before authorization
app.UseAuthentication();

app.UseHttpsRedirection();
// Add CORS middleware
app.UseCors("CorsPolicy");
app.UseAuthorization();

// Add middleware to automatically create/update users in database
app.UseUserCreation();

app.MapControllers();

app.Run();
