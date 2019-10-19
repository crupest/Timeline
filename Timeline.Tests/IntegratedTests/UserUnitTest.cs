using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Timeline.Controllers;
using Timeline.Models;
using Timeline.Models.Http;
using Timeline.Tests.Helpers;
using Timeline.Tests.Helpers.Authentication;
using Timeline.Tests.Mock.Data;
using Xunit;

namespace Timeline.Tests.IntegratedTests
{
    public class UserUnitTest : IClassFixture<WebApplicationFactory<Startup>>, IDisposable
    {
        private readonly TestApplication _testApp;
        private readonly WebApplicationFactory<Startup> _factory;

        public UserUnitTest(WebApplicationFactory<Startup> factory)
        {
            _testApp = new TestApplication(factory);
            _factory = _testApp.Factory;
        }

        public void Dispose()
        {
            _testApp.Dispose();
        }

        [Fact]
        public async Task Get_Users_List()
        {
            using var client = await _factory.CreateClientAsAdmin();
            var res = await client.GetAsync("users");
            res.Should().HaveStatusCode(200)
                .And.Should().HaveJsonBody<UserInfo[]>()
                .Which.Should().BeEquivalentTo(MockUser.UserInfoList);
        }

        [Fact]
        public async Task Get_Users_User()
        {
            using var client = await _factory.CreateClientAsAdmin();
            var res = await client.GetAsync("users/" + MockUser.User.Username);
            res.Should().HaveStatusCode(200)
                .And.Should().HaveJsonBody<UserInfo>()
                .Which.Should().BeEquivalentTo(MockUser.User.Info);
        }

        [Fact]
        public async Task Get_Users_404()
        {
            using var client = await _factory.CreateClientAsAdmin();
            var res = await client.GetAsync("users/usernotexist");
            res.Should().HaveStatusCode(404)
                .And.Should().HaveCommonBody()
                .Which.Code.Should().Be(UserController.ErrorCodes.Get_NotExist);
        }

        public static IEnumerable<object[]> Put_InvalidModel_Data()
        {
            yield return new object[] { null, false };
            yield return new object[] { "p", null };
        }

        [Theory]
        [MemberData(nameof(Put_InvalidModel_Data))]
        public async Task Put_InvalidModel(string password, bool? administrator)
        {
            using var client = await _factory.CreateClientAsAdmin();
            const string url = "users/aaaaaaaa";
            (await client.PutAsJsonAsync(url,
                new UserPutRequest { Password = password, Administrator = administrator }))
                .Should().BeInvalidModel();
        }

        [Fact]
        public async Task Put_BadUsername()
        {
            using var client = await _factory.CreateClientAsAdmin();
            var res = await client.PutAsJsonAsync("users/dsf fddf", new UserPutRequest
            {
                Password = "???",
                Administrator = false
            });
            res.Should().HaveStatusCode(400)
                .And.Should().HaveCommonBody()
                .Which.Code.Should().Be(UserController.ErrorCodes.Put_BadUsername);
        }

        private async Task CheckAdministrator(HttpClient client, string username, bool administrator)
        {
            var res = await client.GetAsync("users/" + username);
            res.Should().HaveStatusCode(200)
                .And.Should().HaveJsonBody<UserInfo>()
                .Which.Administrator.Should().Be(administrator);
        }

        [Fact]
        public async Task Put_Modiefied()
        {
            using var client = await _factory.CreateClientAsAdmin();
            var res = await client.PutAsJsonAsync("users/" + MockUser.User.Username, new UserPutRequest
            {
                Password = "password",
                Administrator = false
            });
            res.Should().BePutModify();
            await CheckAdministrator(client, MockUser.User.Username, false);
        }

        [Fact]
        public async Task Put_Created()
        {
            using var client = await _factory.CreateClientAsAdmin();
            const string username = "puttest";
            const string url = "users/" + username;

            var res = await client.PutAsJsonAsync(url, new UserPutRequest
            {
                Password = "password",
                Administrator = false
            });
            res.Should().BePutCreate();
            await CheckAdministrator(client, username, false);
        }

        [Fact]
        public async Task Patch_NotExist()
        {
            using var client = await _factory.CreateClientAsAdmin();
            var res = await client.PatchAsJsonAsync("users/usernotexist", new UserPatchRequest { });
            res.Should().HaveStatusCode(404)
                .And.Should().HaveCommonBody()
                .Which.Code.Should().Be(UserController.ErrorCodes.Patch_NotExist);
        }

        [Fact]
        public async Task Patch_Success()
        {
            using var client = await _factory.CreateClientAsAdmin();
            {
                var res = await client.PatchAsJsonAsync("users/" + MockUser.User.Username,
                    new UserPatchRequest { Administrator = false });
                res.Should().HaveStatusCode(200);
                await CheckAdministrator(client, MockUser.User.Username, false);
            }
        }

