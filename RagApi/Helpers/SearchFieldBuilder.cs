using Azure.Search.Documents.Indexes.Models;

namespace RagApi.Helpers
{
    public class FieldInfo
    {
        public string Name { get; set; } = "";
        public SearchFieldDataType Type { get; set; }
        public bool IsKey { get; set; }
        public bool IsSearchable { get; set; }
        public bool IsFilterable { get; set; }
        public bool IsSortable { get; set; }
        public bool IsFacetable { get; set; }
        public bool IsHidden { get; set; }
        public bool IsRetrievable { get; set; } = true;
    }
}

