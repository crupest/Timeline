using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Threading.Tasks;
using Timeline.Tests.Helpers;
using Timeline.Tests.Helpers.Authentication;
using Xunit;
using Xunit.Abstractions;

namespace Timeline.Tests
{
    public class AuthorizationUnitTest : IClassFixture<MyWebApplicationFactory<Startup>>
    {
        private const string AuthorizeUrl = "Test/User/Authorize";
        private const string UserUrl = "Test/User/User";
        private const string AdminUrl = "Test/User/Admin";

        private readonly WebApplicationFactory<Startup> _factory;

        public AuthorizationUnitTest(MyWebApplicationFactory<Startup> factory, ITestOutputHelper outputHelper)
        {
            _factory = factory.WithTestLogging(outputHelper);
        }

        [Fact]
        public async Task UnauthenticationTest()
        {
            using (var client = _factory.CreateDefaultClient())
            {
                var response = await client.GetAsync(AuthorizeUrl);
                response.Should().HaveStatusCode(HttpStatusCode.Unauthorized);
            }
        }

        [Fact]
        public async Task AuthenticationTest()
        {
            using (var client = await _factory.CreateClientAsUser())
            {
                var response = await client.GetAsync(AuthorizeUrl);
                response.Should().HaveStatusCode(HttpStatusCode.OK);
            }
        }

        [Fact]
        public async Task UserAuthorizationTest()
        {
            using (var client = await _factory.CreateClientAsUser())
            {
                var response1 = await client.GetAsync(UserUrl);
                response1.Should().HaveStatusCode(HttpStatusCode.OK);
                var response2 = await client.GetAsync(AdminUrl);
                response2.Should().HaveStatusCode(HttpStatusCode.Forbidden);
            }
        }

        [Fact]
        public async Task AdminAuthorizationTest()
        {
            using (var client = await _factory.CreateClientAsAdmin())
            {
                var response1 = await client.GetAsync(UserUrl);
                response1.Should().HaveStatusCode(HttpStatusCode.OK);
                var response2 = await client.GetAsync(AdminUrl);
                response2.Should().HaveStatusCode(HttpStatusCode.OK);
            }
        }
    }
}
