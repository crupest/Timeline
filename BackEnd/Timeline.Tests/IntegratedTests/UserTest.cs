using FluentAssertions;
using System.Collections.Generic;
using System.Threading.Tasks;
using Timeline.Models.Http;
using Xunit;

namespace Timeline.Tests.IntegratedTests
{
    public class UserTest : IntegratedTestBase
    {
        [Fact]
        public async Task UserListShouldHaveUniqueId()
        {
            using var client = await CreateDefaultClient();
            foreach (var user in await client.TestGetAsync<List<HttpUser>>("users"))
            {
                user.UniqueId.Should().NotBeNullOrWhiteSpace();
            }
        }

        [Fact]
        public async Task GetList()
        {
            using var client = await CreateDefaultClient();
            await client.TestGetAsync<List<HttpUser>>("users");
        }

        [Fact]
        public async Task Get()
        {
            using var client = await CreateDefaultClient();
            var user = await client.TestGetAsync<HttpUser>($"users/admin");
            user.Username.Should().Be("admin");
            user.Nickname.Should().Be("administrator");
            user.UniqueId.Should().NotBeNullOrEmpty();
            user.Permissions.Should().NotBeNull();
        }

        [Fact]
        public async Task Get_InvalidModel()
        {
            using var client = await CreateDefaultClient();
            await client.TestGetAssertInvalidModelAsync("users/aaa!a");
        }

        [Fact]
        public async Task Get_404()
        {
            using var client = await CreateDefaultClient();
            await client.TestGetAssertNotFoundAsync("users/usernotexist", errorCode: ErrorCodes.UserCommon.NotExist);
        }

        [Fact]
        public async Task Patch_User()
        {
            using var client = await CreateClientAsUser();
            {
                var body = await client.TestPatchAsync<HttpUser>("users/user1",
                    new HttpUserPatchRequest { Nickname = "aaa" });
                body.Nickname.Should().Be("aaa");
            }

            {
                var body = await client.GetUserAsync("user1");
                body.Nickname.Should().Be("aaa");
            }
        }

        [Fact]
        public async Task Patch_Admin()
        {
            using var client = await CreateClientAsAdministrator();
            using var userClient = await CreateClientAsUser();

            {
                var body = await client.TestPatchAsync<HttpUser>("users/user1",
                    new HttpUserPatchRequest
                    {
                        Username = "newuser",
                        Password = "newpw",
                        Nickname = "aaa"
                    });
                body.Nickname.Should().Be("aaa");
            }

            {
                var body = await client.GetUserAsync("newuser");
                body.Nickname.Should().Be("aaa");
            }

            {
                var token = userClient.DefaultRequestHeaders.Authorization!.Parameter!;
                // Token should expire.
                await userClient.TestPostAssertErrorAsync("token/verify", new HttpVerifyTokenRequest() { Token = token });
            }

            {
                // Check password.
                (await CreateClientWithCredential("newuser", "newpw")).Dispose();
            }
        }

        [Fact]
        public async Task Patch_NotExist()
        {
            using var client = await CreateClientAsAdministrator();
            await client.TestPatchAssertNotFoundAsync("users/usernotexist", new HttpUserPatchRequest { }, errorCode: ErrorCodes.UserCommon.NotExist);
        }

        [Fact]
        public async Task Patch_InvalidModel()
        {
            using var client = await CreateClientAsAdministrator();
            await client.TestPatchAssertInvalidModelAsync("users/aaa!a", new HttpUserPatchRequest { });
        }

        public static IEnumerable<object[]> Patch_InvalidModel_Body_Data()
        {
            yield return new[] { new HttpUserPatchRequest { Username = "aaa!a" } };
            yield return new[] { new HttpUserPatchRequest { Password = "" } };
            yield return new[] { new HttpUserPatchRequest { Nickname = new string('a', 50) } };
        }

        [Theory]
        [MemberData(nameof(Patch_InvalidModel_Body_Data))]
        public async Task Patch_InvalidModel_Body(HttpUserPatchRequest body)
        {
            using var client = await CreateClientAsAdministrator();
            await client.TestPatchAssertInvalidModelAsync("users/user1", body);
        }

        [Fact]
        public async Task Patch_UsernameConflict()
        {
            using var client = await CreateClientAsAdministrator();
            await client.TestPatchAssertErrorAsync("users/user1", new HttpUserPatchRequest { Username = "admin" }, errorCode: ErrorCodes.UserController.UsernameConflict);
        }

        [Fact]
        public async Task Patch_NoAuth_Unauthorized()
        {
            using var client = await CreateDefaultClient();
            await client.TestPatchAssertUnauthorizedAsync("users/user1", new HttpUserPatchRequest { Nickname = "aaa" });
        }

        [Fact]
        public async Task Patch_User_Forbid()
        {
            using var client = await CreateClientAsUser();
            await client.TestPatchAssertForbiddenAsync("users/admin", new HttpUserPatchRequest { Nickname = "aaa" });
        }

        [Fact]
        public async Task Patch_Username_Forbid()
        {
            using var client = await CreateClientAsUser();
            await client.TestPatchAssertForbiddenAsync("users/user1", new HttpUserPatchRequest { Username = "aaa" });
        }

        [Fact]
        public async Task Patch_Password_Forbid()
        {
            using var client = await CreateClientAsUser();
            await client.TestPatchAssertForbiddenAsync("users/user1", new HttpUserPatchRequest { Password = "aaa" });
        }

