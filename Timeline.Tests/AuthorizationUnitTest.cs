using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Timeline.Controllers;
using Timeline.Tests.Helpers;
using Xunit;
using Xunit.Abstractions;

namespace Timeline.Tests
{
    public class AuthorizationUnitTest : IClassFixture<WebApplicationFactory<Startup>>
    {
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
                var response = await client.GetAsync("/api/Test/Action1");                
                Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            }
        }

        private static async Task<string> Login(HttpClient client, string username, string password)
        {
            var response = await client.PostAsJsonAsync("/api/User/LogIn", new UserController.UserCredentials { Username = username, Password = password });

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var loginInfo = JsonConvert.DeserializeObject<UserController.LoginInfo>(await response.Content.ReadAsStringAsync());

            return loginInfo.Token;
        }

        private static async Task<HttpResponseMessage> GetWithAuthentication(HttpClient client, string path, string token)
        {
            var request = new HttpRequestMessage
            {
                RequestUri = new Uri(client.BaseAddress, path),
                Method = HttpMethod.Get
            };
            request.Headers.Add("Authorization", "Bearer " + token);

            return await client.SendAsync(request);
        }

        [Fact]
        public async Task AuthenticationTest()
        {
            using (var client = _factory.CreateDefaultClient())
            {
                var token = await Login(client, "user", "user");
                var response = await GetWithAuthentication(client, "/api/Test/Action1", token);
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            }
        }

        [Fact]
        public async Task UserAuthorizationTest()
        {
            using (var client = _factory.CreateDefaultClient())
            {
                var token = await Login(client, "user", "user");
                var response1 = await GetWithAuthentication(client, "/api/Test/Action2", token);
                Assert.Equal(HttpStatusCode.OK, response1.StatusCode);
                var response2 = await GetWithAuthentication(client, "/api/Test/Action3", token);
                Assert.Equal(HttpStatusCode.Forbidden, response2.StatusCode);
            }
        }

        [Fact]
        public async Task AdminAuthorizationTest()
        {
            using (var client = _factory.CreateDefaultClient())
            {
                var token = await Login(client, "admin", "admin");
                var response1 = await GetWithAuthentication(client, "/api/Test/Action2", token);
                Assert.Equal(HttpStatusCode.OK, response1.StatusCode);
                var response2 = await GetWithAuthentication(client, "/api/Test/Action3", token);
                Assert.Equal(HttpStatusCode.OK, response2.StatusCode);
            }
        }
    }
}
