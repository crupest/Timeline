using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using System;
using System.Linq;
using System.Threading.Tasks;
using Timeline.Models;
using Timeline.Models.Http;

namespace Timeline.Helpers.Cache
{
    public static class DataCacheHelper
    {
        public static async Task<ActionResult> GenerateActionResult(Controller controller, ICacheableDataProvider provider, TimeSpan? maxAge = null)
        {
            const string CacheControlHeaderKey = "Cache-Control";
            const string IfNonMatchHeaderKey = "If-None-Match";
            const string IfModifiedSinceHeaderKey = "If-Modified-Since";
            const string ETagHeaderKey = "ETag";
            const string LastModifiedHeaderKey = "Last-Modified";

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

            var digest = await provider.GetDigest();
            var eTagValue = $"\"{digest.ETag}\"";
            var eTag = new EntityTagHeaderValue(eTagValue);

            ActionResult Generate304Result()
            {
                controller.Response.Headers.Add(ETagHeaderKey, eTagValue);
                controller.Response.Headers.Add(LastModifiedHeaderKey, digest.LastModified.ToString("R"));
                controller.Response.Headers.Add(CacheControlHeaderKey, GenerateCacheControlHeaderValue());
                return controller.StatusCode(StatusCodes.Status304NotModified, null);
            }

            if (controller.Request.Headers.TryGetValue(IfNonMatchHeaderKey, out var ifNonMatchHeaderValue))
            {
                if (!EntityTagHeaderValue.TryParseList(ifNonMatchHeaderValue, out var eTagList))
                {
                    return controller.BadRequest(new CommonResponse(ErrorCodes.Common.Header.IfNonMatch_BadFormat, "Header If-None-Match is of bad format."));
                }

                if (eTagList.FirstOrDefault(e => e.Equals(eTag)) != null)
                {
                    return Generate304Result();
                }
            }
            else if (controller.Request.Headers.TryGetValue(IfModifiedSinceHeaderKey, out var ifModifiedSinceHeaderValue))
            {
                if (!DateTime.TryParse(ifModifiedSinceHeaderValue, out var headerValue))
                {
                    return controller.BadRequest(new CommonResponse(ErrorCodes.Common.Header.IfModifiedSince_BadFormat, "Header If-Modified-Since is of bad format."));
                }

                if (headerValue > digest.LastModified)
                {
                    return Generate304Result();
                }
            }

            var data = await provider.GetData();
            controller.Response.Headers.Add(CacheControlHeaderKey, GenerateCacheControlHeaderValue());
            return controller.File(data.Data, data.ContentType, digest.LastModified, eTag);
        }

        public static Task<ActionResult> GenerateActionResult(Controller controller, Func<Task<ICacheableDataDigest>> getDigestDelegate, Func<Task<ByteData>> getDataDelegate, TimeSpan? maxAge = null)
        {
            return GenerateActionResult(controller, new DelegateCacheableDataProvider(getDigestDelegate, getDataDelegate), maxAge);
        }
    }
}
