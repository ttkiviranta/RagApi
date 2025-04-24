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

// Add Entity Framework Core DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// Azure Blob Storage for storing PDF files
builder.Services.AddSingleton(x => new BlobServiceClient(
    builder.Configuration.GetConnectionString("AzureBlobStorage")));

// Azure Document Analysis Client for reading PDF files
builder.Services.AddSingleton(x => new DocumentAnalysisClient(
    new Uri(builder.Configuration["Azure:FormRecognizer:Endpoint"]),
    new AzureKeyCredential(builder.Configuration["Azure:FormRecognizer:Key"])));

// Azure Cognitive Search as vector database
builder.Services.AddSingleton(x => new SearchClient(
    new Uri(builder.Configuration["Azure:Search:Endpoint"]),
    builder.Configuration["Azure:Search:IndexName"],
    new AzureKeyCredential(builder.Configuration["Azure:Search:Key"])));

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
// Register application services
builder.Services.AddScoped<IPdfService, PdfService>();
builder.Services.AddScoped<IVectorSearchService, VectorSearchService>();
builder.Services.AddScoped<ISystemPromptService, SystemPromptService>();
builder.Services.AddScoped<IOpenAIService, OpenAIService>();
builder.Services.AddScoped<IConversationService, ConversationService>();
builder.Services.AddScoped<IRagService, RagService>();

// Configure CORS to use allowed origins from appsettings
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policy =>
    {
        policy.WithOrigins(builder.Configuration.GetSection("Security:AllowedOrigins").Get<string[]>())
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
app.MapControllers();

app.Run();