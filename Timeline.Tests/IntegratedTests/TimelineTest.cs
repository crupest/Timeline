﻿using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Timeline.Models.Http;
using Timeline.Tests.Helpers;
using Xunit;

namespace Timeline.Tests.IntegratedTests
{
    public class TimelineTest : IntegratedTestBase
    {
        public TimelineTest(WebApplicationFactory<Startup> factory)
            : base(factory, 3)
        {

        }

        private List<TimelineInfo> _testTimelines;

        private async Task CreateTestTimelines()
        {
            _testTimelines = new List<TimelineInfo>();
            for (int i = 0; i <= 3; i++)
            {
                var client = await CreateClientAs(i);
                var res = await client.PostAsJsonAsync("timelines", new TimelineCreateRequest { Name = $"t{i}" });
                var timelineInfo = res.Should().HaveStatusCode(200)
                    .And.HaveJsonBody<TimelineInfo>().Which;
                _testTimelines.Add(timelineInfo);
            }
        }

        [Fact]
        public async Task TimelineList()
        {
            await CreateTestTimelines();

            TimelineInfo user1Timeline;

            var client = await CreateDefaultClient();

            {
                var res = await client.GetAsync("/users/user1/timeline");
                user1Timeline = res.Should().HaveStatusCode(200)
                    .And.HaveJsonBody<TimelineInfo>().Which;
            }

            {
                var testResult = new List<TimelineInfo>();
                testResult.Add(user1Timeline);
                testResult.AddRange(_testTimelines);

                var res = await client.GetAsync("/timelines");
                res.Should().HaveStatusCode(200)
                    .And.HaveJsonBody<List<TimelineInfo>>()
                    .Which.Should().BeEquivalentTo(testResult);
            }
        }

        [Fact]
        public async Task TimelineList_WithRelate()
        {
            await CreateTestTimelines();

            var testResultOwn = new List<TimelineInfo>();
            var testResultJoin = new List<TimelineInfo>();
            var testResultAll = new List<TimelineInfo>();

            {
                var client = await CreateClientAsUser();

                {
                    var res = await client.PutAsync("/users/user1/timeline/members/user2", null);
                    res.Should().HaveStatusCode(200);
                }

                {
                    var res = await client.PutAsync("/timelines/t1/members/user2", null);
                    res.Should().HaveStatusCode(200);
                }

                {
                    var res = await client.GetAsync("/users/user1/timeline");
                    var timeline = res.Should().HaveStatusCode(200)
                        .And.HaveJsonBody<TimelineInfo>().Which;
                    testResultAll.Add(timeline);
                    testResultJoin.Add(timeline);
                }

                {
                    var res = await client.GetAsync("/timelines/t1");
                    var timeline = res.Should().HaveStatusCode(200)
                        .And.HaveJsonBody<TimelineInfo>().Which;
                    testResultAll.Add(timeline);
                    testResultJoin.Add(timeline);
                }
            }

            testResultAll.Add(_testTimelines[2]);
            testResultOwn.Add(_testTimelines[2]);

            {
                var client = await CreateClientAs(2);
                var res = await client.GetAsync("/timelines?relate=user2");
                res.Should().HaveStatusCode(200)
                    .And.HaveJsonBody<List<TimelineInfo>>()
                    .Which.Should().BeEquivalentTo(testResultAll);
            }

            {
                var client = await CreateClientAs(2);
                var res = await client.GetAsync("/timelines?relate=user2&relateType=own");
                res.Should().HaveStatusCode(200)
                    .And.HaveJsonBody<List<TimelineInfo>>()
                    .Which.Should().BeEquivalentTo(testResultOwn);
            }

            {
                var client = await CreateClientAs(2);
                var res = await client.GetAsync("/timelines?relate=user2&relateType=join");
                res.Should().HaveStatusCode(200)
                    .And.HaveJsonBody<List<TimelineInfo>>()
                    .Which.Should().BeEquivalentTo(testResultJoin);
            }
        }

