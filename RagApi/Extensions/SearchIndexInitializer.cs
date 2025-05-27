using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Azure;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using Azure.Search.Documents.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RagApi.Helpers;

namespace RagApi.Extensions;

/// <summary>
/// Static class responsible for creating and initializing the Azure Cognitive Search index
/// </summary>
public static class SearchIndexInitializer
{
    /// <summary>
    /// Initializes the search index if it doesn't already exist
    /// </summary>
    /// <param name="serviceProvider">The service provider for dependency resolution</param>
    /// <param name="logger">The logger instance</param>
    /// <returns>A task representing the asynchronous operation</returns>
    public static async Task InitializeSearchIndex(IServiceProvider serviceProvider, ILogger logger)
    {
        try
        {
            // Retrieve required services from DI container
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();
            var searchIndexClient = serviceProvider.GetRequiredService<SearchIndexClient>();
            var indexName = configuration["Azure:Search:IndexName"] ?? "pdf-documents";

            // Check if the index already exists
            try
            {
                var indexExists = await searchIndexClient.GetIndexAsync(indexName);
                logger.LogInformation($"Search index '{indexName}' exists");
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                // Index doesn't exist, create it
                logger.LogInformation($"Creating search index '{indexName}'...");

                // Get standard field definitions
                var fieldInfos = SearchFieldBuilder.GetStandardDocumentFields();

                // Create the index with generated field definitions
                var searchIndex = new SearchIndex(indexName)
                {
                    Fields = SearchFieldBuilder.BuildFields(fieldInfos)
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

                // Create the index in Azure Cognitive Search
                await searchIndexClient.CreateIndexAsync(searchIndex);

                logger.LogInformation($"Search index '{indexName}' created successfully");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while creating search index");
        }
    }
}
