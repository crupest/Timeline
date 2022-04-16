using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Timeline.Models;
using Timeline.Models.Http;
using Xunit;
using Xunit.Abstractions;

namespace Timeline.Tests.IntegratedTests2
{
    public class UserTest : IntegratedTestBase
    {
        public UserTest(ITestOutputHelper testOutput) : base(testOutput)
        {
        }

        [Fact]
        public async Task ListTest()
        {
            using var client = CreateDefaultClient();

            var a = await client.TestJsonSendAsync<Page<HttpUser>>(HttpMethod.Get, "v2/users", expectedStatusCode: HttpStatusCode.OK);
            a.TotalCount.Should().Be(3);
        }

        [Fact]
        public async Task GetTest()
        {
            using var client = CreateDefaultClient();

            var a = await client.TestJsonSendAsync<HttpUser>(HttpMethod.Get, "v2/users/user", expectedStatusCode: HttpStatusCode.OK);
            a.Username.Should().Be("user");
        }

        [Fact]
        public async Task CreateTest()
        {
            using var client = CreateClientAsAdmin();

            var a = await client.TestJsonSendAsync<HttpUser>(HttpMethod.Post, "v2/users", new HttpUserPostRequest
            {
                Username = "user2",
                Password = "user2pw",
                Nickname = "nickname"
            }, expectedStatusCode: HttpStatusCode.Created);

            a.Username.Should().Be("user2");
            a.Nickname.Should().Be("nickname");
        }

        [Fact]
        public async Task CreateNotLogin()
        {
            using var client = CreateDefaultClient();

            await client.TestJsonSendAsync(HttpMethod.Post, "v2/users", new HttpUserPostRequest
            {
                Username = "user2",
                Password = "user2pw",
            }, expectedStatusCode: HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task CreateForbid()
        {
            using var client = CreateClientAsUser();

            await client.TestJsonSendAsync(HttpMethod.Post, "v2/users", new HttpUserPostRequest
            {
                Username = "user2",
                Password = "user2pw",
            }, expectedStatusCode: HttpStatusCode.Forbidden);
        }
    }
}
