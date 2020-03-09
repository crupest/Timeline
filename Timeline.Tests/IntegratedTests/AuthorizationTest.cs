using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Threading.Tasks;
using TimelineApp.Tests.Helpers;
using Xunit;

namespace TimelineApp.Tests.IntegratedTests
{
    public class AuthorizationTest : IntegratedTestBase
    {
        public AuthorizationTest(WebApplicationFactory<Startup> factory)
            : base(factory)
        {
        }

        private const string BaseUrl = "testing/auth/";
        private const string AuthorizeUrl = BaseUrl + "Authorize";
        private const string UserUrl = BaseUrl + "User";
        private const string AdminUrl = BaseUrl + "Admin";

        [Fact]
        public async Task UnauthenticationTest()
        {
            using var client = await CreateDefaultClient();
            var response = await client.GetAsync(AuthorizeUrl);
            response.Should().HaveStatusCode(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task AuthenticationTest()
        {
            using var client = await CreateClientAsUser();
            var response = await client.GetAsync(AuthorizeUrl);
            response.Should().HaveStatusCode(HttpStatusCode.OK);
        }

        [Fact]
        public async Task UserAuthorizationTest()
        {
            using var client = await CreateClientAsUser();
            var response1 = await client.GetAsync(UserUrl);
            response1.Should().HaveStatusCode(HttpStatusCode.OK);
            var response2 = await client.GetAsync(AdminUrl);
            response2.Should().HaveStatusCode(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task AdminAuthorizationTest()
        {
            using var client = await CreateClientAsAdministrator();
            var response1 = await client.GetAsync(UserUrl);
            response1.Should().HaveStatusCode(HttpStatusCode.OK);
            var response2 = await client.GetAsync(AdminUrl);
            response2.Should().HaveStatusCode(HttpStatusCode.OK);
        }
    }
}
