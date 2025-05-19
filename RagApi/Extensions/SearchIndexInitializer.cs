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

namespace RagApi.Extensions;

public static class SearchIndexInitializer
{
    public static async Task InitializeSearchIndex(IServiceProvider serviceProvider, ILogger logger)
    {
        try
        {
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();
            var searchIndexClient = serviceProvider.GetRequiredService<SearchIndexClient>();
            var indexName = configuration["Azure:Search:IndexName"] ?? "pdf-documents";

            // Check if index exists
            try
            {
                var indexExists = await searchIndexClient.GetIndexAsync(indexName);
                logger.LogInformation($"Search index '{indexName}' exists");
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                // Index doesn't exist, create it
                logger.LogInformation($"Creating search index '{indexName}'...");

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

                logger.LogInformation($"Search index '{indexName}' created successfully");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while creating search index");
        }
    }
}
