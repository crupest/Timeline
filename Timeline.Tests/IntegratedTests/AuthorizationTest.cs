using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System;
using System.Net;
using System.Threading.Tasks;
using Timeline.Tests.Helpers;
using Timeline.Tests.Helpers.Authentication;
using Xunit;

namespace Timeline.Tests.IntegratedTests
{
    public class AuthorizationTest : IClassFixture<WebApplicationFactory<Startup>>, IDisposable
    {
        private readonly TestApplication _testApp;
        private readonly WebApplicationFactory<Startup> _factory;

        public AuthorizationTest(WebApplicationFactory<Startup> factory)
        {
            _testApp = new TestApplication(factory);
            _factory = _testApp.Factory;
        }

        public void Dispose()
        {
            _testApp.Dispose();
        }

        private const string BaseUrl = "testing/auth/";
        private const string AuthorizeUrl = BaseUrl + "Authorize";
        private const string UserUrl = BaseUrl + "User";
        private const string AdminUrl = BaseUrl + "Admin";

        [Fact]
        public async Task UnauthenticationTest()
        {
            using var client = _factory.CreateDefaultClient();
            var response = await client.GetAsync(AuthorizeUrl);
            response.Should().HaveStatusCode(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task AuthenticationTest()
        {
            using var client = await _factory.CreateClientAsUser();
            var response = await client.GetAsync(AuthorizeUrl);
            response.Should().HaveStatusCode(HttpStatusCode.OK);
        }

        [Fact]
        public async Task UserAuthorizationTest()
        {
            using var client = await _factory.CreateClientAsUser();
            var response1 = await client.GetAsync(UserUrl);
            response1.Should().HaveStatusCode(HttpStatusCode.OK);
            var response2 = await client.GetAsync(AdminUrl);
            response2.Should().HaveStatusCode(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task AdminAuthorizationTest()
        {
            using var client = await _factory.CreateClientAsAdmin();
            var response1 = await client.GetAsync(UserUrl);
            response1.Should().HaveStatusCode(HttpStatusCode.OK);
            var response2 = await client.GetAsync(AdminUrl);
            response2.Should().HaveStatusCode(HttpStatusCode.OK);
        }
    }
}
