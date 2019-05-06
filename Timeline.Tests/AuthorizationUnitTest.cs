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
        private const string NeedAuthorizeUrl = "Test/User/NeedAuthorize";
        private const string BothUserAndAdminUrl = "Test/User/BothUserAndAdmin";
        private const string OnlyAdminUrl = "Test/User/OnlyAdmin";

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
                var response = await client.GetAsync(NeedAuthorizeUrl);
                Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            }
        }

        [Fact]
        public async Task AuthenticationTest()
        {
            using (var client = await _factory.CreateClientWithUser("user", "user"))
            {
                var response = await client.GetAsync(NeedAuthorizeUrl);
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            }
        }

        [Fact]
        public async Task UserAuthorizationTest()
        {
            using (var client = _factory.CreateDefaultClient())
            {
                var token = (await client.CreateUserTokenAsync("user", "user")).Token;
                var response1 = await client.SendWithAuthenticationAsync(token, BothUserAndAdminUrl);
                Assert.Equal(HttpStatusCode.OK, response1.StatusCode);
                var response2 = await client.SendWithAuthenticationAsync(token, OnlyAdminUrl);
                Assert.Equal(HttpStatusCode.Forbidden, response2.StatusCode);
            }
        }

        [Fact]
        public async Task AdminAuthorizationTest()
        {
            using (var client = await _factory.CreateClientWithUser("admin", "admin"))
            {
                var response1 = await client.GetAsync(BothUserAndAdminUrl);
                Assert.Equal(HttpStatusCode.OK, response1.StatusCode);
                var response2 = await client.GetAsync(OnlyAdminUrl);
                Assert.Equal(HttpStatusCode.OK, response2.StatusCode);
            }
        }
    }
}
