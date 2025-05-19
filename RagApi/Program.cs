using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RagApi.Api.Middleware;
using RagApi.Extensions;

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
        policy.WithOrigins(builder.Configuration.GetSection("Security:AllowedOrigins").Get<string[]>() ?? new[] { "http://localhost:3000" })
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// Add API controllers
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve;
        options.JsonSerializerOptions.WriteIndented = true;
    });

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

// Configure middleware pipeline
app.UseAuthentication();
app.UseHttpsRedirection();
app.UseCors("CorsPolicy");
app.UseAuthorization();
app.UseUserCreation();
app.MapControllers();

app.Run();
