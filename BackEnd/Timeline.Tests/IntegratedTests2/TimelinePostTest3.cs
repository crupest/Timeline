using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Timeline.Models;
using Timeline.Models.Http;
using Timeline.Tests.Helpers;
using Xunit;
using Xunit.Abstractions;

namespace Timeline.Tests.IntegratedTests2
{
    public class TimelinePostTest3 : IntegratedTestBase
    {
        public TimelinePostTest3(ITestOutputHelper testOutput) : base(testOutput)
        {
        }

        protected override async Task OnInitializeAsync()
        {
            using var client = CreateClientAsUser();
            await client.TestJsonSendAsync(HttpMethod.Post, "v2/timelines", new HttpTimelineCreateRequest
            {
                Name = "hello"
            }, expectedStatusCode: HttpStatusCode.Created);

            await client.TestJsonSendAsync(HttpMethod.Patch, "v2/timelines/user/hello", new HttpTimelinePatchRequest
            {
                Visibility = TimelineVisibility.Private
            });


            await client.TestJsonSendAsync(HttpMethod.Post, "v2/timelines/user/hello/posts", new HttpTimelinePostCreateRequest
            {
                DataList = new List<HttpTimelinePostCreateRequestData>
                {
                    new HttpTimelinePostCreateRequestData
                    {
                        ContentType = MimeTypes.TextPlain,
                        Data = Convert.ToBase64String(Encoding.UTF8.GetBytes("hello1"))
                    },
                    new HttpTimelinePostCreateRequestData
                    {
                        ContentType = MimeTypes.TextMarkdown,
                        Data = Convert.ToBase64String(Encoding.UTF8.GetBytes("hello*2*"))
                    },
                    new HttpTimelinePostCreateRequestData
                    {
                        ContentType = MimeTypes.ImagePng,
                        Data = Convert.ToBase64String(ImageHelper.CreatePngWithSize(100, 100))
                    }
                }
            }, expectedStatusCode: HttpStatusCode.Created);
        }

        [Fact]
        public async Task PostDataIndexGet()
        {
            using var client = CreateClientAsUser();
            await client.TestSendAsync(HttpMethod.Get, "v2/timelines/user/hello/posts/1/data");
        }

        [Fact]
        public async Task PostDataGet()
        {
            using var client = CreateClientAsUser();
            await client.TestSendAsync(HttpMethod.Get, "v2/timelines/user/hello/posts/1/data/1");
        }

        [Fact]
        public async Task PostDataGetDeleted()
        {
            using var client = CreateClientAsUser();
            await client.TestSendAsync(HttpMethod.Delete, "v2/timelines/user/hello/posts/1", expectedStatusCode: HttpStatusCode.NoContent);
            await client.TestSendAsync(HttpMethod.Get, "v2/timelines/user/hello/posts/1/data", expectedStatusCode: HttpStatusCode.Gone);
            await client.TestSendAsync(HttpMethod.Get, "v2/timelines/user/hello/posts/1/data/1", expectedStatusCode: HttpStatusCode.Gone);
        }

        [Fact]
        public async Task PostDataGetNotExist()
        {
            using var client = CreateClientAsUser();
            await client.TestSendAsync(HttpMethod.Get, "v2/timelines/user/hello/posts/1/data/3", expectedStatusCode: HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task PostDataGetNotLogin()
        {
            using var client = CreateDefaultClient();
            await client.TestSendAsync(HttpMethod.Get, "v2/timelines/user/hello/posts/1/data", expectedStatusCode: HttpStatusCode.Forbidden);
            await client.TestSendAsync(HttpMethod.Get, "v2/timelines/user/hello/posts/1/data/1", expectedStatusCode: HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task PostDataGetForbid()
        {
            await CreateUserAsync("user2", "user2pw");
            using var client = CreateClientWithToken(await CreateTokenWithCredentialAsync("user2", "user2pw"));
            await client.TestSendAsync(HttpMethod.Get, "v2/timelines/user/hello/posts/1/data", expectedStatusCode: HttpStatusCode.Forbidden);
            await client.TestSendAsync(HttpMethod.Get, "v2/timelines/user/hello/posts/1/data/1", expectedStatusCode: HttpStatusCode.Forbidden);
        }
    }
}
