using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Timeline.Models;
using Timeline.Models.Http;
using Timeline.Tests.Helpers;
using Timeline.Tests.Helpers.Authentication;
using Xunit;

namespace Timeline.Tests.IntegratedTests
{
    public class PersonalTimelineTest : IntegratedTestBase
    {
        public PersonalTimelineTest(WebApplicationFactory<Startup> factory)
            : base(factory)
        {

        }

        [Fact]
        public async Task TimelineGet_Should_Work()
        {
            using var client = Factory.CreateDefaultClient();
            var res = await client.GetAsync("users/user/timeline");
            var body = res.Should().HaveStatusCode(200)
                .And.HaveJsonBody<BaseTimelineInfo>().Which;
            body.Owner.Should().Be("user");
            body.Visibility.Should().Be(TimelineVisibility.Register);
            body.Description.Should().Be("");
            body.Members.Should().NotBeNull().And.BeEmpty();
        }

        [Fact]
        public async Task Description_Should_Work()
        {
            using var client = await Factory.CreateClientAsUser();

            async Task AssertDescription(string description)
            {
                var res = await client.GetAsync("users/user/timeline");
                var body = res.Should().HaveStatusCode(200)
                    .And.HaveJsonBody<BaseTimelineInfo>()
                    .Which.Description.Should().Be(description);
            }

            const string mockDescription = "haha";

            await AssertDescription("");
            {
                var res = await client.PostAsJsonAsync("users/user/timeline/op/property",
                    new TimelinePropertyChangeRequest { Description = mockDescription });
                res.Should().HaveStatusCode(200);
                await AssertDescription(mockDescription);
            }
            {
                var res = await client.PostAsJsonAsync("users/user/timeline/op/property",
                    new TimelinePropertyChangeRequest { Description = null });
                res.Should().HaveStatusCode(200);
                await AssertDescription(mockDescription);
            }
            {
                var res = await client.PostAsJsonAsync("users/user/timeline/op/property",
                    new TimelinePropertyChangeRequest { Description = "" });
                res.Should().HaveStatusCode(200);
                await AssertDescription("");
            }
        }

        [Fact]
        public async Task Member_Should_Work()
        {
            const string getUrl = "users/user/timeline";
            const string changeUrl = "users/user/timeline/op/member";
            using var client = await Factory.CreateClientAsUser();

            async Task AssertMembers(IList<string> members)
            {
                var res = await client.GetAsync(getUrl);
                res.Should().HaveStatusCode(200)
                    .And.HaveJsonBody<BaseTimelineInfo>()
                    .Which.Members.Should().NotBeNull().And.BeEquivalentTo(members);
            }

            async Task AssertEmptyMembers()
            {
                var res = await client.GetAsync(getUrl);
                res.Should().HaveStatusCode(200)
                    .And.HaveJsonBody<BaseTimelineInfo>()
                    .Which.Members.Should().NotBeNull().And.BeEmpty();
            }

            await AssertEmptyMembers();
            {
                var res = await client.PostAsJsonAsync(changeUrl,
                    new TimelineMemberChangeRequest { Add = new List<string> { "admin", "usernotexist" } });
                res.Should().HaveStatusCode(400)
                    .And.HaveCommonBody()
                    .Which.Code.Should().Be(ErrorCodes.Http.Timeline.ChangeMemberUserNotExist);
            }
            {
                var res = await client.PostAsJsonAsync(changeUrl,
                    new TimelineMemberChangeRequest { Remove = new List<string> { "admin", "usernotexist" } });
                res.Should().HaveStatusCode(400)
                    .And.HaveCommonBody()
                    .Which.Code.Should().Be(ErrorCodes.Http.Timeline.ChangeMemberUserNotExist);
            }
            {
                var res = await client.PostAsJsonAsync(changeUrl,
                    new TimelineMemberChangeRequest { Add = new List<string> { "admin" }, Remove = new List<string> { "admin" } });
                res.Should().HaveStatusCode(200);
                await AssertEmptyMembers();
            }
            {
                var res = await client.PostAsJsonAsync(changeUrl,
                    new TimelineMemberChangeRequest { Add = new List<string> { "admin" } });
                res.Should().HaveStatusCode(200);
                await AssertMembers(new List<string> { "admin" });
            }
            {
                var res = await client.PostAsJsonAsync(changeUrl,
                    new TimelineMemberChangeRequest { Remove = new List<string> { "admin" } });
                res.Should().HaveStatusCode(200);
                await AssertEmptyMembers();
            }
        }

