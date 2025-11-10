using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RagApi.Api.Middleware;
using RagApi.Extensions;
using RagApi.Services;
using RagApi.Api.Models;
using System.Reflection;
using System.IO;
using System.Linq;
using System;
using Microsoft.AspNetCore.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Add services to container
builder.Services
    .AddDatabase(builder.Configuration)
    .AddAzureServices(builder.Configuration)
    .AddApplicationServices()
    .AddSwaggerServices();

// Add CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policy =>
    {
        policy.WithOrigins(builder.Configuration.GetSection("Security:AllowedOrigins").Get<string[]>() ?? new[] { "http://localhost:3000" , "https://localhost:7296", "http://localhost:5002"
      })
            .AllowAnyHeader()
  .AllowAnyMethod()
  .AllowCredentials();
    });
});

// Add rate limiting
builder.Services.AddRateLimiter(options =>
{
    // Add a fixed window rate limiter for the Insights controller
    options.AddFixedWindowLimiter("fixed", options =>
    {
        options.PermitLimit = 10;
        options.Window = TimeSpan.FromMinutes(1);
        options.QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst;
        options.QueueLimit = 5;
    });
    
    // Configure rate limit exceeded response
    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        await context.HttpContext.Response.WriteAsJsonAsync(new 
        {
            error = true,
            message = "Too many requests. Please try again later.",
            timestamp = DateTime.UtcNow,
            path = context.HttpContext.Request.Path
        }, token);
    };
});

// Add API controllers with filters
builder.Services.AddControllers(options =>
{
    options.AddApiFilters(builder.Services);
})
.AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve;
    options.JsonSerializerOptions.WriteIndented = true;
});

// Add Service Bus background processor
builder.Services.AddHostedService<PdfProcessingService>();


var app = builder.Build();

// Configure logger for initialization tasks
var logger = app.Services.GetRequiredService<ILogger<Program>>();

// Initialize search index
using (var scope = app.Services.CreateScope())
{
    await SearchIndexInitializer.InitializeSearchIndex(scope.ServiceProvider, logger);

    // Seed system prompts in development
    if (app.Environment.IsDevelopment())
    {
        await SystemPromptSeeder.SeedSystemPrompts(scope.ServiceProvider, logger);
    }
}

// Configure HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

// Configure Swagger for all environments
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "RAG API v1");
    c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
    c.DefaultModelsExpandDepth(-1); // Hide schemas section by default
});

// Add version endpoint - minimal API implementation
app.MapGet("/version", (IHostEnvironment hostEnvironment) =>
{
    var assembly = Assembly.GetExecutingAssembly();
    var assemblyVersion = assembly.GetName().Version;
    
    // Get assembly attributes
    var productAttr = assembly.GetCustomAttribute<AssemblyProductAttribute>();
    var companyAttr = assembly.GetCustomAttribute<AssemblyCompanyAttribute>();
    
    // Try to get build date from file metadata
    DateTime? buildDate = null;
    try
    {
        var assemblyLocation = assembly.Location;
        if (!string.IsNullOrEmpty(assemblyLocation))
        {
            buildDate = File.GetLastWriteTime(assemblyLocation);
        }
    }
    catch
    {
        // Ignore any errors when trying to get the build date
    }
    
    return new VersionInfoResponse
    {
        Version = assemblyVersion?.ToString() ?? "1.0.0", // Default to 1.0.0 if not set
        ProductName = productAttr?.Product ?? "RagAPI",
        Company = companyAttr?.Company ?? string.Empty,
        BuildDate = buildDate,
        Environment = hostEnvironment.EnvironmentName
    };
})
.WithName("GetVersion")
.WithDescription("Returns application version information");

// Configure middleware pipeline
app.UseAuthentication();
app.UseHttpsRedirection();
app.UseCors("CorsPolicy");
// Apply rate limiting
app.UseRateLimiter();
app.UseAuthorization();
// Add custom authentication middleware for development bypass
app.UseAuthenticationMiddleware();
app.UseUserCreation();
app.MapControllers();

app.Run();