        [Fact]
        public async Task TimelineList_InvalidModel()
        {
            var client = await CreateClientAsUser();

            {
                var res = await client.GetAsync("/timelines?relate=us!!");
                res.Should().BeInvalidModel();
            }

            {
                var res = await client.GetAsync("/timelines?relateType=aaa");
                res.Should().BeInvalidModel();
            }
        }

        [Fact]
        public async Task TimelineCreate_Should_Work()
        {
            {
                using var client = await CreateDefaultClient();
                var res = await client.PostAsJsonAsync("timelines", new TimelineCreateRequest { Name = "aaa" });
                res.Should().HaveStatusCode(HttpStatusCode.Unauthorized);
            }

            using (var client = await CreateClientAsUser())
            {
                {
                    var res = await client.PostAsJsonAsync("timelines", new TimelineCreateRequest { Name = "!!!" });
                    res.Should().BeInvalidModel();
                }

                TimelineInfo timelineInfo;
                {
                    var res = await client.PostAsJsonAsync("timelines", new TimelineCreateRequest { Name = "aaa" });
                    timelineInfo = res.Should().HaveStatusCode(200)
                        .And.HaveJsonBody<TimelineInfo>().Which;
                }

                {
                    var res = await client.GetAsync("timelines/aaa");
                    res.Should().HaveStatusCode(200)
                        .And.HaveJsonBody<TimelineInfo>()
                        .Which.Should().BeEquivalentTo(timelineInfo);
                }

                {
                    var res = await client.PostAsJsonAsync("timelines", new TimelineCreateRequest { Name = "aaa" });
                    res.Should().HaveStatusCode(400)
                        .And.HaveCommonBody(ErrorCodes.TimelineCommon.NameConflict);
                }
            }
        }

        [Fact]
        public async Task InvalidModel_BadName()
        {
            using var client = await CreateClientAsAdministrator();
            {
                var res = await client.GetAsync("timelines/aaa!!!");
                res.Should().BeInvalidModel();
            }
            {
                var res = await client.PatchAsJsonAsync("timelines/aaa!!!", new TimelinePatchRequest { });
                res.Should().BeInvalidModel();
            }
            {
                var res = await client.PutAsync("timelines/aaa!!!/members/user1", null);
                res.Should().BeInvalidModel();
            }
            {
                var res = await client.DeleteAsync("timelines/aaa!!!/members/user1");
                res.Should().BeInvalidModel();
            }
            {
                var res = await client.GetAsync("timelines/aaa!!!/posts");
                res.Should().BeInvalidModel();
            }
            {
                var res = await client.PostAsJsonAsync("timelines/aaa!!!/posts", new TimelinePostCreateRequest { Content = "aaa" });
                res.Should().BeInvalidModel();
            }
            {
                var res = await client.DeleteAsync("timelines/aaa!!!/posts/123");
                res.Should().BeInvalidModel();
            }
        }

        [Fact]
        public async Task NotFound()
        {
            using var client = await CreateClientAsAdministrator();
            {
                var res = await client.GetAsync("timelines/notexist");
                res.Should().HaveStatusCode(404).And.HaveCommonBody(ErrorCodes.TimelineCommon.NotExist);
            }
            {
                var res = await client.PatchAsJsonAsync("timelines/notexist", new TimelinePatchRequest { });
                res.Should().HaveStatusCode(404).And.HaveCommonBody(ErrorCodes.TimelineCommon.NotExist);
            }
            {
                var res = await client.PutAsync("timelines/notexist/members/user1", null);
                res.Should().HaveStatusCode(404).And.HaveCommonBody(ErrorCodes.TimelineCommon.NotExist);
            }
            {
                var res = await client.DeleteAsync("timelines/notexist/members/user1");
                res.Should().HaveStatusCode(404).And.HaveCommonBody(ErrorCodes.TimelineCommon.NotExist);
            }
            {
                var res = await client.GetAsync("timelines/notexist/posts");
                res.Should().HaveStatusCode(404).And.HaveCommonBody(ErrorCodes.TimelineCommon.NotExist);
            }
            {
                var res = await client.PostAsJsonAsync("timelines/notexist/posts", new TimelinePostCreateRequest { Content = "aaa" });
                res.Should().HaveStatusCode(404).And.HaveCommonBody(ErrorCodes.TimelineCommon.NotExist);
            }
            {
                var res = await client.DeleteAsync("timelines/notexist/posts/123");
                res.Should().HaveStatusCode(404).And.HaveCommonBody(ErrorCodes.TimelineCommon.NotExist);
            }
        }

