using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Timeline.Models.Http;
using Timeline.Tests.Helpers;
using Xunit;

namespace Timeline.Tests.IntegratedTests
{
    public class PersonalTimelineTest : IntegratedTestBase
    {
        public PersonalTimelineTest(WebApplicationFactory<Startup> factory)
            : base(factory, 3)
        {

        }

        [Fact]
        public async Task TimelineGet_Should_Work()
        {
            using var client = await CreateDefaultClient();
            var res = await client.GetAsync("users/user1/timeline");
            var body = res.Should().HaveStatusCode(200)
                .And.HaveJsonBody<BaseTimelineInfo>().Which;
            body.Owner.Should().BeEquivalentTo(UserInfoList[1]);
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
                var res = await client.GetAsync("users/user1/timeline");
                var body = res.Should().HaveStatusCode(200)
                    .And.HaveJsonBody<BaseTimelineInfo>()
                    .Which.Description.Should().Be(description);
            }

            const string mockDescription = "haha";

            await AssertDescription("");
            {
                var res = await client.PatchAsJsonAsync("users/user1/timeline",
                    new TimelinePatchRequest { Description = mockDescription });
                res.Should().HaveStatusCode(200);
                await AssertDescription(mockDescription);
            }
            {
                var res = await client.PatchAsJsonAsync("users/user1/timeline",
                    new TimelinePatchRequest { Description = null });
                res.Should().HaveStatusCode(200);
                await AssertDescription(mockDescription);
            }
            {
                var res = await client.PatchAsJsonAsync("users/user1/timeline",
                    new TimelinePatchRequest { Description = "" });
                res.Should().HaveStatusCode(200);
                await AssertDescription("");
            }
        }

