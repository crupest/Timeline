using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Timeline.Configs;

namespace Timeline.Services
{
    public interface IQCloudCosService
    {
        Task<bool> ObjectExists(string bucket, string key);
        string GetObjectUrl(string bucket, string key);
    }

    public class QCloudCosService : IQCloudCosService
    {
        private readonly QCloudCosConfig _config;
        private readonly ILogger<QCloudCosService> _logger;

        public QCloudCosService(IOptions<QCloudCosConfig> config, ILogger<QCloudCosService> logger)
        {
            _config = config.Value;
            _logger = logger;
        }

        public class QCloudCredentials
        {
            public string SecretId { get; set; }
            public string SecretKey { get; set; }
        }

        public class RequestInfo
        {
            public string Method { get; set; }
            public string Uri { get; set; }
            public IEnumerable<KeyValuePair<string, string>> Parameters { get; set; }
            public IEnumerable<KeyValuePair<string, string>> Headers { get; set; }
        }

        public class TimeDuration
        {
            public DateTimeOffset Start { get; set; }
            public DateTimeOffset End { get; set; }
        }

        public static string GenerateSign(QCloudCredentials credentials, RequestInfo request, TimeDuration signValidTime)
        {
            Debug.Assert(credentials != null);
            Debug.Assert(credentials.SecretId != null);
            Debug.Assert(credentials.SecretKey != null);
            Debug.Assert(request != null);
            Debug.Assert(request.Method != null);
            Debug.Assert(request.Uri != null);
            Debug.Assert(request.Parameters != null);
            Debug.Assert(request.Headers != null);
            Debug.Assert(signValidTime != null);
            Debug.Assert(signValidTime.Start < signValidTime.End, "Start must be before End in sign valid time.");

            List<(string key, string value)> Transform(IEnumerable<KeyValuePair<string, string>> raw)
            {
                var sorted= raw.Select(p => (key: p.Key.ToLower(), value: WebUtility.UrlEncode(p.Value))).ToList();
                sorted.Sort((left, right) => string.CompareOrdinal(left.key, right.key));
                return sorted;
            }

            var transformedParameters = Transform(request.Parameters);
            var transformedHeaders = Transform(request.Headers);

            List<(string, string)> result = new List<(string, string)>();

            const string signAlgorithm = "sha1";
            result.Add(("q-sign-algorithm", signAlgorithm));

            result.Add(("q-ak", credentials.SecretId));

            var signTime = $"{signValidTime.Start.ToUnixTimeSeconds().ToString()};{signValidTime.End.ToUnixTimeSeconds().ToString()}";
            var keyTime = signTime;
            result.Add(("q-sign-time", signTime));
            result.Add(("q-key-time", keyTime));

            result.Add(("q-header-list", string.Join(';', transformedHeaders.Select(h => h.key))));
            result.Add(("q-url-param-list", string.Join(';', transformedParameters.Select(p => p.key))));

            HMACSHA1 hmac = new HMACSHA1();

            string ByteArrayToString(byte[] bytes)
            {
                return BitConverter.ToString(bytes).Replace("-", "").ToLower();
            }

            hmac.Key = Encoding.UTF8.GetBytes(credentials.SecretKey);
            var signKey = ByteArrayToString(hmac.ComputeHash(Encoding.UTF8.GetBytes(keyTime)));

            string Join(IEnumerable<(string key, string value)> raw)
            {
                return string.Join('&', raw.Select(p => string.Concat(p.key, "=", p.value)));
            }

            var httpString = new StringBuilder()
                .Append(request.Method).Append('\n')
                .Append(request.Uri).Append('\n')
                .Append(Join(transformedParameters)).Append('\n')
                .Append(Join(transformedHeaders)).Append('\n')
                .ToString();

            string Sha1(string data)
            {
                var sha1 = SHA1.Create().ComputeHash(Encoding.UTF8.GetBytes(data));
                return ByteArrayToString(sha1);
            }

            var stringToSign = new StringBuilder()
                .Append(signAlgorithm).Append('\n')
                .Append(signTime).Append('\n')
                .Append(Sha1(httpString)).Append('\n')
                .ToString();

            hmac.Key = Encoding.UTF8.GetBytes(signKey);
            var signature = ByteArrayToString(hmac.ComputeHash(
                Encoding.UTF8.GetBytes(stringToSign)));

            result.Add(("q-signature", signature));

            return Join(result);
        }

        public Task<bool> ObjectExists(string bucket, string key)
        {
            throw new NotImplementedException();
        }

        public string GetObjectUrl(string bucket, string key)
        {
            throw new NotImplementedException();
        }
    }
}
