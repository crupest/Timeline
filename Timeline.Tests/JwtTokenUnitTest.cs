using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Timeline.Controllers;
using Timeline.Services;
using Timeline.Tests.Helpers;
using Timeline.Tests.Helpers.Authentication;
using Xunit;
using Xunit.Abstractions;

namespace Timeline.Tests
{
    public class JwtTokenUnitTest : IClassFixture<WebApplicationFactory<Startup>>
    {
        private const string ValidateTokenUrl = "/api/User/ValidateToken";

        private readonly WebApplicationFactory<Startup> _factory;

        public JwtTokenUnitTest(WebApplicationFactory<Startup> factory, ITestOutputHelper outputHelper)
        {
            _factory = factory.WithTestConfig(outputHelper);
        }

        [Fact]
        public async void ValidateToken_BadTokenTest()
        {
            using (var client = _factory.CreateDefaultClient())
            {
                var response = await client.PostAsync(ValidateTokenUrl, new StringContent("bad token hahaha", Encoding.UTF8, "text/plain"));

                Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                var validationInfo = JsonConvert.DeserializeObject<TokenValidationResult>(await response.Content.ReadAsStringAsync());

                Assert.False(validationInfo.IsValid);
                Assert.Null(validationInfo.UserInfo);
            }
        }

        [Fact]
        public async void ValidateToken_PlainTextGoodTokenTest()
        {
            using (var client = _factory.CreateDefaultClient())
            {
                var createTokenResult = await client.CreateUserTokenAsync("admin", "admin");

                var response = await client.PostAsync(ValidateTokenUrl, new StringContent(createTokenResult.Token, Encoding.UTF8, "text/plain"));

                Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                var result = JsonConvert.DeserializeObject<TokenValidationResult>(await response.Content.ReadAsStringAsync());

                Assert.True(result.IsValid);
                Assert.NotNull(result.UserInfo);
                Assert.Equal(createTokenResult.UserInfo.Username, result.UserInfo.Username);
                Assert.Equal(createTokenResult.UserInfo.Roles, result.UserInfo.Roles);
            }
        }

        [Fact]
        public async void ValidateToken_JsonGoodTokenTest()
        {
            using (var client = _factory.CreateDefaultClient())
            {
                var createTokenResult = await client.CreateUserTokenAsync("admin", "admin");

                var response = await client.PostAsJsonAsync(ValidateTokenUrl, new UserController.TokenValidationRequest { Token = createTokenResult.Token });

                Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                var result = JsonConvert.DeserializeObject<TokenValidationResult>(await response.Content.ReadAsStringAsync());

                Assert.True(result.IsValid);
                Assert.NotNull(result.UserInfo);
                Assert.Equal(createTokenResult.UserInfo.Username, result.UserInfo.Username);
                Assert.Equal(createTokenResult.UserInfo.Roles, result.UserInfo.Roles);
            }
        }
    }
}
