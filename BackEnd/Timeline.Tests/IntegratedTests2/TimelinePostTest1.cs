using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Timeline.Models;
using Timeline.Models.Http;
using Xunit;
using Xunit.Abstractions;

namespace Timeline.Tests.IntegratedTests2
{
    public class TimelinePostTest1 : IntegratedTestBase
    {
        public TimelinePostTest1(ITestOutputHelper testOutput) : base(testOutput)
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
                    }
                }
            }, expectedStatusCode: HttpStatusCode.Created);

            await client.TestJsonSendAsync(HttpMethod.Post, "v2/timelines/user/hello/posts", new HttpTimelinePostCreateRequest
            {
                DataList = new List<HttpTimelinePostCreateRequestData>
                {
                    new HttpTimelinePostCreateRequestData
                    {
                        ContentType = MimeTypes.TextPlain,
                        Data = Convert.ToBase64String(Encoding.UTF8.GetBytes("hello2"))
                    }
                }
            }, expectedStatusCode: HttpStatusCode.Created);

            await client.TestJsonSendAsync(HttpMethod.Post, "v2/timelines/user/hello/posts", new HttpTimelinePostCreateRequest
            {
                DataList = new List<HttpTimelinePostCreateRequestData>
                {
                    new HttpTimelinePostCreateRequestData
                    {
                        ContentType = MimeTypes.TextPlain,
                        Data = Convert.ToBase64String(Encoding.UTF8.GetBytes("hello3"))
                    }
                }
            }, expectedStatusCode: HttpStatusCode.Created);
        }

        [Fact]
        public async Task ListTest()
        {
            using var client = CreateClientAsUser();
            var posts = await client.TestJsonSendAsync<List<HttpTimelinePost>>(HttpMethod.Get, "v2/timelines/user/hello/posts");
            posts.Should().HaveCount(3);
        }

        [Fact]
        public async Task GetTest()
        {
            using var client = CreateClientAsUser();
            await client.TestJsonSendAsync<HttpTimelinePost>(HttpMethod.Get, "v2/timelines/user/hello/posts/1");
        }

        [Fact]
        public async Task ListAndGetForbid()
        {
            await CreateUserAsync("user2", "user2pw");
            var client = CreateClientWithToken(await CreateTokenWithCredentialAsync("user2", "user2pw"));
            await client.TestJsonSendAsync(HttpMethod.Get, "v2/timelines/user/hello/posts", expectedStatusCode: HttpStatusCode.Forbidden);
            await client.TestJsonSendAsync(HttpMethod.Get, "v2/timelines/user/hello/posts/1", expectedStatusCode: HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task ListAndGetForbidForNoAuth()
        {
            var client = CreateDefaultClient();
            await client.TestJsonSendAsync(HttpMethod.Get, "v2/timelines/user/hello/posts", expectedStatusCode: HttpStatusCode.Forbidden);
            await client.TestJsonSendAsync(HttpMethod.Get, "v2/timelines/user/hello/posts/1", expectedStatusCode: HttpStatusCode.Forbidden);
        }
    }
}

