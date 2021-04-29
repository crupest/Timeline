using System.Collections.Generic;

namespace Timeline.Services.Api
{
    public class SearchResult<TItem>
    {
#pragma warning disable CA2227 // Collection properties should be read only
        public List<SearchResultItem<TItem>> Items { get; set; } = new();
#pragma warning restore CA2227 // Collection properties should be read only
    }
}
