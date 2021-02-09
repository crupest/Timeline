using System;
using System.Threading.Tasks;
using Timeline.Models;

namespace Timeline.Helpers.Cache
{
    public class DelegateCacheableDataProvider : ICacheableDataProvider
    {
        private readonly Func<Task<ICacheableDataDigest>> _getDigestDelegate;
        private readonly Func<Task<ByteData>> _getDataDelegate;

        public DelegateCacheableDataProvider(Func<Task<ICacheableDataDigest>> getDigestDelegate, Func<Task<ByteData>> getDataDelegate)
        {
            _getDigestDelegate = getDigestDelegate;
            _getDataDelegate = getDataDelegate;
        }

        public Task<ICacheableDataDigest> GetDigest()
        {
            return _getDigestDelegate();
        }

        public Task<ByteData> GetData()
        {
            return _getDataDelegate();
        }
    }
}
