using System;
using System.Collections.Generic;

namespace Timeline.Models
{
    public class Page<T>
    {
        public Page()
        {
        }

        public Page(long pageNumber, long pageSize, long totalCount, List<T> items)
        {
            PageNumber = pageNumber;
            PageSize = pageSize;
            TotalPageCount = totalCount / PageSize + (totalCount % PageSize != 0 ? 1 : 0);
            TotalCount = totalCount;
            Items = items;
        }

        public long PageNumber { get; set; }
        public long PageSize { get; set; }
        public long TotalPageCount { get; set; }
        public long TotalCount { get; set; }
        public List<T> Items { get; set; } = new List<T>();

        public Page<U> WithItems<U>(List<U> items)
        {
            return new Page<U>(PageNumber, PageSize, TotalCount, items);
        }
    }
}