        [Fact]
        public async Task Description_Should_Work()
        {
            await CreateTestTimelines();

            using var client = await CreateClientAsUser();

            async Task AssertDescription(string description)
            {
                var res = await client.GetAsync("timelines/t1");
                var body = res.Should().HaveStatusCode(200)
                    .And.HaveJsonBody<TimelineInfo>()
                    .Which.Description.Should().Be(description);
            }

            const string mockDescription = "haha";

            await AssertDescription("");
            {
                var res = await client.PatchAsJsonAsync("timelines/t1",
                    new TimelinePatchRequest { Description = mockDescription });
                res.Should().HaveStatusCode(200)
                    .And.HaveJsonBody<TimelineInfo>().Which.Description.Should().Be(mockDescription);
                await AssertDescription(mockDescription);
            }
            {
                var res = await client.PatchAsJsonAsync("timelines/t1",
                    new TimelinePatchRequest { Description = null });
                res.Should().HaveStatusCode(200)
                    .And.HaveJsonBody<TimelineInfo>().Which.Description.Should().Be(mockDescription);
                await AssertDescription(mockDescription);
            }
            {
                var res = await client.PatchAsJsonAsync("timelines/t1",
                    new TimelinePatchRequest { Description = "" });
                res.Should().HaveStatusCode(200)
                    .And.HaveJsonBody<TimelineInfo>().Which.Description.Should().Be("");
                await AssertDescription("");
            }
        }

        [Fact]
        public async Task Member_Should_Work()
        {
            await CreateTestTimelines();

            const string getUrl = "timelines/t1";
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
                var res = await client.PutAsync("/timelines/t1/members/usernotexist", null);
                res.Should().HaveStatusCode(400)
                    .And.HaveCommonBody(ErrorCodes.TimelineCommon.MemberPut_NotExist);
            }
            await AssertEmptyMembers();
            {
                var res = await client.PutAsync("/timelines/t1/members/user2", null);
                res.Should().HaveStatusCode(200);
            }
            await AssertMembers(new List<UserInfo> { UserInfos[2] });
            {
                var res = await client.DeleteAsync("/timelines/t1/members/user2");
                res.Should().BeDelete(true);
            }
            await AssertEmptyMembers();
            {
                var res = await client.DeleteAsync("/timelines/t1/members/users2");
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
            await CreateTestTimelines();

            using var client = await CreateClientAs(userNumber);
            {
                var res = await client.GetAsync("timelines/t1");
                res.Should().HaveStatusCode(get);
            }

            {
                var res = await client.PatchAsJsonAsync("timelines/t1", new TimelinePatchRequest { Description = "hahaha" });
                res.Should().HaveStatusCode(opPatchUser);
            }

            {
                var res = await client.PatchAsJsonAsync("timelines/t0", new TimelinePatchRequest { Description = "hahaha" });
                res.Should().HaveStatusCode(opPatchAdmin);
            }

            {
                var res = await client.PutAsync("timelines/t1/members/user2", null);
                res.Should().HaveStatusCode(opMemberUser);
            }

            {
                var res = await client.DeleteAsync("timelines/t1/members/user2");
                res.Should().HaveStatusCode(opMemberUser);
            }

            {
                var res = await client.PutAsync("timelines/t0/members/user2", null);
                res.Should().HaveStatusCode(opMemberAdmin);
            }

            {
                var res = await client.DeleteAsync("timelines/t0/members/user2");
                res.Should().HaveStatusCode(opMemberAdmin);
            }
        }

