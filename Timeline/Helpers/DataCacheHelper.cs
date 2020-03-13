using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using System;
using System.Linq;
using System.Threading.Tasks;
using Timeline.Models.Http;
using static Timeline.Resources.Helper.DataCacheHelper;

namespace Timeline.Helpers
{
    public interface ICacheableData
    {
        string Type { get; }
#pragma warning disable CA1819 // Properties should not return arrays
        byte[] Data { get; }
#pragma warning restore CA1819 // Properties should not return arrays
        DateTime? LastModified { get; }
    }

    public class CacheableData : ICacheableData
    {
        public CacheableData(string type, byte[] data, DateTime? lastModified)
        {
            Type = type;
            Data = data;
            LastModified = lastModified;
        }

        public string Type { get; set; }
#pragma warning disable CA1819 // Properties should not return arrays
        public byte[] Data { get; set; }
#pragma warning restore CA1819 // Properties should not return arrays
        public DateTime? LastModified { get; set; }
    }

    public interface ICacheableDataProvider
    {
        Task<string> GetDataETag();
        Task<ICacheableData> GetData();
    }

    public class DelegateCacheableDataProvider : ICacheableDataProvider
    {
        private readonly Func<Task<string>> _getDataETagDelegate;
        private readonly Func<Task<ICacheableData>> _getDataDelegate;

        public DelegateCacheableDataProvider(Func<Task<string>> getDataETagDelegate, Func<Task<ICacheableData>> getDataDelegate)
        {
            _getDataETagDelegate = getDataETagDelegate;
            _getDataDelegate = getDataDelegate;
        }

        public Task<ICacheableData> GetData()
        {
            return _getDataDelegate();
        }

        public Task<string> GetDataETag()
        {
            return _getDataETagDelegate();
        }
    }

    public static class DataCacheHelper
    {
        public static async Task<ActionResult> GenerateActionResult(Controller controller, ICacheableDataProvider provider, TimeSpan? maxAge = null)
        {
            const string CacheControlHeaderKey = "Cache-Control";
            const string IfNonMatchHeaderKey = "If-None-Match";
            const string ETagHeaderKey = "ETag";

            string GenerateCacheControlHeaderValue()
            {
                var cacheControlHeader = new CacheControlHeaderValue()
                {
                    NoCache = true,
                    NoStore = false,
                    MaxAge = maxAge ?? TimeSpan.FromDays(14),
                    Private = true,
                    MustRevalidate = true
                };
                return cacheControlHeader.ToString();
            }

            var loggerFactory = controller.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger(typeof(DataCacheHelper));

            var eTagValue = await provider.GetDataETag();
            eTagValue = '"' + eTagValue + '"';
            var eTag = new EntityTagHeaderValue(eTagValue);


            if (controller.Request.Headers.TryGetValue(IfNonMatchHeaderKey, out var value))
            {
                if (!EntityTagHeaderValue.TryParseStrictList(value, out var eTagList))
                {
                    logger.LogInformation(Log.Format(LogBadIfNoneMatch, ("Header Value", value)));
                    return controller.BadRequest(ErrorResponse.Common.Header.IfNonMatch_BadFormat());
                }

                if (eTagList.FirstOrDefault(e => e.Equals(eTag)) != null)
                {
                    logger.LogInformation(LogResultNotModified);
                    controller.Response.Headers.Add(ETagHeaderKey, eTagValue);
                    controller.Response.Headers.Add(CacheControlHeaderKey, GenerateCacheControlHeaderValue());

                    return controller.StatusCode(StatusCodes.Status304NotModified);
                }
            }

            var data = await provider.GetData();
            logger.LogInformation(LogResultData);
            controller.Response.Headers.Add(CacheControlHeaderKey, GenerateCacheControlHeaderValue());
            return controller.File(data.Data, data.Type, data.LastModified, eTag);
        }

        public static Task<ActionResult> GenerateActionResult(Controller controller, Func<Task<string>> getDataETagDelegate, Func<Task<ICacheableData>> getDataDelegate, TimeSpan? maxAge = null)
        {
            return GenerateActionResult(controller, new DelegateCacheableDataProvider(getDataETagDelegate, getDataDelegate), maxAge);
        }
    }
}
