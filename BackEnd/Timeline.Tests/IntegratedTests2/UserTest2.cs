using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Timeline.Models.Http;
using Xunit;
using Xunit.Abstractions;

namespace Timeline.Tests.IntegratedTests2
{
    public class UserTest2 : IntegratedTestBase
    {
        public UserTest2(ITestOutputHelper testOutput) : base(testOutput)
        {
        }

        [Fact]
        public async Task UserPatchTest()
        {
            var a = await UserClient.TestJsonSendAsync<HttpUser>(HttpMethod.Patch, "v2/users/user", new HttpUserPatchRequest
            {
                Nickname = "nick"
            });
            a.Nickname.Should().Be("nick");
        }

        [Fact]
        public async Task UserPatchNotFound()
        {
            await AdminClient.TestJsonSendAsync(HttpMethod.Patch, "v2/users/notexist", new HttpUserPatchRequest
            {
                Nickname = "nick"
            }, expectedStatusCode: HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task UserPatchUnauthorize()
        {
            await DefaultClient.TestJsonSendAsync(HttpMethod.Patch, "v2/users/user", new HttpUserPatchRequest
            {
                Nickname = "nick"
            }, expectedStatusCode: HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task UserPatchForbid()
        {
            await UserClient.TestJsonSendAsync(HttpMethod.Patch, "v2/users/user", new HttpUserPatchRequest
            {
                Username = "hello"
            }, expectedStatusCode: HttpStatusCode.Forbidden);

            await UserClient.TestJsonSendAsync(HttpMethod.Patch, "v2/users/user", new HttpUserPatchRequest
            {
                Password = "hello"
            }, expectedStatusCode: HttpStatusCode.Forbidden);

            await UserClient.TestJsonSendAsync(HttpMethod.Patch, "v2/users/admin", new HttpUserPatchRequest
            {
                Nickname = "nick"
            }, expectedStatusCode: HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task AdminPatchTest()
        {
            var a = await AdminClient.TestJsonSendAsync<HttpUser>(HttpMethod.Patch, "v2/users/user", new HttpUserPatchRequest
            {
                Username = "hello",
                Password = "hellopw",
                Nickname = "nick"
            });
            a.Username.Should().Be("hello");
            a.Nickname.Should().Be("nick");
        }

        [Fact]
        public async Task DeleteTest()
        {
            await AdminClient.TestSendAsync(HttpMethod.Delete, "v2/users/user");
        }

        [Fact]
        public async Task DeleteUnauthorized()
        {
            await DefaultClient.TestSendAsync(HttpMethod.Delete, "v2/users/user", expectedStatusCode: HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task DeleteForbid()
        {
            await UserClient.TestSendAsync(HttpMethod.Delete, "v2/users/user", expectedStatusCode: HttpStatusCode.Forbidden);
        }
    }
}

