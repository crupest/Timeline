using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System;
using System.Net;
using System.Threading.Tasks;
using Timeline.Controllers;
using Timeline.Models;
using Timeline.Models.Http;
using Timeline.Tests.Helpers;
using Timeline.Tests.Helpers.Authentication;
using Timeline.Tests.Mock.Data;
using Xunit;
using Xunit.Abstractions;

namespace Timeline.Tests.IntegratedTests
{
    public class UserDetailTest : IClassFixture<MyWebApplicationFactory<Startup>>, IDisposable
    {
        private readonly WebApplicationFactory<Startup> _factory;
        private readonly Action _disposeAction;

        public UserDetailTest(MyWebApplicationFactory<Startup> factory, ITestOutputHelper outputHelper)
        {
            _factory = factory.WithTestConfig(outputHelper, out _disposeAction);
        }

        public void Dispose()
        {
            _disposeAction();
        }

        [Fact]
        public async Task TestAsUser()
        {
            using (var client = await _factory.CreateClientAsUser())
            {
                {
                    var res = await client.GetAsync($"users/usernotexist/nickname");
                    res.Should().HaveStatusCodeNotFound()
                        .And.Should().HaveBodyAsCommonResponseWithCode(UserDetailController.ErrorCodes.GetNickname_UserNotExist);
                }

                {
                    var res = await client.GetAsync($"users/usernotexist/details");
                    res.Should().HaveStatusCodeNotFound()
                        .And.Should().HaveBodyAsCommonResponseWithCode(UserDetailController.ErrorCodes.Get_UserNotExist);
                }

                async Task GetAndTest(UserDetail d)
                {
                    var res = await client.GetAsync($"users/{MockUsers.UserUsername}/details");
                    res.Should().HaveStatusCodeOk()
                        .And.Should().HaveBodyAsJson<UserDetail>()
                        .Which.Should().BeEquivalentTo(d);
                }

                await GetAndTest(new UserDetail());

                {
                    var res = await client.PatchAsJsonAsync($"users/{MockUsers.AdminUsername}/details", new UserDetail());
                    res.Should().HaveStatusCode(HttpStatusCode.Forbidden)
                        .And.Should().HaveBodyAsCommonResponseWithCode(UserDetailController.ErrorCodes.Patch_Forbid);
                }

                {
                    var res = await client.PatchAsJsonAsync($"users/{MockUsers.UserUsername}/details", new UserDetail
                    {
                        Nickname = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa",
                        QQ = "aaaaaaa",
                        Email = "aaaaaa",
                        PhoneNumber = "aaaaaaaa"
                    });
                    var body = res.Should().HaveStatusCode(HttpStatusCode.BadRequest)
                        .And.Should().HaveBodyAsCommonResponse().Which;
                    body.Code.Should().Be(CommonResponse.ErrorCodes.InvalidModel);
                    foreach (var key in new string[] { "nickname", "qq", "email", "phonenumber" })
                    {
                        body.Message.Should().ContainEquivalentOf(key);
                    }
                }


                var detail = new UserDetail
                {
                    Nickname = "aaa",
                    QQ = "1234567",
                    Email = "aaaa@aaa.net",
                    Description = "aaaaaaaaa"
                };

                {
                    var res = await client.PatchAsJsonAsync($"users/{MockUsers.UserUsername}/details", detail);
                    res.Should().HaveStatusCodeOk();
                    await GetAndTest(detail);
                }

                {
                    var res = await client.GetAsync($"users/{MockUsers.UserUsername}/nickname");
                    res.Should().HaveStatusCodeOk().And.Should().HaveBodyAsJson<UserDetail>()
                        .Which.Should().BeEquivalentTo(new UserDetail
                        {
                            Nickname = detail.Nickname
                        });
                }

                var detail2 = new UserDetail
                {
                    QQ = "",
                    PhoneNumber = "12345678910",
                    Description = "bbbbbbbb"
                };

                {
                    var res = await client.PatchAsJsonAsync($"users/{MockUsers.UserUsername}/details", detail2);
                    res.Should().HaveStatusCodeOk();
                    await GetAndTest(new UserDetail
                    {
                        Nickname = detail.Nickname,
                        QQ = null,
                        Email = detail.Email,
                        PhoneNumber = detail2.PhoneNumber,
                        Description = detail2.Description
                    });
                }
            }
        }

        [Fact]
        public async Task TestAsAdmin()
        {
            using (var client = await _factory.CreateClientAsAdmin())
            {
                {
                    var res = await client.PatchAsJsonAsync($"users/{MockUsers.UserUsername}/details", new UserDetail());
                    res.Should().HaveStatusCodeOk();
                }

                {
                    var res = await client.PatchAsJsonAsync($"users/usernotexist/details", new UserDetail());
                    res.Should().HaveStatusCodeNotFound()
                        .And.Should().HaveBodyAsCommonResponseWithCode(UserDetailController.ErrorCodes.Patch_UserNotExist);
                }
            }
        }
    }
}