using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Timeline.Models;
using Timeline.Models.Http;
using Timeline.Tests.Helpers;
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
            using var client = await CreateClientWithNoAuth();
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
            using var client = await CreateClientAsUser();

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
            using var client = await CreateClientAsUser();

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
                    .Which.Code.Should().Be(ErrorCodes.UserCommon.NotExist);
            }
            {
                var res = await client.PostAsJsonAsync(changeUrl,
                    new TimelineMemberChangeRequest { Remove = new List<string> { "admin", "usernotexist" } });
                res.Should().HaveStatusCode(400)
                    .And.HaveCommonBody()
                    .Which.Code.Should().Be(ErrorCodes.UserCommon.NotExist);
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
            using var client = await CreateClientAs(authType);
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
            {
                using var client = await CreateClientAsUser();
                var res = await client.PostAsync("users/user/timeline/op/property",
                    new StringContent(@"{""visibility"":""abcdefg""}", System.Text.Encoding.UTF8, System.Net.Mime.MediaTypeNames.Application.Json));
                res.Should().BeInvalidModel();
            }
            { // default visibility is registered
                {
                    using var client = await CreateClientWithNoAuth();
                    var res = await client.GetAsync(userUrl);
                    res.Should().HaveStatusCode(403);
                }

                {
                    using var client = await CreateClientAsUser();
                    var res = await client.GetAsync(adminUrl);
                    res.Should().HaveStatusCode(200);
                }
            }

            { // change visibility to public
                {
                    using var client = await CreateClientAsUser();
                    var res = await client.PostAsJsonAsync("users/user/timeline/op/property",
                        new TimelinePropertyChangeRequest { Visibility = TimelineVisibility.Public });
                    res.Should().HaveStatusCode(200);
                }
                {
                    using var client = await CreateClientWithNoAuth();
                    var res = await client.GetAsync(userUrl);
                    res.Should().HaveStatusCode(200);
                }
            }

            { // change visibility to private
                {
                    using var client = await CreateClientAsAdmin();
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
                    using var client = await CreateClientWithNoAuth();
                    var res = await client.GetAsync(userUrl);
                    res.Should().HaveStatusCode(403);
                }
                { // user can't read admin's
                    using var client = await CreateClientAsUser();
                    var res = await client.GetAsync(adminUrl);
                    res.Should().HaveStatusCode(403);
                }
                { // admin can read user's
                    using var client = await CreateClientAsAdmin();
                    var res = await client.GetAsync(userUrl);
                    res.Should().HaveStatusCode(200);
                }
                { // add member
                    using var client = await CreateClientAsAdmin();
                    var res = await client.PostAsJsonAsync("users/admin/timeline/op/member",
                        new TimelineMemberChangeRequest { Add = new List<string> { "user" } });
                    res.Should().HaveStatusCode(200);
                }
                { // now user can read admin's
                    using var client = await CreateClientAsUser();
                    var res = await client.GetAsync(adminUrl);
                    res.Should().HaveStatusCode(200);
                }
            }
        }


        [Fact]
        public async Task Permission_Post_Create()
        {
            CreateExtraMockUsers(1);

            using (var client = await CreateClientAsUser())
            {
                var res = await client.PostAsJsonAsync("users/user/timeline/op/member",
                    new TimelineMemberChangeRequest { Add = new List<string> { "user0" } });
                res.Should().HaveStatusCode(200);
            }

            using (var client = await CreateClientWithNoAuth())
            {
                { // no auth should get 401
                    var res = await client.PostAsJsonAsync("users/user/timeline/postop/create",
                        new TimelinePostCreateRequest { Content = "aaa" });
                    res.Should().HaveStatusCode(401);
                }
            }

            using (var client = await CreateClientAsUser())
            {
                { // post self's
                    var res = await client.PostAsJsonAsync("users/user/timeline/postop/create",
                        new TimelinePostCreateRequest { Content = "aaa" });
                    res.Should().HaveStatusCode(200);
                }
                { // post other not as a member should get 403
                    var res = await client.PostAsJsonAsync("users/admin/timeline/postop/create",
                        new TimelinePostCreateRequest { Content = "aaa" });
                    res.Should().HaveStatusCode(403);
                }
            }

            using (var client = await CreateClientAsAdmin())
            {
                { // post as admin
                    var res = await client.PostAsJsonAsync("users/user/timeline/postop/create",
                        new TimelinePostCreateRequest { Content = "aaa" });
                    res.Should().HaveStatusCode(200);
                }
            }

            using (var client = await CreateClientAs(ExtraMockUsers[0]))
            {
                { // post as member
                    var res = await client.PostAsJsonAsync("users/user/timeline/postop/create",
                        new TimelinePostCreateRequest { Content = "aaa" });
                    res.Should().HaveStatusCode(200);
                }
            }
        }

        [Fact]
        public async Task Permission_Post_Delete()
        {
            CreateExtraMockUsers(2);

            async Task<long> CreatePost(MockUser auth, string timeline)
            {
                using var client = await CreateClientAs(auth);
                var res = await client.PostAsJsonAsync($"users/{timeline}/timeline/postop/create",
                    new TimelinePostCreateRequest { Content = "aaa" });
                return res.Should().HaveStatusCode(200)
                    .And.HaveJsonBody<TimelinePostCreateResponse>()
                    .Which.Id;
            }

            using (var client = await CreateClientAsUser())
            {
                var res = await client.PostAsJsonAsync("users/user/timeline/op/member",
                    new TimelineMemberChangeRequest { Add = new List<string> { "user0", "user1" } });
                res.Should().HaveStatusCode(200);
            }

            { // no auth should get 401
                using var client = await CreateClientWithNoAuth();
                var res = await client.PostAsJsonAsync("users/user/timeline/postop/delete",
                    new TimelinePostDeleteRequest { Id = 12 });
                res.Should().HaveStatusCode(401);
            }

            { // self can delete self
                var postId = await CreatePost(MockUser.User, "user");
                using var client = await CreateClientAsUser();
                var res = await client.PostAsJsonAsync("users/user/timeline/postop/delete",
                    new TimelinePostDeleteRequest { Id = postId });
                res.Should().HaveStatusCode(200);
            }

            { // admin can delete any
                var postId = await CreatePost(MockUser.User, "user");
                using var client = await CreateClientAsAdmin();
                var res = await client.PostAsJsonAsync("users/user/timeline/postop/delete",
                    new TimelinePostDeleteRequest { Id = postId });
                res.Should().HaveStatusCode(200);
            }

            { // owner can delete other
                var postId = await CreatePost(ExtraMockUsers[0], "user");
                using var client = await CreateClientAsUser();
                var res = await client.PostAsJsonAsync("users/user/timeline/postop/delete",
                    new TimelinePostDeleteRequest { Id = postId });
                res.Should().HaveStatusCode(200);
            }

            { // author can delete self
                var postId = await CreatePost(ExtraMockUsers[0], "user");
                using var client = await CreateClientAs(ExtraMockUsers[0]);
                var res = await client.PostAsJsonAsync("users/user/timeline/postop/delete",
                    new TimelinePostDeleteRequest { Id = postId });
                res.Should().HaveStatusCode(200);
            }

            { // otherwise is forbidden
                var postId = await CreatePost(ExtraMockUsers[0], "user");
                using var client = await CreateClientAs(ExtraMockUsers[1]);
                var res = await client.PostAsJsonAsync("users/user/timeline/postop/delete",
                    new TimelinePostDeleteRequest { Id = postId });
                res.Should().HaveStatusCode(403);
            }
        }

        [Fact]
        public async Task Post_Op_Should_Work()
        {
            {
                using var client = await CreateClientAsUser();
                {
                    var res = await client.GetAsync("users/user/timeline/posts");
                    res.Should().HaveStatusCode(200)
                        .And.HaveJsonBody<TimelinePostInfo[]>()
                        .Which.Should().NotBeNull().And.BeEmpty();
                }
                {
                    var res = await client.PostAsJsonAsync("users/user/timeline/postop/create",
                        new TimelinePostCreateRequest { Content = null });
                    res.Should().BeInvalidModel();
                }
                const string mockContent = "aaa";
                TimelinePostCreateResponse createRes;
                {
                    var res = await client.PostAsJsonAsync("users/user/timeline/postop/create",
                        new TimelinePostCreateRequest { Content = mockContent });
                    var body = res.Should().HaveStatusCode(200)
                        .And.HaveJsonBody<TimelinePostCreateResponse>()
                        .Which;
                    body.Should().NotBeNull();
                    createRes = body;
                }
                {
                    var res = await client.GetAsync("users/user/timeline/posts");
                    res.Should().HaveStatusCode(200)
                        .And.HaveJsonBody<TimelinePostInfo[]>()
                        .Which.Should().NotBeNull().And.BeEquivalentTo(
                        new TimelinePostInfo
                        {
                            Id = createRes.Id,
                            Author = "user",
                            Content = mockContent,
                            Time = createRes.Time
                        });
                }
                const string mockContent2 = "bbb";
                var mockTime2 = DateTime.Now.AddDays(-1);
                TimelinePostCreateResponse createRes2;
                {
                    var res = await client.PostAsJsonAsync("users/user/timeline/postop/create",
                        new TimelinePostCreateRequest { Content = mockContent2, Time = mockTime2 });
                    var body = res.Should().HaveStatusCode(200)
                        .And.HaveJsonBody<TimelinePostCreateResponse>()
                        .Which;
                    body.Should().NotBeNull();
                    createRes2 = body;
                }
                {
                    var res = await client.GetAsync("users/user/timeline/posts");
                    res.Should().HaveStatusCode(200)
                        .And.HaveJsonBody<TimelinePostInfo[]>()
                        .Which.Should().NotBeNull().And.BeEquivalentTo(
                        new TimelinePostInfo
                        {
                            Id = createRes.Id,
                            Author = "user",
                            Content = mockContent,
                            Time = createRes.Time
                        },
                        new TimelinePostInfo
                        {
                            Id = createRes2.Id,
                            Author = "user",
                            Content = mockContent2,
                            Time = createRes2.Time
                        });
                }
                {
                    var res = await client.PostAsJsonAsync("users/user/timeline/postop/delete",
                        new TimelinePostDeleteRequest { Id = createRes.Id });
                    res.Should().HaveStatusCode(200);
                }
                {
                    var res = await client.PostAsJsonAsync("users/user/timeline/postop/delete",
                        new TimelinePostDeleteRequest { Id = 30000 });
                    res.Should().HaveStatusCode(400)
                        .And.HaveCommonBody()
                        .Which.Code.Should().Be(ErrorCodes.TimelineController.PostOperationDelete_NotExist);
                }
                {
                    var res = await client.GetAsync("users/user/timeline/posts");
                    res.Should().HaveStatusCode(200)
                        .And.HaveJsonBody<TimelinePostInfo[]>()
                        .Which.Should().NotBeNull().And.BeEquivalentTo(
                        new TimelinePostInfo
                        {
                            Id = createRes2.Id,
                            Author = "user",
                            Content = mockContent2,
                            Time = createRes2.Time
                        });
                }
            }
        }

        [Fact]
        public async Task GetPost_Should_Ordered()
        {
            using var client = await CreateClientAsUser();

            async Task<long> CreatePost(DateTime time)
            {
                var res = await client.PostAsJsonAsync("users/user/timeline/postop/create",
                    new TimelinePostCreateRequest { Content = "aaa", Time = time });
                return res.Should().HaveStatusCode(200)
                    .And.HaveJsonBody<TimelinePostCreateResponse>()
                    .Which.Id;
            }

            var now = DateTime.Now;
            var id0 = await CreatePost(now.AddDays(1));
            var id1 = await CreatePost(now.AddDays(-1));
            var id2 = await CreatePost(now);

            {
                var res = await client.GetAsync("users/user/timeline/posts");
                res.Should().HaveStatusCode(200)
                    .And.HaveJsonBody<TimelinePostInfo[]>()
                    .Which.Select(p => p.Id).Should().Equal(id1, id2, id0);
            }
        }
    }
}
