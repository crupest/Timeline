using System;
using System.Collections.Generic;
using System.Text;
using Timeline.Models;
using Timeline.Models.Http;
using Xunit.Abstractions;

namespace Timeline.Tests.IntegratedTests
{
    public class TimelinePostTest : BaseTimelineTest
    {
        public static HttpTimelinePostCreateRequest CreateTextPostRequest(string text, DateTime? time = null, string? color = null)
        {
            return new HttpTimelinePostCreateRequest()
            {
                Time = time,
                Color = color,
                DataList = new List<HttpTimelinePostCreateRequestData>()
                {
                    new HttpTimelinePostCreateRequestData()
                    {
                        ContentType = MimeTypes.TextPlain,
                        Data = Convert.ToBase64String(Encoding.UTF8.GetBytes(text))
                    }
                }
            };
        }

        public static HttpTimelinePostCreateRequest CreateMarkdownPostRequest(string text, DateTime? time = null, string? color = null)
        {
            return new HttpTimelinePostCreateRequest()
            {
                Time = time,
                Color = color,
                DataList = new List<HttpTimelinePostCreateRequestData>()
                {
                    new HttpTimelinePostCreateRequestData()
                    {
                        ContentType = MimeTypes.TextMarkdown,
                        Data = Convert.ToBase64String(Encoding.UTF8.GetBytes(text))
                    }
                }
            };
        }

        public TimelinePostTest(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {

        }
    }
}
