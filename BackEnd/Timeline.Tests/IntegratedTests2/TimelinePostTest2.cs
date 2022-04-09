using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Timeline.Models;
using Timeline.Models.Http;
using Xunit;
using Xunit.Abstractions;

namespace Timeline.Tests.IntegratedTests2
{
    public class TimelinePostTest2 : IntegratedTestBase
    {
        public TimelinePostTest2(ITestOutputHelper testOutput) : base(testOutput)
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
        public async Task PostNotLogin()
        {
            using var client = CreateDefaultClient();
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
            }, expectedStatusCode: HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task PostForbid()
        {
            await CreateUserAsync("user2", "user2pw");
            using var client = CreateClientWithToken(await CreateTokenWithCredentialAsync("user2", "user2pw"));
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
            }, expectedStatusCode: HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task PostNotExist()
        {
            using var client = CreateClientAsUser();
            await client.TestJsonSendAsync(HttpMethod.Post, "v2/timelines/user/notexist/posts", new HttpTimelinePostCreateRequest
            {
                DataList = new List<HttpTimelinePostCreateRequestData>
                {
                    new HttpTimelinePostCreateRequestData
                    {
                        ContentType = MimeTypes.TextPlain,
                        Data = Convert.ToBase64String(Encoding.UTF8.GetBytes("hello3"))
                    }
                }
            }, expectedStatusCode: HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task PostDeleteTest()
        {
            using var client = CreateClientAsUser();
            await client.TestSendAsync(HttpMethod.Delete, "v2/timelines/user/hello/posts/1", expectedStatusCode: HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task PostDeleteNotLogin()
        {
            using var client = CreateDefaultClient();
            await client.TestSendAsync(HttpMethod.Delete, "v2/timelines/user/hello/posts/1", expectedStatusCode: HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task PostDeleteForbid()
        {
            await CreateUserAsync("user2", "user2pw");
            using var client = CreateClientWithToken(await CreateTokenWithCredentialAsync("user2", "user2pw"));
            await client.TestSendAsync(HttpMethod.Delete, "v2/timelines/user/hello/posts/1", expectedStatusCode: HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task PostDeleteNotExist()
        {
            using var client = CreateClientAsUser();
            await client.TestSendAsync(HttpMethod.Delete, "v2/timelines/user/hello/posts/4", expectedStatusCode: HttpStatusCode.NotFound);
            await client.TestSendAsync(HttpMethod.Delete, "v2/timelines/user/notexist/posts/1", expectedStatusCode: HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task PostPatchTest()
        {
            using var client = CreateClientAsUser();
            await client.TestJsonSendAsync(HttpMethod.Patch, "v2/timelines/user/hello/posts/1", new HttpTimelinePostPatchRequest
            {
                Color = "#FFFFFF"
            });
        }

        [Fact]
        public async Task PostPatchNotLogin()
        {
            using var client = CreateDefaultClient();
            await client.TestJsonSendAsync(HttpMethod.Patch, "v2/timelines/user/hello/posts/1", new HttpTimelinePostPatchRequest
            {
                Color = "#FFFFFF"
            }, expectedStatusCode: HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task PostPatchForbid()
        {
            await CreateUserAsync("user2", "user2pw");
            using var client = CreateClientWithToken(await CreateTokenWithCredentialAsync("user2", "user2pw"));
            await client.TestJsonSendAsync(HttpMethod.Patch, "v2/timelines/user/hello/posts/1", new HttpTimelinePostPatchRequest
            {
                Color = "#FFFFFF"
            }, expectedStatusCode: HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task PostPatchNotExist()
        {
            using var client = CreateClientAsUser();
            await client.TestJsonSendAsync(HttpMethod.Patch, "v2/timelines/user/hello/posts/4", new HttpTimelinePostPatchRequest
            {
                Color = "#FFFFFF"
            }, expectedStatusCode: HttpStatusCode.NotFound);
        }
    }
}

