using System;

namespace Timeline.Helpers.Cache
{
    public interface ICacheableDataDigest
    {
        string ETag { get; }
        DateTime LastModified { get; }
    }
}
