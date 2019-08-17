using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Timeline.Controllers;
using Timeline.Models;
using Timeline.Models.Http;
using Timeline.Tests.Helpers;
using Timeline.Tests.Helpers.Authentication;
using Timeline.Tests.Mock.Data;
using Xunit;
using Xunit.Abstractions;

namespace Timeline.Tests
{
    public class UserUnitTest : IClassFixture<MyWebApplicationFactory<Startup>>, IDisposable
    {
        private readonly WebApplicationFactory<Startup> _factory;
        private readonly Action _disposeAction;

        public UserUnitTest(MyWebApplicationFactory<Startup> factory, ITestOutputHelper outputHelper)
        {
            _factory = factory.WithTestConfig(outputHelper, out _disposeAction);
        }

        public void Dispose()
        {
            _disposeAction();
        }

        [Fact]
        public async Task Get_Users_List()
        {
            using (var client = await _factory.CreateClientAsAdmin())
            {
                var res = await client.GetAsync("users");
                res.Should().HaveStatusCodeOk().And.Should().HaveBodyAsJson<UserInfo[]>()
                    .Which.Should().BeEquivalentTo(MockUsers.UserInfos);
            }
        }

        [Fact]
        public async Task Get_Users_User()
        {
            using (var client = await _factory.CreateClientAsAdmin())
            {
                var res = await client.GetAsync("users/" + MockUsers.UserUsername);
                res.Should().HaveStatusCodeOk()
                    .And.Should().HaveBodyAsJson<UserInfo>()
                    .Which.Should().BeEquivalentTo(MockUsers.UserUserInfo);
            }
        }

        [Fact]
        public async Task Get_Users_404()
        {
            using (var client = await _factory.CreateClientAsAdmin())
            {
                var res = await client.GetAsync("users/usernotexist");
                res.Should().HaveStatusCodeNotFound()
                    .And.Should().HaveBodyAsCommonResponseWithCode(UserController.ErrorCodes.Get_NotExist);
            }
        }

        [Fact]
        public async Task Put_InvalidModel()
        {
            using (var client = await _factory.CreateClientAsAdmin())
            {
                const string url = "users/aaaaaaaa";
                // missing password
                await InvalidModelTestHelpers.TestPutInvalidModel(client, url, new UserPutRequest { Password = null, Administrator = false });
                // missing administrator
                await InvalidModelTestHelpers.TestPutInvalidModel(client, url, new UserPutRequest { Password = "???", Administrator = null });
            }
        }

        [Fact]
        public async Task Put_BadUsername()
        {
            using (var client = await _factory.CreateClientAsAdmin())
            {
                var res = await client.PutAsJsonAsync("users/dsf fddf", new UserPutRequest
                {
                    Password = "???",
                    Administrator = false
                });
                res.Should().HaveStatusCodeBadRequest()
                    .And.Should().HaveBodyAsCommonResponseWithCode(UserController.ErrorCodes.Put_BadUsername);
            }
        }

        private async Task CheckAdministrator(HttpClient client, string username, bool administrator)
        {
            var res = await client.GetAsync("users/" + username);
            res.Should().HaveStatusCodeOk()
                .And.Should().HaveBodyAsJson<UserInfo>()
                .Which.Administrator.Should().Be(administrator);
        }

        [Fact]
        public async Task Put_Modiefied()
        {
            using (var client = await _factory.CreateClientAsAdmin())
            {
                var res = await client.PutAsJsonAsync("users/" + MockUsers.UserUsername, new UserPutRequest
                {
                    Password = "password",
                    Administrator = false
                });
                res.Should().BePutModified();
                await CheckAdministrator(client, MockUsers.UserUsername, false);
            }
        }

        [Fact]
        public async Task Put_Created()
        {
            using (var client = await _factory.CreateClientAsAdmin())
            {
                const string username = "puttest";
                const string url = "users/" + username;

                var res = await client.PutAsJsonAsync(url, new UserPutRequest
                {
                    Password = "password",
                    Administrator = false
                });
                res.Should().BePutCreated();
                await CheckAdministrator(client, username, false);
            }
        }

        [Fact]
        public async Task Patch_NotExist()
        {
            using (var client = await _factory.CreateClientAsAdmin())
            {
                var res = await client.PatchAsJsonAsync("users/usernotexist", new UserPatchRequest { });
                res.Should().HaveStatusCodeNotFound()
                    .And.Should().HaveBodyAsCommonResponseWithCode(UserController.ErrorCodes.Patch_NotExist);
            }
        }

        [Fact]
        public async Task Patch_Success()
        {
            using (var client = await _factory.CreateClientAsAdmin())
            {
                {
                    var res = await client.PatchAsJsonAsync("users/" + MockUsers.UserUsername,
                        new UserPatchRequest { Administrator = false });
                    res.Should().HaveStatusCodeOk();
                    await CheckAdministrator(client, MockUsers.UserUsername, false);
                }
            }
        }

        [Fact]
        public async Task Delete_Deleted()
        {
            using (var client = await _factory.CreateClientAsAdmin())
            {
                {
                    var url = "users/" + MockUsers.UserUsername;
                    var res = await client.DeleteAsync(url);
                    res.Should().BeDeleteDeleted();

                    var res2 = await client.GetAsync(url);
                    res2.Should().HaveStatusCodeNotFound();
                }
            }
        }

        [Fact]
        public async Task Delete_NotExist()
        {
            using (var client = await _factory.CreateClientAsAdmin())
            {
                {
                    var res = await client.DeleteAsync("users/usernotexist");
                    res.Should().BeDeleteNotExist();
                }
            }
        }

        public class ChangePasswordUnitTest : IClassFixture<MyWebApplicationFactory<Startup>>, IDisposable
        {
            private const string url = "userop/changepassword";

            private readonly WebApplicationFactory<Startup> _factory;
            private readonly Action _disposeAction;

            public ChangePasswordUnitTest(MyWebApplicationFactory<Startup> factory, ITestOutputHelper outputHelper)
            {
                _factory = factory.WithTestConfig(outputHelper, out _disposeAction);
            }

            public void Dispose()
            {
                _disposeAction();
            }


            [Fact]
            public async Task InvalidModel()
            {
                using (var client = await _factory.CreateClientAsUser())
                {
                    // missing old password
                    await InvalidModelTestHelpers.TestPostInvalidModel(client, url,
                        new ChangePasswordRequest { OldPassword = null, NewPassword = "???" });
                    // missing new password
                    await InvalidModelTestHelpers.TestPostInvalidModel(client, url,
                        new ChangePasswordRequest { OldPassword = "???", NewPassword = null });
                }
            }

            [Fact]
            public async Task BadOldPassword()
            {
                using (var client = await _factory.CreateClientAsUser())
                {
                    var res = await client.PostAsJsonAsync(url, new ChangePasswordRequest { OldPassword = "???", NewPassword = "???" });
                    res.Should().HaveStatusCodeBadRequest()
                        .And.Should().HaveBodyAsCommonResponseWithCode(UserController.ErrorCodes.ChangePassword_BadOldPassword);
                }
            }

            [Fact]
            public async Task Success()
            {
                using (var client = await _factory.CreateClientAsUser())
                {
                    const string newPassword = "new";
                    var res = await client.PostAsJsonAsync(url,
                        new ChangePasswordRequest { OldPassword = MockUsers.UserPassword, NewPassword = newPassword });
                    res.Should().HaveStatusCodeOk();
                    await client.CreateUserTokenAsync(MockUsers.UserUsername, newPassword);
                }
            }
        }
    }
}
