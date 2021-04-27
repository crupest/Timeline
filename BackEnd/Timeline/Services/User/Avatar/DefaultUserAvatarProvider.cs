using Microsoft.AspNetCore.Hosting;
using SixLabors.ImageSharp;
using System.IO;
using System.Threading.Tasks;
using Timeline.Helpers.Cache;
using Timeline.Models;
using Timeline.Services.Data;

namespace Timeline.Services.User.Avatar
{
    // TODO! : Make this configurable.
    public class DefaultUserAvatarProvider : IDefaultUserAvatarProvider
    {
        private readonly IETagGenerator _eTagGenerator;

        private readonly string _avatarPath;

        private CacheableDataDigest? _cacheDigest;
        private ByteData? _cacheData;

        public DefaultUserAvatarProvider(IWebHostEnvironment environment, IETagGenerator eTagGenerator)
        {
            _avatarPath = Path.Combine(environment.ContentRootPath, "default-avatar.png");
            _eTagGenerator = eTagGenerator;
        }

        private async Task CheckAndInit()
        {
            var path = _avatarPath;
            if (_cacheData == null || File.GetLastWriteTime(path) > _cacheDigest!.LastModified)
            {
                var data = await File.ReadAllBytesAsync(path);
                _cacheDigest = new CacheableDataDigest(await _eTagGenerator.GenerateETagAsync(data), File.GetLastWriteTime(path));
                Image.Identify(data, out var format);
                _cacheData = new ByteData(data, format.DefaultMimeType);
            }
        }

        public async Task<ICacheableDataDigest> GetDefaultAvatarDigest()
        {
            await CheckAndInit();
            return _cacheDigest!;
        }

        public async Task<ByteData> GetDefaultAvatar()
        {
            await CheckAndInit();
            return _cacheData!;
        }
    }
}
