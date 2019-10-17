using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Timeline.Models.Http;
using Timeline.Services;
using Timeline.Tests.Helpers;
using Timeline.Tests.Helpers.Authentication;
using Timeline.Tests.Mock.Data;
using Xunit;
using static Timeline.ErrorCodes.Http.Token;

namespace Timeline.Tests
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
        [Fact]
        public async Task CreateToken_InvalidModel()
        {
            using var client = _factory.CreateDefaultClient();
            // missing username
            await InvalidModelTestHelpers.TestPostInvalidModel(client, CreateTokenUrl,
                new CreateTokenRequest { Username = null, Password = "user" });
            // missing password
            await InvalidModelTestHelpers.TestPostInvalidModel(client, CreateTokenUrl,
                new CreateTokenRequest { Username = "user", Password = null });
            // bad expire offset
            await InvalidModelTestHelpers.TestPostInvalidModel(client, CreateTokenUrl,
                new CreateTokenRequest
                {
                    Username = "user",
                    Password = "password",
                    Expire = 1000
                });
        }

        [Fact]
        public async void CreateToken_UserNotExist()
        {
            using var client = _factory.CreateDefaultClient();
            var response = await client.PostAsJsonAsync(CreateTokenUrl,
                new CreateTokenRequest { Username = "usernotexist", Password = "???" });
            response.Should().HaveStatusCodeBadRequest()
                .And.Should().HaveBodyAsCommonResponseWithCode(Create.BadCredential);
        }

        [Fact]
        public async void CreateToken_BadPassword()
        {
            using (var client = _factory.CreateDefaultClient())
            {
                var response = await client.PostAsJsonAsync(CreateTokenUrl,
                    new CreateTokenRequest { Username = MockUser.User.Username, Password = "???" });
                response.Should().HaveStatusCodeBadRequest()
                    .And.Should().HaveBodyAsCommonResponseWithCode(Create.BadCredential);
            }
        }

        [Fact]
        public async void CreateToken_Success()
        {
            using (var client = _factory.CreateDefaultClient())
            {
                var response = await client.PostAsJsonAsync(CreateTokenUrl,
                    new CreateTokenRequest { Username = MockUser.User.Username, Password = MockUser.User.Password });
                var body = response.Should().HaveStatusCodeOk()
                   .And.Should().HaveBodyAsJson<CreateTokenResponse>().Which;
                body.Token.Should().NotBeNullOrWhiteSpace();
                body.User.Should().BeEquivalentTo(MockUser.User.Info);
            }
        }

        [Fact]
        public async void VerifyToken_InvalidModel()
        {
            using (var client = _factory.CreateDefaultClient())
            {
                // missing token
                await InvalidModelTestHelpers.TestPostInvalidModel(client, VerifyTokenUrl,
                    new VerifyTokenRequest { Token = null });
            }
        }

        [Fact]
        public async void VerifyToken_BadToken()
        {
            using (var client = _factory.CreateDefaultClient())
            {
                var response = await client.PostAsJsonAsync(VerifyTokenUrl, new VerifyTokenRequest { Token = "bad token hahaha" });
                response.Should().HaveStatusCodeBadRequest()
                     .And.Should().HaveBodyAsCommonResponseWithCode(Verify.BadFormat);
            }
        }

        [Fact]
        public async void VerifyToken_BadVersion()
        {
            using (var client = _factory.CreateDefaultClient())
            {
                var token = (await client.CreateUserTokenAsync(MockUser.User.Username, MockUser.User.Password)).Token;

                using (var scope = _factory.Server.Host.Services.CreateScope()) // UserService is scoped.
                {
                    // create a user for test
                    var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
                    await userService.PatchUser(MockUser.User.Username, null, null);
                }

                var response = await client.PostAsJsonAsync(VerifyTokenUrl, new VerifyTokenRequest { Token = token });
                response.Should().HaveStatusCodeBadRequest()
                    .And.Should().HaveBodyAsCommonResponseWithCode(Verify.OldVersion);
            }
        }

        [Fact]
        public async void VerifyToken_UserNotExist()
        {
            using (var client = _factory.CreateDefaultClient())
            {
                var token = (await client.CreateUserTokenAsync(MockUser.User.Username, MockUser.User.Password)).Token;

                using (var scope = _factory.Server.Host.Services.CreateScope()) // UserService is scoped.
                {
                    // create a user for test
                    var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
                    await userService.DeleteUser(MockUser.User.Username);
                }

                var response = await client.PostAsJsonAsync(VerifyTokenUrl, new VerifyTokenRequest { Token = token });
                response.Should().HaveStatusCodeBadRequest()
                    .And.Should().HaveBodyAsCommonResponseWithCode(Verify.UserNotExist);
            }
        }

        //[Fact]
        //public async void VerifyToken_Expired()
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
        public async void VerifyToken_Success()
        {
            using (var client = _factory.CreateDefaultClient())
            {
                var createTokenResult = await client.CreateUserTokenAsync(MockUser.User.Username, MockUser.User.Password);
                var response = await client.PostAsJsonAsync(VerifyTokenUrl,
                    new VerifyTokenRequest { Token = createTokenResult.Token });
                response.Should().HaveStatusCodeOk()
                    .And.Should().HaveBodyAsJson<VerifyTokenResponse>()
                    .Which.User.Should().BeEquivalentTo(MockUser.User.Info);
            }
        }
    }
}
