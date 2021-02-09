using System;

namespace Timeline.Models.Http
{
    public class HttpTimelinePostDataDigest
    {
        public HttpTimelinePostDataDigest()
        {

        }

        public HttpTimelinePostDataDigest(string kind, string eTag, DateTime lastUpdated)
        {
            Kind = kind;
            ETag = eTag;
            LastUpdated = lastUpdated;
        }

        public string Kind { get; set; } = default!;
        public string ETag { get; set; } = default!;
        public DateTime LastUpdated { get; set; }
    }
}