        [Theory]
        [InlineData(AuthType.None, 200, 401, 401, 401, 401)]
        [InlineData(AuthType.User, 200, 200, 403, 200, 403)]
        [InlineData(AuthType.Admin, 200, 200, 200, 200, 200)]
        public async Task Permission_Timeline(AuthType authType, int get, int opPropertyUser, int opPropertyAdmin, int opMemberUser, int opMemberAdmin)
        {
            using var client = await Factory.CreateClientAs(authType);
            {
                var res = await client.GetAsync("users/user/timeline");
                res.Should().HaveStatusCode(get);
            }

            {
                var res = await client.PostAsJsonAsync("users/user/timeline/op/property",
                    new TimelinePropertyChangeRequest { Description = "hahaha" });
                res.Should().HaveStatusCode(opPropertyUser);
            }

            {
                var res = await client.PostAsJsonAsync("users/admin/timeline/op/property",
                    new TimelinePropertyChangeRequest { Description = "hahaha" });
                res.Should().HaveStatusCode(opPropertyAdmin);
            }

            {
                var res = await client.PostAsJsonAsync("users/user/timeline/op/member",
                    new TimelineMemberChangeRequest { Add = new List<string> { "admin" } });
                res.Should().HaveStatusCode(opMemberUser);
            }

            {
                var res = await client.PostAsJsonAsync("users/admin/timeline/op/member",
                    new TimelineMemberChangeRequest { Add = new List<string> { "user" } });
                res.Should().HaveStatusCode(opMemberAdmin);
            }
        }

        [Fact]
        public async Task Permission_GetPost()
        {
            const string userUrl = "users/user/timeline/posts";
            const string adminUrl = "users/admin/timeline/posts";
            { // default visibility is registered
                {
                    using var client = Factory.CreateDefaultClient();
                    var res = await client.GetAsync(userUrl);
                    res.Should().HaveStatusCode(403);
                }

                {
                    using var client = await Factory.CreateClientAsUser();
                    var res = await client.GetAsync(adminUrl);
                    res.Should().HaveStatusCode(200);
                }
            }

            { // change visibility to public
                {
                    using var client = await Factory.CreateClientAsUser();
                    var res = await client.PostAsJsonAsync("users/user/timeline/op/property",
                        new TimelinePropertyChangeRequest { Visibility = TimelineVisibility.Public });
                    res.Should().HaveStatusCode(200);
                }
                {
                    using var client = Factory.CreateDefaultClient();
                    var res = await client.GetAsync(userUrl);
                    res.Should().HaveStatusCode(200);
                }
            }

            { // change visibility to private
                {
                    using var client = await Factory.CreateClientAsAdmin();
                    {
                        var res = await client.PostAsJsonAsync("users/user/timeline/op/property",
                        new TimelinePropertyChangeRequest { Visibility = TimelineVisibility.Private });
                        res.Should().HaveStatusCode(200);
                    }
                    {
                        var res = await client.PostAsJsonAsync("users/admin/timeline/op/property",
                            new TimelinePropertyChangeRequest { Visibility = TimelineVisibility.Private });
                        res.Should().HaveStatusCode(200);
                    }
                }
                {
                    using var client = Factory.CreateDefaultClient();
                    var res = await client.GetAsync(userUrl);
                    res.Should().HaveStatusCode(403);
                }
                { // user can't read admin's
                    using var client = await Factory.CreateClientAsUser();
                    var res = await client.GetAsync(adminUrl);
                    res.Should().HaveStatusCode(403);
                }
                { // admin can read user's
                    using var client = await Factory.CreateClientAsAdmin();
                    var res = await client.GetAsync(userUrl);
                    res.Should().HaveStatusCode(200);
                }
                { // add member
                    using var client = await Factory.CreateClientAsAdmin();
                    var res = await client.PostAsJsonAsync("users/admin/timeline/op/member",
                        new TimelineMemberChangeRequest { Add = new List<string> { "user" } });
                    res.Should().HaveStatusCode(200);
                }
                { // now user can read admin's
                    using var client = await Factory.CreateClientAsUser();
                    var res = await client.GetAsync(adminUrl);
                    res.Should().HaveStatusCode(200);
                }
            }
        }
    }
}