        [Fact]
        public async Task Delete_Deleted()
        {
            using var client = await CreateClientAsAdministrator();
            await client.TestDeleteAsync("users/user1", true);
            await client.TestGetAssertNotFoundAsync("users/user1");
        }

        [Fact]
        public async Task Delete_NotExist()
        {
            using var client = await CreateClientAsAdministrator();
            await client.TestDeleteAsync("users/usernotexist", false);
        }

        [Fact]
        public async Task DeleteRootUser_Should_Error()
        {
            using var client = await CreateClientAsAdministrator();
            await client.TestDeleteAssertErrorAsync("users/admin", errorCode: ErrorCodes.UserController.Delete_RootUser);
        }

        [Fact]
        public async Task Delete_InvalidModel()
        {
            using var client = await CreateClientAsAdministrator();
            await client.TestDeleteAssertInvalidModelAsync("users/aaa!a");
        }

        [Fact]
        public async Task Delete_NoAuth_Unauthorized()
        {
            using var client = await CreateDefaultClient();
            await client.TestDeleteAssertUnauthorizedAsync("users/aaa!a");
        }

        [Fact]
        public async Task Delete_User_Forbid()
        {
            using var client = await CreateClientAsUser();
            await client.TestDeleteAssertForbiddenAsync("users/aaa!a");
        }

        private const string createUserUrl = "userop/createuser";

        [Fact]
        public async Task Op_CreateUser()
        {
            using var client = await CreateClientAsAdministrator();
            {
                var body = await client.TestPostAsync<HttpUser>(createUserUrl, new HttpCreateUserRequest
                {
                    Username = "aaa",
                    Password = "bbb",
                });
                body.Username.Should().Be("aaa");
            }
            {
                var body = await client.GetUserAsync("aaa");
                body.Username.Should().Be("aaa");
            }
            {
                // Test password.
                (await CreateClientWithCredential("aaa", "bbb")).Dispose();
            }
        }

        public static IEnumerable<object[]> Op_CreateUser_InvalidModel_Data()
        {
            yield return new[] { new HttpCreateUserRequest { Username = "aaa" } };
            yield return new[] { new HttpCreateUserRequest { Password = "bbb" } };
            yield return new[] { new HttpCreateUserRequest { Username = "a!a", Password = "bbb" } };
            yield return new[] { new HttpCreateUserRequest { Username = "aaa", Password = "" } };
        }

        [Theory]
        [MemberData(nameof(Op_CreateUser_InvalidModel_Data))]
        public async Task Op_CreateUser_InvalidModel(HttpCreateUserRequest body)
        {
            using var client = await CreateClientAsAdministrator();
            await client.TestPostAssertInvalidModelAsync(createUserUrl, body);
        }

        [Fact]
        public async Task Op_CreateUser_UsernameConflict()
        {
            using var client = await CreateClientAsAdministrator();
            await client.TestPostAssertErrorAsync(createUserUrl, new HttpCreateUserRequest
            {
                Username = "user1",
                Password = "bbb",
            }, errorCode: ErrorCodes.UserController.UsernameConflict);
        }

        [Fact]
        public async Task Op_CreateUser_NoAuth_Unauthorized()
        {
            using var client = await CreateDefaultClient();
            await client.TestPostAssertUnauthorizedAsync(createUserUrl, new HttpCreateUserRequest
            {
                Username = "aaa",
                Password = "bbb",
            });
        }

        [Fact]
        public async Task Op_CreateUser_User_Forbid()
        {
            using var client = await CreateClientAsUser();
            await client.TestPostAssertForbiddenAsync(createUserUrl, new HttpCreateUserRequest
            {
                Username = "aaa",
                Password = "bbb",
            });
        }

        private const string changePasswordUrl = "userop/changepassword";

        [Fact]
        public async Task Op_ChangePassword()
        {
            using var client = await CreateClientAsUser();
            await client.TestPostAsync(changePasswordUrl, new HttpChangePasswordRequest { OldPassword = "user1pw", NewPassword = "newpw" });
            await client.TestPatchAssertUnauthorizedAsync("users/user1", new HttpUserPatchRequest { });
            (await CreateClientWithCredential("user1", "newpw")).Dispose();
        }

        public static IEnumerable<object?[]> Op_ChangePassword_InvalidModel_Data()
        {
            yield return new[] { null, "ppp" };
            yield return new[] { "ppp", null };
        }

        [Theory]
        [MemberData(nameof(Op_ChangePassword_InvalidModel_Data))]
        public async Task Op_ChangePassword_InvalidModel(string oldPassword, string newPassword)
        {
            using var client = await CreateClientAsUser();
            await client.TestPostAssertInvalidModelAsync(changePasswordUrl,
                new HttpChangePasswordRequest { OldPassword = oldPassword, NewPassword = newPassword });
        }

        [Fact]
        public async Task Op_ChangePassword_BadOldPassword()
        {
            using var client = await CreateClientAsUser();
            await client.TestPostAssertErrorAsync(changePasswordUrl, new HttpChangePasswordRequest { OldPassword = "???", NewPassword = "???" }, errorCode: ErrorCodes.UserController.ChangePassword_BadOldPassword);
        }

        [Fact]
        public async Task Op_ChangePassword_NoAuth_Unauthorized()
        {
            using var client = await CreateDefaultClient();
            await client.TestPostAssertUnauthorizedAsync(changePasswordUrl, new HttpChangePasswordRequest { OldPassword = "???", NewPassword = "???" });
        }
    }
}