        [Fact]
        public async Task Visibility_Test()
        {
            await CreateTestTimelines();

            const string userUrl = "timelines/t1/posts";
            const string adminUrl = "timelines/t0/posts";
            {

                using var client = await CreateClientAsUser();
                using var content = new StringContent(@"{""visibility"":""abcdefg""}", System.Text.Encoding.UTF8, System.Net.Mime.MediaTypeNames.Application.Json);
                var res = await client.PatchAsync("timelines/t1", content);
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
                    var res = await client.PatchAsJsonAsync("timelines/t1",
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
                        var res = await client.PatchAsJsonAsync("timelines/t1",
                        new TimelinePatchRequest { Visibility = TimelineVisibility.Private });
                        res.Should().HaveStatusCode(200);
                    }
                    {
                        var res = await client.PatchAsJsonAsync("timelines/t0",
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
                    var res = await client.PutAsync("/timelines/t0/members/user1", null);
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
            await CreateTestTimelines();

            using (var client = await CreateClientAsUser())
            {
                var res = await client.PutAsync("timelines/t1/members/user2", null);
                res.Should().HaveStatusCode(200);
            }

            using (var client = await CreateDefaultClient())
            {
                { // no auth should get 401
                    var res = await client.PostAsJsonAsync("timelines/t1/posts",
                        new TimelinePostCreateRequest { Content = "aaa" });
                    res.Should().HaveStatusCode(401);
                }
            }

            using (var client = await CreateClientAsUser())
            {
                { // post self's
                    var res = await client.PostAsJsonAsync("timelines/t1/posts",
                        new TimelinePostCreateRequest { Content = "aaa" });
                    res.Should().HaveStatusCode(200);
                }
                { // post other not as a member should get 403
                    var res = await client.PostAsJsonAsync("timelines/t0/posts",
                        new TimelinePostCreateRequest { Content = "aaa" });
                    res.Should().HaveStatusCode(403);
                }
            }

            using (var client = await CreateClientAsAdministrator())
            {
                { // post as admin
                    var res = await client.PostAsJsonAsync("timelines/t1/posts",
                        new TimelinePostCreateRequest { Content = "aaa" });
                    res.Should().HaveStatusCode(200);
                }
            }

            using (var client = await CreateClientAs(2))
            {
                { // post as member
                    var res = await client.PostAsJsonAsync("timelines/t1/posts",
                        new TimelinePostCreateRequest { Content = "aaa" });
                    res.Should().HaveStatusCode(200);
                }
            }
        }

        [Fact]
        public async Task Permission_Post_Delete()
        {
            await CreateTestTimelines();

            async Task<long> CreatePost(int userNumber)
            {
                using var client = await CreateClientAs(userNumber);
                var res = await client.PostAsJsonAsync($"timelines/t1/posts",
                    new TimelinePostCreateRequest { Content = "aaa" });
                return res.Should().HaveStatusCode(200)
                    .And.HaveJsonBody<TimelinePostInfo>()
                    .Which.Id;
            }

            using (var client = await CreateClientAsUser())
            {
                {
                    var res = await client.PutAsync("timelines/t1/members/user2", null);
                    res.Should().HaveStatusCode(200);
                }
                {
                    var res = await client.PutAsync("timelines/t1/members/user3", null);
                    res.Should().HaveStatusCode(200);
                }
            }

            { // no auth should get 401
                using var client = await CreateDefaultClient();
                var res = await client.DeleteAsync("timelines/t1/posts/12");
                res.Should().HaveStatusCode(401);
            }

            { // self can delete self
                var postId = await CreatePost(1);
                using var client = await CreateClientAsUser();
                var res = await client.DeleteAsync($"timelines/t1/posts/{postId}");
                res.Should().HaveStatusCode(200);
            }

            { // admin can delete any
                var postId = await CreatePost(1);
                using var client = await CreateClientAsAdministrator();
                var res = await client.DeleteAsync($"timelines/t1/posts/{postId}");
                res.Should().HaveStatusCode(200);
            }

            { // owner can delete other
                var postId = await CreatePost(2);
                using var client = await CreateClientAsUser();
                var res = await client.DeleteAsync($"timelines/t1/posts/{postId}");
                res.Should().HaveStatusCode(200);
            }

            { // author can delete self
                var postId = await CreatePost(2);
                using var client = await CreateClientAs(2);
                var res = await client.DeleteAsync($"timelines/t1/posts/{postId}");
                res.Should().HaveStatusCode(200);
            }

            { // otherwise is forbidden
                var postId = await CreatePost(2);
                using var client = await CreateClientAs(3);
                var res = await client.DeleteAsync($"timelines/t1/posts/{postId}");
                res.Should().HaveStatusCode(403);
            }
        }

        [Fact]
        public async Task Post_Op_Should_Work()
        {
            await CreateTestTimelines();

            {
                using var client = await CreateClientAsUser();
                {
                    var res = await client.GetAsync("timelines/t1/posts");
                    res.Should().HaveStatusCode(200)
                        .And.HaveJsonBody<TimelinePostInfo[]>()
                        .Which.Should().NotBeNull().And.BeEmpty();
                }
                {
                    var res = await client.PostAsJsonAsync("timelines/t1/posts",
                        new TimelinePostCreateRequest { Content = null });
                    res.Should().BeInvalidModel();
                }
                const string mockContent = "aaa";
                TimelinePostInfo createRes;
                {
                    var res = await client.PostAsJsonAsync("timelines/t1/posts",
                        new TimelinePostCreateRequest { Content = mockContent });
                    var body = res.Should().HaveStatusCode(200)
                        .And.HaveJsonBody<TimelinePostInfo>()
                        .Which;
                    body.Should().NotBeNull();
                    body.Content.Should().Be(mockContent);
                    body.Author.Should().BeEquivalentTo(UserInfos[1]);
                    createRes = body;
                }
                {
                    var res = await client.GetAsync("timelines/t1/posts");
                    res.Should().HaveStatusCode(200)
                        .And.HaveJsonBody<TimelinePostInfo[]>()
                        .Which.Should().NotBeNull().And.BeEquivalentTo(createRes);
                }
                const string mockContent2 = "bbb";
                var mockTime2 = DateTime.Now.AddDays(-1);
                TimelinePostInfo createRes2;
                {
                    var res = await client.PostAsJsonAsync("timelines/t1/posts",
                        new TimelinePostCreateRequest { Content = mockContent2, Time = mockTime2 });
                    var body = res.Should().HaveStatusCode(200)
                        .And.HaveJsonBody<TimelinePostInfo>()
                        .Which;
                    body.Should().NotBeNull();
                    body.Content.Should().Be(mockContent2);
                    body.Author.Should().BeEquivalentTo(UserInfos[1]);
                    body.Time.Should().BeCloseTo(mockTime2, 1000);
                    createRes2 = body;
                }
                {
                    var res = await client.GetAsync("timelines/t1/posts");
                    res.Should().HaveStatusCode(200)
                        .And.HaveJsonBody<TimelinePostInfo[]>()
                        .Which.Should().NotBeNull().And.BeEquivalentTo(createRes, createRes2);
                }
                {
                    var res = await client.DeleteAsync($"timelines/t1/posts/{createRes.Id}");
                    res.Should().BeDelete(true);
                }
                {
                    var res = await client.DeleteAsync("timelines/t1/posts/30000");
                    res.Should().BeDelete(false);
                }
                {
                    var res = await client.GetAsync("timelines/t1/posts");
                    res.Should().HaveStatusCode(200)
                        .And.HaveJsonBody<TimelinePostInfo[]>()
                        .Which.Should().NotBeNull().And.BeEquivalentTo(createRes2);
                }
            }
        }

        [Fact]
        public async Task GetPost_Should_Ordered()
        {
            await CreateTestTimelines();

            using var client = await CreateClientAsUser();

            async Task<long> CreatePost(DateTime time)
            {
                var res = await client.PostAsJsonAsync("timelines/t1/posts",
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
                var res = await client.GetAsync("timelines/t1/posts");
                res.Should().HaveStatusCode(200)
                    .And.HaveJsonBody<TimelinePostInfo[]>()
                    .Which.Select(p => p.Id).Should().Equal(id1, id2, id0);
            }
        }
    }
}
