using Azure.Search.Documents.Indexes.Models;
using System.Collections.Generic;
using System.Linq;

namespace RagApi.Helpers
{
    /// <summary>
    /// Represents configuration information for a field in Azure Cognitive Search index
    /// </summary>
    public class FieldInfo
    {
        /// <summary>
        /// Name of the field in the search index
        /// </summary>
        public string Name { get; set; } = "";

        /// <summary>
        /// Data type of the field
        /// </summary>
        public SearchFieldDataType Type { get; set; }

        /// <summary>
        /// Whether this field is the primary key
        /// </summary>
        public bool IsKey { get; set; }

        /// <summary>
        /// Whether this field can be used in full-text searches
        /// </summary>
        public bool IsSearchable { get; set; }

        /// <summary>
        /// Whether this field can be used for filtering
        /// </summary>
        public bool IsFilterable { get; set; }

        /// <summary>
        /// Whether this field can be used for sorting
        /// </summary>
        public bool IsSortable { get; set; }

        /// <summary>
        /// Whether this field can be used for facets
        /// </summary>
        public bool IsFacetable { get; set; }

        /// <summary>
        /// Whether this field should be hidden in search results
        /// </summary>
        public bool IsHidden { get; set; }

        /// <summary>
        /// Whether this field should be included in search results
        /// Default is true
        /// </summary>
        public bool IsRetrievable { get; set; } = true;
    }

    /// <summary>
    /// Helper class for building Azure Cognitive Search index fields
    /// </summary>
    public static class SearchFieldBuilder
    {
        /// <summary>
        /// Builds a SearchField from a FieldInfo configuration
        /// </summary>
        /// <param name="fieldInfo">The field configuration</param>
        /// <returns>A configured SearchField</returns>
        public static SearchField Build(FieldInfo fieldInfo)
        {
            // Create the field with its name and type
            var field = new SearchField(fieldInfo.Name, fieldInfo.Type)
            {
                IsKey = fieldInfo.IsKey,
                IsSearchable = fieldInfo.IsSearchable,
                IsFilterable = fieldInfo.IsFilterable,
                IsSortable = fieldInfo.IsSortable,
                IsFacetable = fieldInfo.IsFacetable,
                IsHidden = fieldInfo.IsHidden
                // IsRetrievable property is not available in the current Azure SDK version
                // IsRetrievable = fieldInfo.IsRetrievable
            };

            // Configure vector search dimensions if this is a vector field
            if (fieldInfo.Name == "contentVector" && fieldInfo.Type == SearchFieldDataType.Collection(SearchFieldDataType.Single))
            {
                field.VectorSearchDimensions = 1536;
            }

            return field;
        }

        /// <summary>
        /// Builds a list of SearchFields from a collection of FieldInfo configurations
        /// </summary>
        /// <param name="fieldInfos">Collection of field configurations</param>
        /// <returns>A list of configured SearchFields</returns>
        public static List<SearchField> BuildFields(IEnumerable<FieldInfo> fieldInfos)
        {
            return fieldInfos.Select(Build).ToList();
        }

        /// <summary>
        /// Creates a standard set of fields for document search index
        /// </summary>
        /// <returns>A collection of field configurations</returns>
        public static IEnumerable<FieldInfo> GetStandardDocumentFields()
        {
            return new List<FieldInfo>
            {
                // Key field
                new FieldInfo
                {
                    Name = "id",
                    Type = SearchFieldDataType.String,
                    IsKey = true,
                    IsFilterable = true
                },
                
                // Type field for document type
                new FieldInfo
                {
                    Name = "type",
                    Type = SearchFieldDataType.String,
                    IsFilterable = true
                },
                
                // Content field for full text search
                new FieldInfo
                {
                    Name = "content",
                    Type = SearchFieldDataType.String,
                    IsSearchable = true
                },
                
                // EntityId field for linking to database entities
                new FieldInfo
                {
                    Name = "entityId",
                    Type = SearchFieldDataType.String,
                    IsFilterable = true
                },
                
                // CreatedAt field for sorting by date
                new FieldInfo
                {
                    Name = "createdAt",
                    Type = SearchFieldDataType.DateTimeOffset,
                    IsFilterable = true,
                    IsSortable = true
                },
                
                // Vector field for embeddings
                new FieldInfo
                {
                    Name = "contentVector",
                    Type = SearchFieldDataType.Collection(SearchFieldDataType.Single)
                    // VectorSearchDimensions will be set in Build method
                }
            };
        }
    }
}


