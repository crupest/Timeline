using System;

namespace Timeline.Helpers.Cache
{
    public class CacheableDataDigest : ICacheableDataDigest
    {
        public CacheableDataDigest(string eTag, DateTime lastModified)
        {
            ETag = eTag;
            LastModified = lastModified;
        }

        public string ETag { get; set; }
        public DateTime LastModified { get; set; }
    }
}