        [Fact]
        public async Task Member_Should_Work()
        {
            const string getUrl = "users/user1/timeline";
            using var client = await CreateClientAsUser();

            async Task AssertMembers(IList<UserInfo> members)
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
                var res = await client.PutAsync("/users/user1/timeline/members/usernotexist", null);
                res.Should().HaveStatusCode(400)
                    .And.HaveCommonBody(ErrorCodes.TimelineController.MemberPut_NotExist);
            }
            await AssertEmptyMembers();
            {
                var res = await client.PutAsync("/users/user1/timeline/members/user2", null);
                res.Should().HaveStatusCode(200);
            }
            await AssertMembers(new List<UserInfo> { UserInfoList[2] });
            {
                var res = await client.DeleteAsync("/users/user1/timeline/members/user2");
                res.Should().BeDelete(true);
            }
            await AssertEmptyMembers();
            {
                var res = await client.DeleteAsync("/users/user1/timeline/members/users2");
                res.Should().BeDelete(false);
            }
            await AssertEmptyMembers();
        }

        [Theory]
        [InlineData(-1, 200, 401, 401, 401, 401)]
        [InlineData(1, 200, 200, 403, 200, 403)]
        [InlineData(0, 200, 200, 200, 200, 200)]
        public async Task Permission_Timeline(int userNumber, int get, int opPatchUser, int opPatchAdmin, int opMemberUser, int opMemberAdmin)
        {
            using var client = await CreateClientAs(userNumber);
            {
                var res = await client.GetAsync("users/user1/timeline");
                res.Should().HaveStatusCode(get);
            }

            {
                var res = await client.PatchAsJsonAsync("users/user1/timeline", new TimelinePatchRequest { Description = "hahaha" });
                res.Should().HaveStatusCode(opPatchUser);
            }

            {
                var res = await client.PatchAsJsonAsync("users/admin/timeline", new TimelinePatchRequest { Description = "hahaha" });
                res.Should().HaveStatusCode(opPatchAdmin);
            }

            {
                var res = await client.PutAsync("users/user1/timeline/members/user2", null);
                res.Should().HaveStatusCode(opMemberUser);
            }

            {
                var res = await client.DeleteAsync("users/user1/timeline/members/user2");
                res.Should().HaveStatusCode(opMemberUser);
            }

            {
                var res = await client.PutAsync("users/admin/timeline/members/user2", null);
                res.Should().HaveStatusCode(opMemberAdmin);
            }

            {
                var res = await client.DeleteAsync("users/admin/timeline/members/user2");
                res.Should().HaveStatusCode(opMemberAdmin);
            }
        }

        [Fact]
        public async Task Visibility_Test()
        {
            const string userUrl = "users/user1/timeline/posts";
            const string adminUrl = "users/admin/timeline/posts";
            {
                using var client = await CreateClientAsUser();
                var res = await client.PatchAsync("users/user1/timeline",
                    new StringContent(@"{""visibility"":""abcdefg""}", System.Text.Encoding.UTF8, System.Net.Mime.MediaTypeNames.Application.Json));
                res.Should().BeInvalidModel();
            }
            { // default visibility is registered
                {
                    using var client = await CreateDefaultClient();
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
                    var res = await client.PatchAsJsonAsync("users/user1/timeline",
                        new TimelinePatchRequest { Visibility = TimelineVisibility.Public });
                    res.Should().HaveStatusCode(200);
                }
                {
                    using var client = await CreateDefaultClient();
                    var res = await client.GetAsync(userUrl);
                    res.Should().HaveStatusCode(200);
                }
            }

            { // change visibility to private
                {
                    using var client = await CreateClientAsAdministrator();
                    {
                        var res = await client.PatchAsJsonAsync("users/user1/timeline",
                        new TimelinePatchRequest { Visibility = TimelineVisibility.Private });
                        res.Should().HaveStatusCode(200);
                    }
                    {
                        var res = await client.PatchAsJsonAsync("users/admin/timeline",
                            new TimelinePatchRequest { Visibility = TimelineVisibility.Private });
                        res.Should().HaveStatusCode(200);
                    }
                }
                {
                    using var client = await CreateDefaultClient();
                    var res = await client.GetAsync(userUrl);
                    res.Should().HaveStatusCode(403);
                }
                { // user can't read admin's
                    using var client = await CreateClientAsUser();
                    var res = await client.GetAsync(adminUrl);
                    res.Should().HaveStatusCode(403);
                }
                { // admin can read user's
                    using var client = await CreateClientAsAdministrator();
                    var res = await client.GetAsync(userUrl);
                    res.Should().HaveStatusCode(200);
                }
                { // add member
                    using var client = await CreateClientAsAdministrator();
                    var res = await client.PutAsync("/users/admin/timeline/members/user1", null);
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
            using (var client = await CreateClientAsUser())
            {
                var res = await client.PutAsync("users/user1/timeline/members/user2", null);
                res.Should().HaveStatusCode(200);
            }

            using (var client = await CreateDefaultClient())
            {
                { // no auth should get 401
                    var res = await client.PostAsJsonAsync("users/user1/timeline/posts",
                        new TimelinePostCreateRequest { Content = "aaa" });
                    res.Should().HaveStatusCode(401);
                }
            }

            using (var client = await CreateClientAsUser())
            {
                { // post self's
                    var res = await client.PostAsJsonAsync("users/user1/timeline/posts",
                        new TimelinePostCreateRequest { Content = "aaa" });
                    res.Should().HaveStatusCode(200);
                }
                { // post other not as a member should get 403
                    var res = await client.PostAsJsonAsync("users/admin/timeline/posts",
                        new TimelinePostCreateRequest { Content = "aaa" });
                    res.Should().HaveStatusCode(403);
                }
            }

            using (var client = await CreateClientAsAdministrator())
            {
                { // post as admin
                    var res = await client.PostAsJsonAsync("users/user1/timeline/posts",
                        new TimelinePostCreateRequest { Content = "aaa" });
                    res.Should().HaveStatusCode(200);
                }
            }

            using (var client = await CreateClientAs(2))
            {
                { // post as member
                    var res = await client.PostAsJsonAsync("users/user1/timeline/posts",
                        new TimelinePostCreateRequest { Content = "aaa" });
                    res.Should().HaveStatusCode(200);
                }
            }
        }

        [Fact]
        public async Task Permission_Post_Delete()
        {
            async Task<long> CreatePost(int userNumber)
            {
                using var client = await CreateClientAs(userNumber);
                var res = await client.PostAsJsonAsync($"users/user1/timeline/posts",
                    new TimelinePostCreateRequest { Content = "aaa" });
                return res.Should().HaveStatusCode(200)
                    .And.HaveJsonBody<TimelinePostInfo>()
                    .Which.Id;
            }

            using (var client = await CreateClientAsUser())
            {
                {
                    var res = await client.PutAsync("users/user1/timeline/members/user2", null);
                    res.Should().HaveStatusCode(200);
                }
                {
                    var res = await client.PutAsync("users/user1/timeline/members/user3", null);
                    res.Should().HaveStatusCode(200);
                }
            }

            { // no auth should get 401
                using var client = await CreateDefaultClient();
                var res = await client.DeleteAsync("users/user1/timeline/posts/12");
                res.Should().HaveStatusCode(401);
            }

            { // self can delete self
                var postId = await CreatePost(1);
                using var client = await CreateClientAsUser();
                var res = await client.DeleteAsync($"users/user1/timeline/posts/{postId}");
                res.Should().HaveStatusCode(200);
            }

            { // admin can delete any
                var postId = await CreatePost(1);
                using var client = await CreateClientAsAdministrator();
                var res = await client.DeleteAsync($"users/user1/timeline/posts/{postId}");
                res.Should().HaveStatusCode(200);
            }

            { // owner can delete other
                var postId = await CreatePost(2);
                using var client = await CreateClientAsUser();
                var res = await client.DeleteAsync($"users/user1/timeline/posts/{postId}");
                res.Should().HaveStatusCode(200);
            }

            { // author can delete self
                var postId = await CreatePost(2);
                using var client = await CreateClientAs(2);
                var res = await client.DeleteAsync($"users/user1/timeline/posts/{postId}");
                res.Should().HaveStatusCode(200);
            }

            { // otherwise is forbidden
                var postId = await CreatePost(2);
                using var client = await CreateClientAs(3);
                var res = await client.DeleteAsync($"users/user1/timeline/posts/{postId}");
                res.Should().HaveStatusCode(403);
            }
        }

        [Fact]
        public async Task Post_Op_Should_Work()
        {
            {
                using var client = await CreateClientAsUser();
                {
                    var res = await client.GetAsync("users/user1/timeline/posts");
                    res.Should().HaveStatusCode(200)
                        .And.HaveJsonBody<TimelinePostInfo[]>()
                        .Which.Should().NotBeNull().And.BeEmpty();
                }
                {
                    var res = await client.PostAsJsonAsync("users/user1/timeline/posts",
                        new TimelinePostCreateRequest { Content = null });
                    res.Should().BeInvalidModel();
                }
                const string mockContent = "aaa";
                TimelinePostInfo createRes;
                {
                    var res = await client.PostAsJsonAsync("users/user1/timeline/posts",
                        new TimelinePostCreateRequest { Content = mockContent });
                    var body = res.Should().HaveStatusCode(200)
                        .And.HaveJsonBody<TimelinePostInfo>()
                        .Which;
                    body.Should().NotBeNull();
                    body.Content.Should().Be(mockContent);
                    body.Author.Should().BeEquivalentTo(UserInfoList[1]);
                    createRes = body;
                }
                {
                    var res = await client.GetAsync("users/user1/timeline/posts");
                    res.Should().HaveStatusCode(200)
                        .And.HaveJsonBody<TimelinePostInfo[]>()
                        .Which.Should().NotBeNull().And.BeEquivalentTo(createRes);
                }
                const string mockContent2 = "bbb";
                var mockTime2 = DateTime.Now.AddDays(-1);
                TimelinePostInfo createRes2;
                {
                    var res = await client.PostAsJsonAsync("users/user1/timeline/posts",
                        new TimelinePostCreateRequest { Content = mockContent2, Time = mockTime2 });
                    var body = res.Should().HaveStatusCode(200)
                        .And.HaveJsonBody<TimelinePostInfo>()
                        .Which;
                    body.Should().NotBeNull();
                    body.Content.Should().Be(mockContent2);
                    body.Author.Should().BeEquivalentTo(UserInfoList[1]);
                    body.Time.Should().BeCloseTo(mockTime2, 1000);
                    createRes2 = body;
                }
                {
                    var res = await client.GetAsync("users/user1/timeline/posts");
                    res.Should().HaveStatusCode(200)
                        .And.HaveJsonBody<TimelinePostInfo[]>()
                        .Which.Should().NotBeNull().And.BeEquivalentTo(createRes, createRes2);
                }
                {
                    var res = await client.DeleteAsync($"users/user1/timeline/posts/{createRes.Id}");
                    res.Should().BeDelete(true);
                }
                {
                    var res = await client.DeleteAsync("users/user1/timeline/posts/30000");
                    res.Should().BeDelete(false);
                }
                {
                    var res = await client.GetAsync("users/user1/timeline/posts");
                    res.Should().HaveStatusCode(200)
                        .And.HaveJsonBody<TimelinePostInfo[]>()
                        .Which.Should().NotBeNull().And.BeEquivalentTo(createRes2);
                }
            }
        }

        [Fact]
        public async Task GetPost_Should_Ordered()
        {
            using var client = await CreateClientAsUser();

            async Task<long> CreatePost(DateTime time)
            {
                var res = await client.PostAsJsonAsync("users/user1/timeline/posts",
                    new TimelinePostCreateRequest { Content = "aaa", Time = time });
                return res.Should().HaveStatusCode(200)
                    .And.HaveJsonBody<TimelinePostInfo>()
                    .Which.Id;
            }

            var now = DateTime.Now;
            var id0 = await CreatePost(now.AddDays(1));
            var id1 = await CreatePost(now.AddDays(-1));
            var id2 = await CreatePost(now);

            {
                var res = await client.GetAsync("users/user1/timeline/posts");
                res.Should().HaveStatusCode(200)
                    .And.HaveJsonBody<TimelinePostInfo[]>()
                    .Which.Select(p => p.Id).Should().Equal(id1, id2, id0);
            }
        }
    }
}
