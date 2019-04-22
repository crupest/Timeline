using COSXML;
using COSXML.Auth;
using COSXML.CosException;
using COSXML.Model;
using COSXML.Model.Object;
using COSXML.Model.Tag;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;
using Timeline.Configs;

namespace Timeline.Services
{
    public interface ITencentCloudCosService
    {
        Task<bool> Exists(string bucket, string key);
        string GetObjectUrl(string bucket, string key);
    }

    public class TencentCloudCosService : ITencentCloudCosService
    {
        private readonly TencentCosConfig _config;
        private readonly CosXmlServer _server;
        private readonly ILogger<TencentCloudCosService> _logger;

        public TencentCloudCosService(IOptions<TencentCosConfig> config, ILogger<TencentCloudCosService> logger)
        {
            _config = config.Value;
            _logger = logger;

            var cosConfig = new CosXmlConfig.Builder()
                .IsHttps(true)
                .SetAppid(config.Value.AppId)
                .SetRegion(config.Value.Region)
                .SetDebugLog(true)
                .Build();

            var credentialProvider = new DefaultQCloudCredentialProvider(config.Value.SecretId, config.Value.SecretKey, 3600);

            _server = new CosXmlServer(cosConfig, credentialProvider);
        }

        public Task<bool> Exists(string bucket, string key)
        {
            bucket = bucket + "-" + _config.AppId;

            var request = new HeadObjectRequest(bucket, key);

            var t = new TaskCompletionSource<bool>();

            _server.HeadObject(request, delegate (CosResult result)
            {
                if (result.httpCode >= 200 && result.httpCode < 300)
                    t.SetResult(true);
                else
                    t.SetResult(false);
            },
            delegate (CosClientException clientException, CosServerException serverException)
            {
                if (clientException != null)
                {
                    _logger.LogError(clientException, "An client error occured when test cos object existence. Bucket : {} . Key : {} .", bucket, key);
                    t.SetException(clientException);
                    return;
                }
                if (serverException != null)
                {
                    _logger.LogError(serverException, "An server error occured when test cos object existence. Bucket : {} . Key : {} .", bucket, key);
                    t.SetException(serverException);
                    return;
                }
                _logger.LogError("An unknown error occured when test cos object existence. Bucket : {} . Key : {} .", bucket, key);
                t.SetException(new Exception("Unknown exception when test cos object existence."));
            });

            return t.Task;
        }

        public string GetObjectUrl(string bucket, string key)
        {
            return _server.GenerateSignURL(new PreSignatureStruct()
            {
                appid = _config.AppId,
                region = _config.Region,
                bucket = bucket + "-" + _config.AppId,
                key = key,
                httpMethod = "GET",
                isHttps = true,
                signDurationSecond = 300
            });
        }
    }
}
