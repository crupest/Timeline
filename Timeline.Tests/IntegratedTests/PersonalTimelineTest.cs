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
            : base(factory, 3)
        {

        }

        [Fact]
        public async Task TimelineGet_Should_Work()
        {
            using var client = await CreateDefaultClient();
            var res = await client.GetAsync("timelines/@user1");
            var body = res.Should().HaveStatusCode(200)
                .And.HaveJsonBody<TimelineInfo>().Which;
            body.Owner.Should().BeEquivalentTo(UserInfos[1]);
            body.Visibility.Should().Be(TimelineVisibility.Register);
            body.Description.Should().Be("");
            body.Members.Should().NotBeNull().And.BeEmpty();
        }

        [Fact]
        public async Task InvalidModel_BadUsername()
        {
            using var client = await CreateClientAsAdministrator();
            {
                var res = await client.GetAsync("timelines/@user!!!");
                res.Should().BeInvalidModel();
            }
            {
                var res = await client.PatchAsJsonAsync("timelines/@user!!!", new TimelinePatchRequest { });
                res.Should().BeInvalidModel();
            }
            {
                var res = await client.PutAsync("timelines/@user!!!/members/user1", null);
                res.Should().BeInvalidModel();
            }
            {
                var res = await client.DeleteAsync("timelines/@user!!!/members/user1");
                res.Should().BeInvalidModel();
            }
            {
                var res = await client.GetAsync("timelines/@user!!!/posts");
                res.Should().BeInvalidModel();
            }
            {
                var res = await client.PostAsJsonAsync("timelines/@user!!!/posts", TimelineHelper.TextPostCreateRequest("aaa"));
                res.Should().BeInvalidModel();
            }
            {
                var res = await client.DeleteAsync("timelines/@user!!!/posts/123");
                res.Should().BeInvalidModel();
            }
        }

        [Fact]
        public async Task NotFound()
        {
            using var client = await CreateClientAsAdministrator();
            {
                var res = await client.GetAsync("timelines/@usernotexist");
                res.Should().HaveStatusCode(404).And.HaveCommonBody(ErrorCodes.UserCommon.NotExist);
            }
            {
                var res = await client.PatchAsJsonAsync("timelines/@usernotexist", new TimelinePatchRequest { });
                res.Should().HaveStatusCode(404).And.HaveCommonBody(ErrorCodes.UserCommon.NotExist);
            }
            {
                var res = await client.PutAsync("timelines/@usernotexist/members/user1", null);
                res.Should().HaveStatusCode(404).And.HaveCommonBody(ErrorCodes.UserCommon.NotExist);
            }
            {
                var res = await client.DeleteAsync("timelines/@usernotexist/members/user1");
                res.Should().HaveStatusCode(404).And.HaveCommonBody(ErrorCodes.UserCommon.NotExist);
            }
            {
                var res = await client.GetAsync("timelines/@usernotexist/posts");
                res.Should().HaveStatusCode(404).And.HaveCommonBody(ErrorCodes.UserCommon.NotExist);
            }
            {
                var res = await client.PostAsJsonAsync("timelines/@usernotexist/posts", TimelineHelper.TextPostCreateRequest("aaa"));
                res.Should().HaveStatusCode(404).And.HaveCommonBody(ErrorCodes.UserCommon.NotExist);
            }
            {
                var res = await client.DeleteAsync("timelines/@usernotexist/posts/123");
                res.Should().HaveStatusCode(404).And.HaveCommonBody(ErrorCodes.UserCommon.NotExist);
            }
        }

        [Fact]
        public async Task Description_Should_Work()
        {
            using var client = await CreateClientAsUser();

            async Task AssertDescription(string description)
            {
                var res = await client.GetAsync("timelines/@user1");
                var body = res.Should().HaveStatusCode(200)
                    .And.HaveJsonBody<TimelineInfo>()
                    .Which.Description.Should().Be(description);
            }

            const string mockDescription = "haha";

            await AssertDescription("");
            {
                var res = await client.PatchAsJsonAsync("timelines/@user1",
                    new TimelinePatchRequest { Description = mockDescription });
                res.Should().HaveStatusCode(200)
                    .And.HaveJsonBody<TimelineInfo>().Which.Description.Should().Be(mockDescription);
                await AssertDescription(mockDescription);
            }
            {
                var res = await client.PatchAsJsonAsync("timelines/@user1",
                    new TimelinePatchRequest { Description = null });
                res.Should().HaveStatusCode(200)
                    .And.HaveJsonBody<TimelineInfo>().Which.Description.Should().Be(mockDescription);
                await AssertDescription(mockDescription);
            }
            {
                var res = await client.PatchAsJsonAsync("timelines/@user1",
                    new TimelinePatchRequest { Description = "" });
                res.Should().HaveStatusCode(200)
                    .And.HaveJsonBody<TimelineInfo>().Which.Description.Should().Be("");
                await AssertDescription("");
            }
        }

        [Fact]
        public async Task Member_Should_Work()
        {
            const string getUrl = "timelines/@user1";
            using var client = await CreateClientAsUser();

            async Task AssertMembers(IList<UserInfo> members)
            {
                var res = await client.GetAsync(getUrl);
                res.Should().HaveStatusCode(200)
                    .And.HaveJsonBody<TimelineInfo>()
                    .Which.Members.Should().NotBeNull().And.BeEquivalentTo(members);
            }

            async Task AssertEmptyMembers()
            {
                var res = await client.GetAsync(getUrl);
                res.Should().HaveStatusCode(200)
                    .And.HaveJsonBody<TimelineInfo>()
                    .Which.Members.Should().NotBeNull().And.BeEmpty();
            }

            await AssertEmptyMembers();
            {
                var res = await client.PutAsync("/timelines/@user1/members/usernotexist", null);
                res.Should().HaveStatusCode(400)
                    .And.HaveCommonBody(ErrorCodes.TimelineCommon.MemberPut_NotExist);
            }
            await AssertEmptyMembers();
            {
                var res = await client.PutAsync("/timelines/@user1/members/user2", null);
                res.Should().HaveStatusCode(200);
            }
            await AssertMembers(new List<UserInfo> { UserInfos[2] });
            {
                var res = await client.DeleteAsync("/timelines/@user1/members/user2");
                res.Should().BeDelete(true);
            }
            await AssertEmptyMembers();
            {
                var res = await client.DeleteAsync("/timelines/@user1/members/users2");
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
                var res = await client.GetAsync("timelines/@user1");
                res.Should().HaveStatusCode(get);
            }

            {
                var res = await client.PatchAsJsonAsync("timelines/@user1", new TimelinePatchRequest { Description = "hahaha" });
                res.Should().HaveStatusCode(opPatchUser);
            }

            {
                var res = await client.PatchAsJsonAsync("timelines/@admin", new TimelinePatchRequest { Description = "hahaha" });
                res.Should().HaveStatusCode(opPatchAdmin);
            }

            {
                var res = await client.PutAsync("timelines/@user1/members/user2", null);
                res.Should().HaveStatusCode(opMemberUser);
            }

            {
                var res = await client.DeleteAsync("timelines/@user1/members/user2");
                res.Should().HaveStatusCode(opMemberUser);
            }

            {
                var res = await client.PutAsync("timelines/@admin/members/user2", null);
                res.Should().HaveStatusCode(opMemberAdmin);
            }

            {
                var res = await client.DeleteAsync("timelines/@admin/members/user2");
                res.Should().HaveStatusCode(opMemberAdmin);
            }
        }

        [Fact]
        public async Task Visibility_Test()
        {
            const string userUrl = "timelines/@user1/posts";
            const string adminUrl = "timelines/@admin/posts";
            {

                using var client = await CreateClientAsUser();
                using var content = new StringContent(@"{""visibility"":""abcdefg""}", System.Text.Encoding.UTF8, System.Net.Mime.MediaTypeNames.Application.Json);
                var res = await client.PatchAsync("timelines/@user1", content);
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
                    var res = await client.PatchAsJsonAsync("timelines/@user1",
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
                        var res = await client.PatchAsJsonAsync("timelines/@user1",
                        new TimelinePatchRequest { Visibility = TimelineVisibility.Private });
                        res.Should().HaveStatusCode(200);
                    }
                    {
                        var res = await client.PatchAsJsonAsync("timelines/@admin",
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
                    var res = await client.PutAsync("/timelines/@admin/members/user1", null);
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
                var res = await client.PutAsync("timelines/@user1/members/user2", null);
                res.Should().HaveStatusCode(200);
            }

            using (var client = await CreateDefaultClient())
            {
                { // no auth should get 401
                    var res = await client.PostAsJsonAsync("timelines/@user1/posts",
                        TimelineHelper.TextPostCreateRequest("aaa"));
                    res.Should().HaveStatusCode(401);
                }
            }

            using (var client = await CreateClientAsUser())
            {
                { // post self's
                    var res = await client.PostAsJsonAsync("timelines/@user1/posts",
                        TimelineHelper.TextPostCreateRequest("aaa"));
                    res.Should().HaveStatusCode(200);
                }
                { // post other not as a member should get 403
                    var res = await client.PostAsJsonAsync("timelines/@admin/posts",
                        TimelineHelper.TextPostCreateRequest("aaa"));
                    res.Should().HaveStatusCode(403);
                }
            }

            using (var client = await CreateClientAsAdministrator())
            {
                { // post as admin
                    var res = await client.PostAsJsonAsync("timelines/@user1/posts",
                        TimelineHelper.TextPostCreateRequest("aaa"));
                    res.Should().HaveStatusCode(200);
                }
            }

            using (var client = await CreateClientAs(2))
            {
                { // post as member
                    var res = await client.PostAsJsonAsync("timelines/@user1/posts",
                        TimelineHelper.TextPostCreateRequest("aaa"));
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
                var res = await client.PostAsJsonAsync($"timelines/@user1/posts",
                    TimelineHelper.TextPostCreateRequest("aaa"));
                return res.Should().HaveStatusCode(200)
                    .And.HaveJsonBody<TimelinePostInfo>()
                    .Which.Id;
            }

            using (var client = await CreateClientAsUser())
            {
                {
                    var res = await client.PutAsync("timelines/@user1/members/user2", null);
                    res.Should().HaveStatusCode(200);
                }
                {
                    var res = await client.PutAsync("timelines/@user1/members/user3", null);
                    res.Should().HaveStatusCode(200);
                }
            }

            { // no auth should get 401
                using var client = await CreateDefaultClient();
                var res = await client.DeleteAsync("timelines/@user1/posts/12");
                res.Should().HaveStatusCode(401);
            }

            { // self can delete self
                var postId = await CreatePost(1);
                using var client = await CreateClientAsUser();
                var res = await client.DeleteAsync($"timelines/@user1/posts/{postId}");
                res.Should().HaveStatusCode(200);
            }

            { // admin can delete any
                var postId = await CreatePost(1);
                using var client = await CreateClientAsAdministrator();
                var res = await client.DeleteAsync($"timelines/@user1/posts/{postId}");
                res.Should().HaveStatusCode(200);
            }

            { // owner can delete other
                var postId = await CreatePost(2);
                using var client = await CreateClientAsUser();
                var res = await client.DeleteAsync($"timelines/@user1/posts/{postId}");
                res.Should().HaveStatusCode(200);
            }

            { // author can delete self
                var postId = await CreatePost(2);
                using var client = await CreateClientAs(2);
                var res = await client.DeleteAsync($"timelines/@user1/posts/{postId}");
                res.Should().HaveStatusCode(200);
            }

            { // otherwise is forbidden
                var postId = await CreatePost(2);
                using var client = await CreateClientAs(3);
                var res = await client.DeleteAsync($"timelines/@user1/posts/{postId}");
                res.Should().HaveStatusCode(403);
            }
        }

        [Fact]
        public async Task Post_Op_Should_Work()
        {
            {
                using var client = await CreateClientAsUser();
                {
                    var res = await client.GetAsync("timelines/@user1/posts");
                    res.Should().HaveStatusCode(200)
                        .And.HaveJsonBody<TimelinePostInfo[]>()
                        .Which.Should().NotBeNull().And.BeEmpty();
                }
                {
                    var res = await client.PostAsJsonAsync("timelines/@user1/posts",
                        TimelineHelper.TextPostCreateRequest(null));
                    res.Should().BeInvalidModel();
                }
                const string mockContent = "aaa";
                TimelinePostInfo createRes;
                {
                    var res = await client.PostAsJsonAsync("timelines/@user1/posts",
                        TimelineHelper.TextPostCreateRequest(mockContent));
                    var body = res.Should().HaveStatusCode(200)
                        .And.HaveJsonBody<TimelinePostInfo>()
                        .Which;
                    body.Should().NotBeNull();
                    body.Content.Should().BeEquivalentTo(TimelineHelper.TextPostContent(mockContent));
                    body.Author.Should().BeEquivalentTo(UserInfos[1]);
                    createRes = body;
                }
                {
                    var res = await client.GetAsync("timelines/@user1/posts");
                    res.Should().HaveStatusCode(200)
                        .And.HaveJsonBody<TimelinePostInfo[]>()
                        .Which.Should().NotBeNull().And.BeEquivalentTo(createRes);
                }
                const string mockContent2 = "bbb";
                var mockTime2 = DateTime.Now.AddDays(-1);
                TimelinePostInfo createRes2;
                {
                    var res = await client.PostAsJsonAsync("timelines/@user1/posts",
                        TimelineHelper.TextPostCreateRequest(mockContent2, mockTime2));
                    var body = res.Should().HaveStatusCode(200)
                        .And.HaveJsonBody<TimelinePostInfo>()
                        .Which;
                    body.Should().NotBeNull();
                    body.Content.Should().BeEquivalentTo(TimelineHelper.TextPostContent(mockContent2));
                    body.Author.Should().BeEquivalentTo(UserInfos[1]);
                    body.Time.Should().BeCloseTo(mockTime2, 1000);
                    createRes2 = body;
                }
                {
                    var res = await client.GetAsync("timelines/@user1/posts");
                    res.Should().HaveStatusCode(200)
                        .And.HaveJsonBody<TimelinePostInfo[]>()
                        .Which.Should().NotBeNull().And.BeEquivalentTo(createRes, createRes2);
                }
                {
                    var res = await client.DeleteAsync($"timelines/@user1/posts/{createRes.Id}");
                    res.Should().BeDelete(true);
                }
                {
                    var res = await client.DeleteAsync("timelines/@user1/posts/30000");
                    res.Should().BeDelete(false);
                }
                {
                    var res = await client.GetAsync("timelines/@user1/posts");
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
                var res = await client.PostAsJsonAsync("timelines/@user1/posts",
                    TimelineHelper.TextPostCreateRequest("aaa", time));
                return res.Should().HaveStatusCode(200)
                    .And.HaveJsonBody<TimelinePostInfo>()
                    .Which.Id;
            }

            var now = DateTime.Now;
            var id0 = await CreatePost(now.AddDays(1));
            var id1 = await CreatePost(now.AddDays(-1));
            var id2 = await CreatePost(now);

            {
                var res = await client.GetAsync("timelines/@user1/posts");
                res.Should().HaveStatusCode(200)
                    .And.HaveJsonBody<TimelinePostInfo[]>()
                    .Which.Select(p => p.Id).Should().Equal(id1, id2, id0);
            }
        }
    }
}
