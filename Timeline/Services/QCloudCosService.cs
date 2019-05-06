using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Timeline.Configs;

namespace Timeline.Services
{
    public interface IQCloudCosService
    {
        /// <summary>
        /// Test if an object in the bucket exists.
        /// </summary>
        /// <param name="bucket">The bucket name.</param>
        /// <param name="key">The object key.</param>
        /// <returns>True if exists. False if not.</returns>
        Task<bool> IsObjectExists(string bucket, string key);

        /// <summary>
        /// Upload an object use put method.
        /// </summary>
        /// <param name="bucket">The bucket name.</param>
        /// <param name="key">The object key.</param>
        /// <param name="data">The data to upload.</param>
        Task PutObject(string bucket, string key, byte[] data, string contentType);

        /// <summary>
        /// Generate a presignated url to access the object.
        /// </summary>
        /// <param name="bucket">The bucket name.</param>
        /// <param name="key">The object key.</param>
        /// <returns>The presignated url.</returns>
        string GenerateObjectGetUrl(string bucket, string key);
    }

    public class QCloudCosService : IQCloudCosService
    {
        private readonly IOptionsMonitor<QCloudCosConfig> _config;
        private readonly ILogger<QCloudCosService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public QCloudCosService(IOptionsMonitor<QCloudCosConfig> config, ILogger<QCloudCosService> logger, IHttpClientFactory httpClientFactory)
        {
            _config = config;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        private const string BucketNamePattern = @"^(([a-z0-9][a-z0-9-]*[a-z0-9])|[a-z0-9])$";

        public static bool ValidateBucketName(string bucketName)
        {
            return Regex.IsMatch(bucketName, BucketNamePattern);
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
            public TimeDuration()
            {

            }

            public TimeDuration(DateTimeOffset start, DateTimeOffset end)
            {
                Start = start;
                End = end;
            }

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
            Debug.Assert(signValidTime != null);
            Debug.Assert(signValidTime.Start < signValidTime.End, "Start must be before End in sign valid time.");

            List<(string key, string value)> Transform(IEnumerable<KeyValuePair<string, string>> raw)
            {
                if (raw == null)
                    return new List<(string key, string value)>();

                var sorted = raw.Select(p => (key: p.Key.ToLower(), value: WebUtility.UrlEncode(p.Value))).ToList();
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
                .Append(request.Method.ToLower()).Append('\n')
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

        private QCloudCredentials GetCredentials()
        {
            var config = _config.CurrentValue;
            return new QCloudCredentials
            {
                SecretId = config.SecretId,
                SecretKey = config.SecretKey
            };
        }

        private string GetHost(string bucket)
        {
            var config = _config.CurrentValue;
            return $"{bucket}-{config.AppId}.cos.{config.Region}.myqcloud.com";
        }

        public async Task<bool> IsObjectExists(string bucket, string key)
        {
            if (bucket == null)
                throw new ArgumentNullException(nameof(bucket));
            if (key == null)
                throw new ArgumentNullException(nameof(key));
            if (!ValidateBucketName(bucket))
                throw new ArgumentException($"Bucket name is not valid. Param is {bucket} .", nameof(bucket));

            var client = _httpClientFactory.CreateClient();

            var host = GetHost(bucket);
            var encodedKey = WebUtility.UrlEncode(key);

            var request = new HttpRequestMessage();
            request.Method = HttpMethod.Head;
            request.RequestUri = new Uri($"https://{host}/{encodedKey}");
            request.Headers.Host = host;
            request.Headers.Date = DateTimeOffset.Now;
            request.Headers.TryAddWithoutValidation("Authorization", GenerateSign(GetCredentials(), new RequestInfo
            {
                Method = "head",
                Uri = "/" + encodedKey,
                Headers = new Dictionary<string, string>
                {
                    ["Host"] = host
                }
            }, new TimeDuration(DateTimeOffset.Now, DateTimeOffset.Now.AddMinutes(2))));

            try
            {
                var response = await client.SendAsync(request);

                if (response.IsSuccessStatusCode)
                    return true;
                if (response.StatusCode == HttpStatusCode.NotFound)
                    return false;

                throw new Exception($"Unknown response code. {response.ToString()}");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An error occured when test a cos object existence.");
                throw;
            }
        }

        public async Task PutObject(string bucket, string key, byte[] data, string contentType)
        {
            if (bucket == null)
                throw new ArgumentNullException(nameof(bucket));
            if (key == null)
                throw new ArgumentNullException(nameof(key));
            if (!ValidateBucketName(bucket))
                throw new ArgumentException($"Bucket name is not valid. Param is {bucket} .", nameof(bucket));
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            var host = GetHost(bucket);
            var encodedKey = WebUtility.UrlEncode(key);
            var md5 = Convert.ToBase64String(MD5.Create().ComputeHash(data));

            const string kContentMD5HeaderName = "Content-MD5";
            const string kContentTypeHeaderName = "Content-Type";

            var httpRequest = new HttpRequestMessage()
            {
                Method = HttpMethod.Put,
                RequestUri = new Uri($"https://{host}/{encodedKey}")
            };
            httpRequest.Headers.Host = host;
            httpRequest.Headers.Date = DateTimeOffset.Now;
            var httpContent = new ByteArrayContent(data);
            httpContent.Headers.Add(kContentMD5HeaderName, md5);
            httpRequest.Content = httpContent;

            var signedHeaders = new Dictionary<string, string>
            {
                ["Host"] = host,
                [kContentMD5HeaderName] = md5
            };

            if (contentType != null)
            {
                httpContent.Headers.Add(kContentTypeHeaderName, contentType);
                signedHeaders.Add(kContentTypeHeaderName, contentType);
            }

            httpRequest.Headers.TryAddWithoutValidation("Authorization", GenerateSign(GetCredentials(), new RequestInfo
            {
                Method = "put",
                Uri = "/" + encodedKey,
                Headers = signedHeaders
            }, new TimeDuration(DateTimeOffset.Now, DateTimeOffset.Now.AddMinutes(10))));

            var client = _httpClientFactory.CreateClient();

            try
            {
                var response = await client.SendAsync(httpRequest);
                if (!response.IsSuccessStatusCode)
                    throw new Exception($"Not success status code. {response.ToString()}");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An error occured when test a cos object existence.");
                throw;
            }
        }

        public string GenerateObjectGetUrl(string bucket, string key)
        {
            if (bucket == null)
                throw new ArgumentNullException(nameof(bucket));
            if (key == null)
                throw new ArgumentNullException(nameof(key));
            if (!ValidateBucketName(bucket))
                throw new ArgumentException($"Bucket name is not valid. Param is {bucket} .", nameof(bucket));

            var host = GetHost(bucket);
            var encodedKey = WebUtility.UrlEncode(key);

            var signature = GenerateSign(GetCredentials(), new RequestInfo
            {
                Method = "get",
                Uri = "/" + encodedKey,
                Headers = new Dictionary<string, string>
                {
                    ["Host"] = host
                }
            }, new TimeDuration(DateTimeOffset.Now, DateTimeOffset.Now.AddMinutes(6)));

            return $"https://{host}/{encodedKey}?{signature}";
        }
    }
}
