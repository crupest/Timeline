using Microsoft.AspNetCore.Mvc.Testing;
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
    public class UnitTest : IClassFixture<WebApplicationFactory<Startup>>
    {
        private readonly WebApplicationFactory<Startup> _factory;

        public UnitTest(WebApplicationFactory<Startup> factory, ITestOutputHelper outputHelper)
        {
            _factory = factory.WithTestConfig(outputHelper);
        }

        [Fact]
        public async Task UnauthenticationTest()
        {
            using (var client = _factory.CreateDefaultClient())
            {
                var response = await client.GetAsync("/api/SampleData/WeatherForecasts");
                
                Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            }
        }

        [Fact]
        public async Task AuthenticationTest()
        {
            using (var client = _factory.CreateDefaultClient())
            {
                var response = await client.PostAsJsonAsync("/api/User/LogIn", new UserController.UserCredentials { Username = "hello", Password = "crupest" });

                Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                var token = response.Headers.GetValues("Authorization").Single();

                var request = new HttpRequestMessage
                {
                    RequestUri = new Uri(client.BaseAddress, "/api/SampleData/WeatherForecasts"),
                    Method = HttpMethod.Get
                };
                request.Headers.Add("Authorization", token);

                var response2 = await client.SendAsync(request);

                Assert.Equal(HttpStatusCode.OK, response2.StatusCode);
            }
        }
    }
}
