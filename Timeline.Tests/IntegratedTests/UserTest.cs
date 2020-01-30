using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Timeline.Models.Http;
using Timeline.Tests.Helpers;
using Xunit;

namespace Timeline.Tests.IntegratedTests
{
    public class UserTest : IntegratedTestBase
    {
        public UserTest(WebApplicationFactory<Startup> factory)
            : base(factory)
        {

        }

        [Fact]
        public async Task GetList_NoAuth()
        {
            using var client = await CreateDefaultClient();
            var res = await client.GetAsync("/users");
            res.Should().HaveStatusCode(200)
                .And.HaveJsonBody<UserInfo[]>()
                .Which.Should().BeEquivalentTo(UserInfoList);
        }

        [Fact]
        public async Task GetList_User()
        {
            using var client = await CreateClientAsUser();
            var res = await client.GetAsync("/users");
            res.Should().HaveStatusCode(200)
                .And.HaveJsonBody<UserInfo[]>()
                .Which.Should().BeEquivalentTo(UserInfoList);
        }

        [Fact]
        public async Task GetList_Admin()
        {
            using var client = await CreateClientAsAdministrator();
            var res = await client.GetAsync("/users");
            res.Should().HaveStatusCode(200)
                .And.HaveJsonBody<UserInfo[]>()
                .Which.Should().BeEquivalentTo(UserInfoForAdminList);
        }

        [Fact]
        public async Task Get_NoAuth()
        {
            using var client = await CreateDefaultClient();
            var res = await client.GetAsync($"/users/admin");
            res.Should().HaveStatusCode(200)
                .And.HaveJsonBody<UserInfo>()
                .Which.Should().BeEquivalentTo(UserInfoList[0]);
        }

        [Fact]
        public async Task Get_User()
        {
            using var client = await CreateClientAsUser();
            var res = await client.GetAsync($"/users/admin");
            res.Should().HaveStatusCode(200)
                .And.HaveJsonBody<UserInfo>()
                .Which.Should().BeEquivalentTo(UserInfoList[0]);
        }

        [Fact]
        public async Task Get_Admin()
        {
            using var client = await CreateClientAsAdministrator();
            var res = await client.GetAsync($"/users/user1");
            res.Should().HaveStatusCode(200)
                .And.HaveJsonBody<UserInfo>()
                .Which.Should().BeEquivalentTo(UserInfoForAdminList[1]);
        }

        [Fact]
        public async Task Get_InvalidModel()
        {
            using var client = await CreateClientAsUser();
            var res = await client.GetAsync("/users/aaa!a");
            res.Should().BeInvalidModel();
        }

        [Fact]
        public async Task Get_404()
        {
            using var client = await CreateClientAsUser();
            var res = await client.GetAsync("/users/usernotexist");
            res.Should().HaveStatusCode(404)
                .And.HaveCommonBody(ErrorCodes.UserCommon.NotExist);
        }

        [Fact]
        public async Task Patch_User()
        {
            using var client = await CreateClientAsUser();
            {
                var res = await client.PatchAsJsonAsync("/users/user1",
                    new UserPatchRequest { Nickname = "aaa" });
                res.Should().HaveStatusCode(200);
            }

            {
                var res = await client.GetAsync("/users/user1");
                res.Should().HaveStatusCode(200)
                    .And.HaveJsonBody<UserInfo>()
                    .Which.Nickname.Should().Be("aaa");
            }
        }

