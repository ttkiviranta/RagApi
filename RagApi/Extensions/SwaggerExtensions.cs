using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace RagApi.Extensions;

public static class SwaggerExtensions
{
    public static IServiceCollection AddSwaggerServices(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();

        // Configure Swagger with JWT Bearer authentication
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "RAG API",
                Version = "v1",
                Description = "Retrieval Augmented Generation API with token-based authentication"
            });

            // Add JWT Bearer authentication support to Swagger UI
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "JWT Authorization header using the Bearer scheme.\r\n\r\n" +
                             "Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\n" +
                             "Example: \"Bearer eyJhbGciOiJIUzI1NiIsI...\"",
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });

        return services;
    }
}
