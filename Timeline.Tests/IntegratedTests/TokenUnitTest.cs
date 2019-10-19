using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Timeline.Models.Http;
using Timeline.Services;
using Timeline.Tests.Helpers;
using Timeline.Tests.Helpers.Authentication;
using Timeline.Tests.Mock.Data;
using Xunit;
using static Timeline.ErrorCodes.Http.Token;

namespace Timeline.Tests.IntegratedTests
{
    public class TokenUnitTest : IClassFixture<WebApplicationFactory<Startup>>, IDisposable
    {
        private const string CreateTokenUrl = "token/create";
        private const string VerifyTokenUrl = "token/verify";

        private readonly TestApplication _testApp;
        private readonly WebApplicationFactory<Startup> _factory;

        public TokenUnitTest(WebApplicationFactory<Startup> factory)
        {
            _testApp = new TestApplication(factory);
            _factory = _testApp.Factory;
        }

        public void Dispose()
        {
            _testApp.Dispose();
        }

        public static IEnumerable<object[]> CreateToken_InvalidModel_Data()
        {
            yield return new[] { null, "p", null };
            yield return new[] { "u", null, null };
            yield return new object[] { "u", "p", 2000 };
            yield return new object[] { "u", "p", -1 };
        }

        [Theory]
        [MemberData(nameof(CreateToken_InvalidModel_Data))]
        public async Task CreateToken_InvalidModel(string username, string password, int expire)
        {
            using var client = _factory.CreateDefaultClient();
            (await client.PostAsJsonAsync(CreateTokenUrl, new CreateTokenRequest
            {
                Username = username,
                Password = password,
                Expire = expire
            })).Should().BeInvalidModel();
        }

        public static IEnumerable<object[]> CreateToken_UserCredential_Data()
        {
            yield return new[] { "usernotexist", "p" };
            yield return new[] { MockUser.User.Username, "???" };
        }

        [Theory]
        [MemberData(nameof(CreateToken_UserCredential_Data))]
        public async void CreateToken_UserCredential(string username, string password)
        {
            using var client = _factory.CreateDefaultClient();
            var response = await client.PostAsJsonAsync(CreateTokenUrl,
                new CreateTokenRequest { Username = username, Password = password });
            response.Should().HaveStatusCode(400)
                .And.Should().HaveCommonBody()
                .Which.Code.Should().Be(Create.BadCredential);
        }

        [Fact]
        public async Task CreateToken_Success()
        {
            using var client = _factory.CreateDefaultClient();
            var response = await client.PostAsJsonAsync(CreateTokenUrl,
                new CreateTokenRequest { Username = MockUser.User.Username, Password = MockUser.User.Password });
            var body = response.Should().HaveStatusCode(200)
               .And.Should().HaveJsonBody<CreateTokenResponse>().Which;
            body.Token.Should().NotBeNullOrWhiteSpace();
            body.User.Should().BeEquivalentTo(MockUser.User.Info);
        }

        [Fact]
        public async Task VerifyToken_InvalidModel()
        {
            using var client = _factory.CreateDefaultClient();
            (await client.PostAsJsonAsync(VerifyTokenUrl,
                new VerifyTokenRequest { Token = null })).Should().BeInvalidModel();
        }

        [Fact]
        public async Task VerifyToken_BadToken()
        {
            using var client = _factory.CreateDefaultClient();
            var response = await client.PostAsJsonAsync(VerifyTokenUrl,
                new VerifyTokenRequest { Token = "bad token hahaha" });
            response.Should().HaveStatusCode(400)
                 .And.Should().HaveCommonBody()
                 .Which.Code.Should().Be(Verify.BadFormat);
        }

        [Fact]
        public async Task VerifyToken_BadVersion()
        {
            using var client = _factory.CreateDefaultClient();
            var token = (await client.CreateUserTokenAsync(MockUser.User.Username, MockUser.User.Password)).Token;

            using (var scope = _factory.Server.Host.Services.CreateScope()) // UserService is scoped.
            {
                // create a user for test
                var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
                await userService.PatchUser(MockUser.User.Username, null, null);
            }

            (await client.PostAsJsonAsync(VerifyTokenUrl,
                new VerifyTokenRequest { Token = token }))
                .Should().HaveStatusCode(400)
                .And.Should().HaveCommonBody()
                .Which.Code.Should().Be(Verify.OldVersion);
        }

        [Fact]
        public async Task VerifyToken_UserNotExist()
        {
            using var client = _factory.CreateDefaultClient();
            var token = (await client.CreateUserTokenAsync(MockUser.User.Username, MockUser.User.Password)).Token;

            using (var scope = _factory.Server.Host.Services.CreateScope()) // UserService is scoped.
            {
                var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
                await userService.DeleteUser(MockUser.User.Username);
            }

            (await client.PostAsJsonAsync(VerifyTokenUrl,
                new VerifyTokenRequest { Token = token }))
                .Should().HaveStatusCode(400)
                .And.Should().HaveCommonBody()
                .Which.Code.Should().Be(Verify.UserNotExist);
        }

        //[Fact]
        //public async Task VerifyToken_Expired()
        //{
        //    using (var client = _factory.CreateDefaultClient())
        //    {
        //        // I can only control the token expired time but not current time
        //        // because verify logic is encapsuled in other library.
        //        var mockClock = _factory.GetTestClock();
        //        mockClock.MockCurrentTime = DateTime.Now - TimeSpan.FromDays(2);
        //        var token = (await client.CreateUserTokenAsync(MockUsers.UserUsername, MockUsers.UserPassword, 1)).Token;
        //        var response = await client.PostAsJsonAsync(VerifyTokenUrl,
        //            new VerifyTokenRequest { Token = token });
        //        response.Should().HaveStatusCodeBadRequest()
        //            .And.Should().HaveBodyAsCommonResponseWithCode(TokenController.ErrorCodes.Verify_Expired);
        //        mockClock.MockCurrentTime = null;
        //    }
        //}

        [Fact]
        public async Task VerifyToken_Success()
        {
            using var client = _factory.CreateDefaultClient();
            var createTokenResult = await client.CreateUserTokenAsync(MockUser.User.Username, MockUser.User.Password);
            var response = await client.PostAsJsonAsync(VerifyTokenUrl,
                new VerifyTokenRequest { Token = createTokenResult.Token });
            response.Should().HaveStatusCode(200)
                .And.Should().HaveJsonBody<VerifyTokenResponse>()
                .Which.User.Should().BeEquivalentTo(MockUser.User.Info);
        }
    }
}
