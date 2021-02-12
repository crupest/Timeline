using System.Threading.Tasks;
using Timeline.Models;

namespace Timeline.Helpers.Cache
{
    public interface ICacheableDataProvider
    {
        Task<ICacheableDataDigest> GetDigest();
        Task<ByteData> GetData();
    }
}