        [Fact]
        public async Task Delete_Deleted()
        {
            using var client = await _factory.CreateClientAsAdmin();
            var url = "users/" + MockUser.User.Username;
            var res = await client.DeleteAsync(url);
            res.Should().BeDeleteDelete();

            var res2 = await client.GetAsync(url);
            res2.Should().HaveStatusCode(404);
        }

        [Fact]
        public async Task Delete_NotExist()
        {
            using var client = await _factory.CreateClientAsAdmin();
            var res = await client.DeleteAsync("users/usernotexist");
            res.Should().BeDeleteNotExist();
        }


        public class ChangeUsernameUnitTest : IClassFixture<WebApplicationFactory<Startup>>, IDisposable
        {
            private const string url = "userop/changeusername";

            private readonly TestApplication _testApp;
            private readonly WebApplicationFactory<Startup> _factory;

            public ChangeUsernameUnitTest(WebApplicationFactory<Startup> factory)
            {
                _testApp = new TestApplication(factory);
                _factory = _testApp.Factory;
            }

            public void Dispose()
            {
                _testApp.Dispose();
            }

            public static IEnumerable<object[]> InvalidModel_Data()
            {
                yield return new[] { null, "uuu" };
                yield return new[] { "uuu", null };
                yield return new[] { "uuu", "???" };
            }

            [Theory]
            [MemberData(nameof(InvalidModel_Data))]
            public async Task InvalidModel(string oldUsername, string newUsername)
            {
                using var client = await _factory.CreateClientAsAdmin();
                (await client.PostAsJsonAsync(url,
                    new ChangeUsernameRequest { OldUsername = oldUsername, NewUsername = newUsername }))
                    .Should().BeInvalidModel();
            }

            [Fact]
            public async Task UserNotExist()
            {
                using var client = await _factory.CreateClientAsAdmin();
                var res = await client.PostAsJsonAsync(url,
                    new ChangeUsernameRequest { OldUsername = "usernotexist", NewUsername = "newUsername" });
                res.Should().HaveStatusCode(400)
                    .And.Should().HaveCommonBody()
                    .Which.Code.Should().Be(UserController.ErrorCodes.ChangeUsername_NotExist);
            }

            [Fact]
            public async Task UserAlreadyExist()
            {
                using var client = await _factory.CreateClientAsAdmin();
                var res = await client.PostAsJsonAsync(url,
                    new ChangeUsernameRequest { OldUsername = MockUser.User.Username, NewUsername = MockUser.Admin.Username });
                res.Should().HaveStatusCode(400)
                    .And.Should().HaveCommonBody()
                    .Which.Code.Should().Be(UserController.ErrorCodes.ChangeUsername_AlreadyExist);
            }

            [Fact]
            public async Task Success()
            {
                using var client = await _factory.CreateClientAsAdmin();
                const string newUsername = "hahaha";
                var res = await client.PostAsJsonAsync(url,
                    new ChangeUsernameRequest { OldUsername = MockUser.User.Username, NewUsername = newUsername });
                res.Should().HaveStatusCode(200);
                await client.CreateUserTokenAsync(newUsername, MockUser.User.Password);
            }
        }


        public class ChangePasswordUnitTest : IClassFixture<WebApplicationFactory<Startup>>, IDisposable
        {
            private const string url = "userop/changepassword";

            private readonly TestApplication _testApp;
            private readonly WebApplicationFactory<Startup> _factory;

            public ChangePasswordUnitTest(WebApplicationFactory<Startup> factory)
            {
                _testApp = new TestApplication(factory);
                _factory = _testApp.Factory;
            }

            public void Dispose()
            {
                _testApp.Dispose();
            }

            public static IEnumerable<object[]> InvalidModel_Data()
            {
                yield return new[] { null, "ppp" };
                yield return new[] { "ppp", null };
            }

            [Theory]
            [MemberData(nameof(InvalidModel_Data))]
            public async Task InvalidModel(string oldPassword, string newPassword)
            {
                using var client = await _factory.CreateClientAsUser();
                (await client.PostAsJsonAsync(url,
                    new ChangePasswordRequest { OldPassword = oldPassword, NewPassword = newPassword }))
                    .Should().BeInvalidModel();
            }

            [Fact]
            public async Task BadOldPassword()
            {
                using var client = await _factory.CreateClientAsUser();
                var res = await client.PostAsJsonAsync(url, new ChangePasswordRequest { OldPassword = "???", NewPassword = "???" });
                res.Should().HaveStatusCode(400)
                    .And.Should().HaveCommonBody()
                    .Which.Code.Should().Be(UserController.ErrorCodes.ChangePassword_BadOldPassword);
            }

            [Fact]
            public async Task Success()
            {
                using var client = await _factory.CreateClientAsUser();
                const string newPassword = "new";
                var res = await client.PostAsJsonAsync(url,
                    new ChangePasswordRequest { OldPassword = MockUser.User.Password, NewPassword = newPassword });
                res.Should().HaveStatusCode(200);
                await client.CreateUserTokenAsync(MockUser.User.Username, newPassword);
            }
        }
    }
}