        [Fact]
        public async Task Patch_Admin()
        {
            using var client = await CreateClientAsAdministrator();
            using var userClient = await CreateClientAsUser();

            {
                var res = await client.PatchAsJsonAsync("/users/user1",
                    new UserPatchRequest
                    {
                        Username = "newuser",
                        Password = "newpw",
                        Administrator = true,
                        Nickname = "aaa"
                    });
                res.Should().HaveStatusCode(200);
            }

            {
                var res = await client.GetAsync("/users/newuser");
                var body = res.Should().HaveStatusCode(200)
                    .And.HaveJsonBody<UserInfoForAdmin>()
                    .Which;
                body.Administrator.Should().Be(true);
                body.Nickname.Should().Be("aaa");
            }

            {
                // Token should expire.
                var res = await userClient.GetAsync("/users");
                res.Should().HaveStatusCode(HttpStatusCode.Unauthorized);
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
            var res = await client.PatchAsJsonAsync("/users/usernotexist", new UserPatchRequest { });
            res.Should().HaveStatusCode(404)
                .And.HaveCommonBody()
                .Which.Code.Should().Be(ErrorCodes.UserCommon.NotExist);
        }

        [Fact]
        public async Task Patch_InvalidModel()
        {
            using var client = await CreateClientAsAdministrator();
            var res = await client.PatchAsJsonAsync("/users/aaa!a", new UserPatchRequest { });
            res.Should().BeInvalidModel();
        }

        public static IEnumerable<object[]> Patch_InvalidModel_Body_Data()
        {
            yield return new[] { new UserPatchRequest { Username = "aaa!a" } };
            yield return new[] { new UserPatchRequest { Password = "" } };
            yield return new[] { new UserPatchRequest { Nickname = new string('a', 50) } };
        }

        [Theory]
        [MemberData(nameof(Patch_InvalidModel_Body_Data))]
        public async Task Patch_InvalidModel_Body(UserPatchRequest body)
        {
            using var client = await CreateClientAsAdministrator();
            var res = await client.PatchAsJsonAsync("/users/user1", body);
            res.Should().BeInvalidModel();
        }

        [Fact]
        public async Task Patch_UsernameConflict()
        {
            using var client = await CreateClientAsAdministrator();
            var res = await client.PatchAsJsonAsync("/users/user1", new UserPatchRequest { Username = "admin" });
            res.Should().HaveStatusCode(400)
                .And.HaveCommonBody(ErrorCodes.UserController.UsernameConflict);
        }

        [Fact]
        public async Task Patch_NoAuth_Unauthorized()
        {
            using var client = await CreateClientAsUser();
            var res = await client.PatchAsJsonAsync("/users/user1", new UserPatchRequest { Nickname = "aaa" });
            res.Should().HaveStatusCode(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Patch_User_Forbid()
        {
            using var client = await CreateClientAsUser();
            var res = await client.PatchAsJsonAsync("/users/admin", new UserPatchRequest { Nickname = "aaa" });
            res.Should().HaveStatusCode(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task Patch_Username_Forbid()
        {
            using var client = await CreateClientAsUser();
            var res = await client.PatchAsJsonAsync("/users/user1", new UserPatchRequest { Username = "aaa" });
            res.Should().HaveStatusCode(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task Patch_Password_Forbid()
        {
            using var client = await CreateClientAsUser();
            var res = await client.PatchAsJsonAsync("/users/user1", new UserPatchRequest { Password = "aaa" });
            res.Should().HaveStatusCode(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task Patch_Administrator_Forbid()
        {
            using var client = await CreateClientAsUser();
            var res = await client.PatchAsJsonAsync("/users/user1", new UserPatchRequest { Administrator = true });
            res.Should().HaveStatusCode(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task Delete_Deleted()
        {
            using var client = await CreateClientAsAdministrator();
            {
                var res = await client.DeleteAsync("/users/user1");
                res.Should().BeDelete(true);
            }

            {
                var res = await client.GetAsync("/users/user1");
                res.Should().HaveStatusCode(404);
            }
        }

        [Fact]
        public async Task Delete_NotExist()
        {
            using var client = await CreateClientAsAdministrator();
            var res = await client.DeleteAsync("/users/usernotexist");
            res.Should().BeDelete(false);
        }

        [Fact]
        public async Task Delete_InvalidModel()
        {
            using var client = await CreateClientAsAdministrator();
            var res = await client.DeleteAsync("/users/aaa!a");
            res.Should().BeInvalidModel();
        }

        [Fact]
        public async Task Delete_NoAuth_Unauthorized()
        {
            using var client = await CreateDefaultClient();
            var res = await client.DeleteAsync("/users/aaa!a");
            res.Should().HaveStatusCode(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Delete_User_Forbid()
        {
            using var client = await CreateClientAsUser();
            var res = await client.DeleteAsync("/users/aaa!a");
            res.Should().HaveStatusCode(HttpStatusCode.Forbidden);
        }

        private const string createUserUrl = "/userop/createuser";

        [Fact]
        public async Task Op_CreateUser()
        {
            using var client = await CreateClientAsAdministrator();
            {
                var res = await client.PostAsJsonAsync(createUserUrl, new CreateUserRequest
                {
                    Username = "aaa",
                    Password = "bbb",
                    Administrator = true,
                    Nickname = "ccc"
                });
                res.Should().HaveStatusCode(200);
            }
            {
                var res = await client.GetAsync("users/aaa");
                var body = res.Should().HaveStatusCode(200)
                    .And.HaveJsonBody<UserInfoForAdmin>().Which;
                body.Username.Should().Be("aaa");
                body.Nickname.Should().Be("ccc");
                body.Administrator.Should().BeTrue();
            }
            {
                // Test password.
                (await CreateClientWithCredential("aaa", "bbb")).Dispose();
            }
        }

        public static IEnumerable<object[]> Op_CreateUser_InvalidModel_Data()
        {
            yield return new[] { new CreateUserRequest { Username = "aaa", Password = "bbb" } };
            yield return new[] { new CreateUserRequest { Username = "aaa", Administrator = true } };
            yield return new[] { new CreateUserRequest { Password = "bbb", Administrator = true } };
            yield return new[] { new CreateUserRequest { Username = "a!a", Password = "bbb", Administrator = true } };
            yield return new[] { new CreateUserRequest { Username = "aaa", Password = "", Administrator = true } };
            yield return new[] { new CreateUserRequest { Username = "aaa", Password = "bbb", Administrator = true, Nickname = new string('a', 40) } };
        }

        [Theory]
        [MemberData(nameof(Op_CreateUser_InvalidModel_Data))]
        public async Task Op_CreateUser_InvalidModel(CreateUserRequest body)
        {
            using var client = await CreateClientAsAdministrator();
            {
                var res = await client.PostAsJsonAsync(createUserUrl, body);
                res.Should().BeInvalidModel();
            }
        }

        [Fact]
        public async Task Op_CreateUser_UsernameConflict()
        {
            using var client = await CreateClientAsAdministrator();
            {
                var res = await client.PostAsJsonAsync(createUserUrl, new CreateUserRequest
                {
                    Username = "user1",
                    Password = "bbb",
                    Administrator = false
                });
                res.Should().HaveStatusCode(400)
                    .And.HaveCommonBody(ErrorCodes.UserController.UsernameConflict);
            }
        }

        [Fact]
        public async Task Op_CreateUser_NoAuth_Unauthorized()
        {
            using var client = await CreateDefaultClient();
            {
                var res = await client.PostAsJsonAsync(createUserUrl, new CreateUserRequest
                {
                    Username = "aaa",
                    Password = "bbb",
                    Administrator = false
                });
                res.Should().HaveStatusCode(HttpStatusCode.Unauthorized);
            }
        }

        [Fact]
        public async Task Op_CreateUser_User_Forbid()
        {
            using var client = await CreateClientAsUser();
            {
                var res = await client.PostAsJsonAsync(createUserUrl, new CreateUserRequest
                {
                    Username = "aaa",
                    Password = "bbb",
                    Administrator = false
                });
                res.Should().HaveStatusCode(HttpStatusCode.Forbidden);
            }
        }

        private const string changePasswordUrl = "/userop/changepassword";

        [Fact]
        public async Task Op_ChangePassword()
        {
            using var client = await CreateClientAsUser();
            {
                var res = await client.PostAsJsonAsync(changePasswordUrl,
                    new ChangePasswordRequest { OldPassword = "user1pw", NewPassword = "newpw" });
                res.Should().HaveStatusCode(200);
            }
            {
                var res = await client.PatchAsJsonAsync("/users/user1", new UserPatchRequest { });
                res.Should().HaveStatusCode(HttpStatusCode.Unauthorized);
            }
            {
                (await CreateClientWithCredential("user1", "newpw")).Dispose();
            }
        }

        public static IEnumerable<object[]> Op_ChangePassword_InvalidModel_Data()
        {
            yield return new[] { null, "ppp" };
            yield return new[] { "ppp", null };
        }

        [Theory]
        [MemberData(nameof(Op_ChangePassword_InvalidModel_Data))]
        public async Task Op_ChangePassword_InvalidModel(string oldPassword, string newPassword)
        {
            using var client = await CreateClientAsUser();
            var res = await client.PostAsJsonAsync(changePasswordUrl,
                new ChangePasswordRequest { OldPassword = oldPassword, NewPassword = newPassword });
            res.Should().BeInvalidModel();
        }

        [Fact]
        public async Task Op_ChangePassword_BadOldPassword()
        {
            using var client = await CreateClientAsUser();
            var res = await client.PostAsJsonAsync(changePasswordUrl, new ChangePasswordRequest { OldPassword = "???", NewPassword = "???" });
            res.Should().HaveStatusCode(400)
                .And.HaveCommonBody(ErrorCodes.UserController.ChangePassword_BadOldPassword);
        }

        [Fact]
        public async Task Op_ChangePassword_NoAuth_Unauthorized()
        {
            using var client = await CreateDefaultClient();
            var res = await client.PostAsJsonAsync(changePasswordUrl, new ChangePasswordRequest { OldPassword = "???", NewPassword = "???" });
            res.Should().HaveStatusCode(HttpStatusCode.Unauthorized);
        }
    }
}
