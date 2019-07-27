using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Threading.Tasks;
using Timeline.Tests.Helpers;
using Timeline.Tests.Helpers.Authentication;
using Xunit;
using Xunit.Abstractions;

namespace Timeline.Tests
{
    public class AuthorizationUnitTest : IClassFixture<WebApplicationFactory<Startup>>
    {
        private const string AuthorizeUrl = "Test/User/Authorize";
        private const string UserUrl = "Test/User/User";
        private const string AdminUrl = "Test/User/Admin";

        private readonly WebApplicationFactory<Startup> _factory;

        public AuthorizationUnitTest(WebApplicationFactory<Startup> factory, ITestOutputHelper outputHelper)
        {
            _factory = factory.WithTestConfig(outputHelper);
        }

        [Fact]
        public async Task UnauthenticationTest()
        {
            using (var client = _factory.CreateDefaultClient())
            {
                var response = await client.GetAsync(AuthorizeUrl);
                Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            }
        }

        [Fact]
        public async Task AuthenticationTest()
        {
            using (var client = await _factory.CreateClientWithUser("user", "user"))
            {
                var response = await client.GetAsync(AuthorizeUrl);
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            }
        }

        [Fact]
        public async Task UserAuthorizationTest()
        {
            using (var client = _factory.CreateDefaultClient())
            {
                var token = (await client.CreateUserTokenAsync("user", "user")).Token;
                var response1 = await client.SendWithAuthenticationAsync(token, UserUrl);
                Assert.Equal(HttpStatusCode.OK, response1.StatusCode);
                var response2 = await client.SendWithAuthenticationAsync(token, AdminUrl);
                Assert.Equal(HttpStatusCode.Forbidden, response2.StatusCode);
            }
        }

        [Fact]
        public async Task AdminAuthorizationTest()
        {
            using (var client = await _factory.CreateClientWithUser("admin", "admin"))
            {
                var response1 = await client.GetAsync(UserUrl);
                Assert.Equal(HttpStatusCode.OK, response1.StatusCode);
                var response2 = await client.GetAsync(AdminUrl);
                Assert.Equal(HttpStatusCode.OK, response2.StatusCode);
            }
        }
    }
}
