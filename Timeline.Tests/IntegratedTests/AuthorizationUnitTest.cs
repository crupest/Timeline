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
    public class AuthorizationUnitTest : IClassFixture<WebApplicationFactory<Startup>>, IDisposable
    {
        private const string AuthorizeUrl = "Test/User/Authorize";
        private const string UserUrl = "Test/User/User";
        private const string AdminUrl = "Test/User/Admin";

        private readonly TestApplication _testApp;
        private readonly WebApplicationFactory<Startup> _factory;

        public AuthorizationUnitTest(WebApplicationFactory<Startup> factory)
        {
            _testApp = new TestApplication(factory);
            _factory = _testApp.Factory;
        }

        public void Dispose()
        {
            _testApp.Dispose();
        }

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
