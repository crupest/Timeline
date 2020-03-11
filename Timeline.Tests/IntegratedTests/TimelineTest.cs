﻿using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Timeline.Models;
using Timeline.Models.Http;
using Timeline.Tests.Helpers;
using Xunit;

namespace Timeline.Tests.IntegratedTests
{
    public static class TimelineHelper
    {
        public static TimelinePostContentInfo TextPostContent(string text)
        {
            return new TimelinePostContentInfo
            {
                Type = "text",
                Text = text
            };
        }

        public static TimelinePostCreateRequest TextPostCreateRequest(string text, DateTime? time = null)
        {
            return new TimelinePostCreateRequest
            {
                Content = new TimelinePostCreateRequestContent
                {
                    Type = "text",
                    Text = text
                },
                Time = time
            };
        }
    }

    public class TimelineTest : IntegratedTestBase
    {
        public TimelineTest(WebApplicationFactory<Startup> factory)
            : base(factory, 3)
        {
            CreateTestTimelines().Wait();
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

        private static string GeneratePersonalTimelineUrl(int id, string subpath = null)
        {
            return $"timelines/@{(id == 0 ? "admin" : ("user" + id))}{(subpath == null ? "" : ("/" + subpath))}";
        }

        private static string GenerateOrdinaryTimelineUrl(int id, string subpath = null)
        {
            return $"timelines/t{id}{(subpath == null ? "" : ("/" + subpath))}";
        }

        public static IEnumerable<object[]> TimelineUrlGeneratorData()
        {
            yield return new[] { new Func<int, string, string>(GeneratePersonalTimelineUrl) };
            yield return new[] { new Func<int, string, string>(GenerateOrdinaryTimelineUrl) };
        }

        [Fact]
        public async Task Personal_TimelineGet_Should_Work()
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
        public async Task TimelineList()
        {
            TimelineInfo user1Timeline;

            var client = await CreateDefaultClient();

            {
                var res = await client.GetAsync("/timelines/@user1");
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
        public async Task TimelineList_WithQuery()
        {
            var testResultRelate = new List<TimelineInfo>();
            var testResultOwn = new List<TimelineInfo>();
            var testResultJoin = new List<TimelineInfo>();
            var testResultOwnPrivate = new List<TimelineInfo>();
            var testResultRelatePublic = new List<TimelineInfo>();
            var testResultRelateRegister = new List<TimelineInfo>();
            var testResultJoinPrivate = new List<TimelineInfo>();
            var testResultPublic = new List<TimelineInfo>();

            {
                var client = await CreateClientAsUser();

                {
                    var res = await client.PutAsync("/timelines/@user1/members/user3", null);
                    res.Should().HaveStatusCode(200);
                }

                {
                    var res = await client.PutAsync("/timelines/t1/members/user3", null);
                    res.Should().HaveStatusCode(200);
                }

                {
                    var res = await client.PatchAsJsonAsync("/timelines/@user1", new TimelinePatchRequest { Visibility = TimelineVisibility.Public });
                    res.Should().HaveStatusCode(200);
                }

                {
                    var res = await client.PatchAsJsonAsync("/timelines/t1", new TimelinePatchRequest { Visibility = TimelineVisibility.Register });
                    res.Should().HaveStatusCode(200);
                }

                {
                    var res = await client.GetAsync("/timelines/@user1");
                    var timeline = res.Should().HaveStatusCode(200)
                        .And.HaveJsonBody<TimelineInfo>().Which;
                    testResultRelate.Add(timeline);
                    testResultJoin.Add(timeline);
                    testResultRelatePublic.Add(timeline);
                    testResultPublic.Add(timeline);
                }

                {
                    var res = await client.GetAsync("/timelines/t1");
                    var timeline = res.Should().HaveStatusCode(200)
                        .And.HaveJsonBody<TimelineInfo>().Which;
                    testResultRelate.Add(timeline);
                    testResultJoin.Add(timeline);
                    testResultRelateRegister.Add(timeline);
                }
            }

            {
                var client = await CreateClientAs(2);

                {
                    var res = await client.PutAsync("/timelines/@user2/members/user3", null);
                    res.Should().HaveStatusCode(200);
                }

                {
                    var res = await client.PutAsync("/timelines/t2/members/user3", null);
                    res.Should().HaveStatusCode(200);
                }

                {
                    var res = await client.PatchAsJsonAsync("/timelines/@user2", new TimelinePatchRequest { Visibility = TimelineVisibility.Register });
                    res.Should().HaveStatusCode(200);
                }

                {
                    var res = await client.PatchAsJsonAsync("/timelines/t2", new TimelinePatchRequest { Visibility = TimelineVisibility.Private });
                    res.Should().HaveStatusCode(200);
                }

                {
                    var res = await client.GetAsync("/timelines/@user2");
                    var timeline = res.Should().HaveStatusCode(200)
                        .And.HaveJsonBody<TimelineInfo>().Which;
                    testResultRelate.Add(timeline);
                    testResultJoin.Add(timeline);
                    testResultRelateRegister.Add(timeline);
                }

                {
                    var res = await client.GetAsync("/timelines/t2");
                    var timeline = res.Should().HaveStatusCode(200)
                        .And.HaveJsonBody<TimelineInfo>().Which;
                    testResultRelate.Add(timeline);
                    testResultJoin.Add(timeline);
                    testResultJoinPrivate.Add(timeline);
                }
            }

            {
                var client = await CreateClientAs(3);

                {
                    var res = await client.PatchAsJsonAsync("/timelines/@user3", new TimelinePatchRequest { Visibility = TimelineVisibility.Private });
                    res.Should().HaveStatusCode(200);
                }

                {
                    var res = await client.PatchAsJsonAsync("/timelines/t3", new TimelinePatchRequest { Visibility = TimelineVisibility.Register });
                    res.Should().HaveStatusCode(200);
                }

                {
                    var res = await client.GetAsync("/timelines/@user3");
                    var timeline = res.Should().HaveStatusCode(200)
                        .And.HaveJsonBody<TimelineInfo>().Which;
                    testResultRelate.Add(timeline);
                    testResultOwn.Add(timeline);
                    testResultOwnPrivate.Add(timeline);
                }

                {
                    var res = await client.GetAsync("/timelines/t3");
                    var timeline = res.Should().HaveStatusCode(200)
                        .And.HaveJsonBody<TimelineInfo>().Which;
                    testResultRelate.Add(timeline);
                    testResultOwn.Add(timeline);
                    testResultRelateRegister.Add(timeline);
                }
            }

            {
                var client = await CreateClientAs(3);
                {
                    var res = await client.GetAsync("/timelines?relate=user3");
                    var body = res.Should().HaveStatusCode(200)
                        .And.HaveJsonBody<List<TimelineInfo>>()
                        .Which;
                    body.Should().BeEquivalentTo(testResultRelate);
                }

                {
                    var res = await client.GetAsync("/timelines?relate=user3&relateType=own");
                    var body = res.Should().HaveStatusCode(200)
                        .And.HaveJsonBody<List<TimelineInfo>>()
                        .Which;
                    body.Should().BeEquivalentTo(testResultOwn);
                }

                {
                    var res = await client.GetAsync("/timelines?relate=user3&visibility=public");
                    var body = res.Should().HaveStatusCode(200)
                        .And.HaveJsonBody<List<TimelineInfo>>()
                        .Which;
                    body.Should().BeEquivalentTo(testResultRelatePublic);
                }

                {
                    var res = await client.GetAsync("/timelines?relate=user3&visibility=register");
                    var body = res.Should().HaveStatusCode(200)
                        .And.HaveJsonBody<List<TimelineInfo>>()
                        .Which;
                    body.Should().BeEquivalentTo(testResultRelateRegister);
                }

                {
                    var res = await client.GetAsync("/timelines?relate=user3&relateType=join&visibility=private");
                    var body = res.Should().HaveStatusCode(200)
                        .And.HaveJsonBody<List<TimelineInfo>>()
                        .Which;
                    body.Should().BeEquivalentTo(testResultJoinPrivate);
                }

                {
                    var res = await client.GetAsync("/timelines?relate=user3&relateType=own&visibility=private");
                    var body = res.Should().HaveStatusCode(200)
                        .And.HaveJsonBody<List<TimelineInfo>>()
                        .Which;
                    body.Should().BeEquivalentTo(testResultOwnPrivate);
                }
            }

            {
                var client = await CreateDefaultClient();
                {
                    var res = await client.GetAsync("/timelines?visibility=public");
                    var body = res.Should().HaveStatusCode(200)
                        .And.HaveJsonBody<List<TimelineInfo>>()
                        .Which;
                    body.Should().BeEquivalentTo(testResultPublic);
                }
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

            {
                var res = await client.GetAsync("/timelines?visibility=aaa");
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
                        .And.HaveCommonBody(ErrorCodes.TimelineController.NameConflict);
                }
            }
        }

        [Fact]
        public async Task TimelineDelete_Should_Work()
        {
            {
                using var client = await CreateDefaultClient();
                var res = await client.DeleteAsync("timelines/t1");
                res.Should().HaveStatusCode(HttpStatusCode.Unauthorized);
            }

            {
                using var client = await CreateClientAs(2);
                var res = await client.DeleteAsync("timelines/t1");
                res.Should().HaveStatusCode(HttpStatusCode.Forbidden);
            }

            {
                using var client = await CreateClientAsAdministrator();

                {
                    var res = await client.DeleteAsync("timelines/!!!");
                    res.Should().BeInvalidModel();
                }

                {
                    var res = await client.DeleteAsync("timelines/t2");
                    res.Should().BeDelete(true);
                }

                {
                    var res = await client.DeleteAsync("timelines/t2");
                    res.Should().BeDelete(false);
                }
            }

            {
                using var client = await CreateClientAs(1);

                {
                    var res = await client.DeleteAsync("timelines/!!!");
                    res.Should().BeInvalidModel();
                }

                {
                    var res = await client.DeleteAsync("timelines/t1");
                    res.Should().BeDelete(true);
                }

                {
                    var res = await client.DeleteAsync("timelines/t1");
                    res.Should().HaveStatusCode(HttpStatusCode.NotFound);
                }
            }
        }

        [Fact]
        public async Task Ordinary_InvalidModel_BadName()
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
                var res = await client.PostAsJsonAsync("timelines/aaa!!!/posts", TimelineHelper.TextPostCreateRequest("aaa"));
                res.Should().BeInvalidModel();
            }
            {
                var res = await client.DeleteAsync("timelines/aaa!!!/posts/123");
                res.Should().BeInvalidModel();
            }
        }

        [Fact]
        public async Task Personal_InvalidModel_BadUsername()
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
        public async Task Ordinary_NotFound()
        {
            using var client = await CreateClientAsAdministrator();
            {
                var res = await client.GetAsync("timelines/notexist");
                res.Should().HaveStatusCode(404).And.HaveCommonBody(ErrorCodes.TimelineController.NotExist);
            }
            {
                var res = await client.PatchAsJsonAsync("timelines/notexist", new TimelinePatchRequest { });
                res.Should().HaveStatusCode(404).And.HaveCommonBody(ErrorCodes.TimelineController.NotExist);
            }
            {
                var res = await client.PutAsync("timelines/notexist/members/user1", null);
                res.Should().HaveStatusCode(404).And.HaveCommonBody(ErrorCodes.TimelineController.NotExist);
            }
            {
                var res = await client.DeleteAsync("timelines/notexist/members/user1");
                res.Should().HaveStatusCode(404).And.HaveCommonBody(ErrorCodes.TimelineController.NotExist);
            }
            {
                var res = await client.GetAsync("timelines/notexist/posts");
                res.Should().HaveStatusCode(404).And.HaveCommonBody(ErrorCodes.TimelineController.NotExist);
            }
            {
                var res = await client.PostAsJsonAsync("timelines/notexist/posts", TimelineHelper.TextPostCreateRequest("aaa"));
                res.Should().HaveStatusCode(404).And.HaveCommonBody(ErrorCodes.TimelineController.NotExist);
            }
            {
                var res = await client.DeleteAsync("timelines/notexist/posts/123");
                res.Should().HaveStatusCode(404).And.HaveCommonBody(ErrorCodes.TimelineController.NotExist);
            }
        }


        [Fact]
        public async Task PersonalNotFound()
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

        [Theory]
        [MemberData(nameof(TimelineUrlGeneratorData))]
        public async Task Description_Should_Work(Func<int, string, string> generator)
        {
            using var client = await CreateClientAsUser();

            async Task AssertDescription(string description)
            {
                var res = await client.GetAsync(generator(1, null));
                var body = res.Should().HaveStatusCode(200)
                    .And.HaveJsonBody<TimelineInfo>()
                    .Which.Description.Should().Be(description);
            }

            const string mockDescription = "haha";

            await AssertDescription("");
            {
                var res = await client.PatchAsJsonAsync(generator(1, null),
                    new TimelinePatchRequest { Description = mockDescription });
                res.Should().HaveStatusCode(200)
                    .And.HaveJsonBody<TimelineInfo>().Which.Description.Should().Be(mockDescription);
                await AssertDescription(mockDescription);
            }
            {
                var res = await client.PatchAsJsonAsync(generator(1, null),
                    new TimelinePatchRequest { Description = null });
                res.Should().HaveStatusCode(200)
                    .And.HaveJsonBody<TimelineInfo>().Which.Description.Should().Be(mockDescription);
                await AssertDescription(mockDescription);
            }
            {
                var res = await client.PatchAsJsonAsync(generator(1, null),
                    new TimelinePatchRequest { Description = "" });
                res.Should().HaveStatusCode(200)
                    .And.HaveJsonBody<TimelineInfo>().Which.Description.Should().Be("");
                await AssertDescription("");
            }
        }

        [Theory]
        [MemberData(nameof(TimelineUrlGeneratorData))]
        public async Task Member_Should_Work(Func<int, string, string> generator)
        {
            var getUrl = generator(1, null);
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
                var res = await client.PutAsync(generator(1, "members/usernotexist"), null);
                res.Should().HaveStatusCode(400)
                    .And.HaveCommonBody(ErrorCodes.TimelineController.MemberPut_NotExist);
            }
            await AssertEmptyMembers();
            {
                var res = await client.PutAsync(generator(1, "members/user2"), null);
                res.Should().HaveStatusCode(200);
            }
            await AssertMembers(new List<UserInfo> { UserInfos[2] });
            {
                var res = await client.DeleteAsync(generator(1, "members/user2"));
                res.Should().BeDelete(true);
            }
            await AssertEmptyMembers();
            {
                var res = await client.DeleteAsync(generator(1, "members/aaa"));
                res.Should().BeDelete(false);
            }
            await AssertEmptyMembers();
        }

        [Theory]
        [InlineData(nameof(GenerateOrdinaryTimelineUrl), -1, 200, 401, 401, 401, 401)]
        [InlineData(nameof(GenerateOrdinaryTimelineUrl), 1, 200, 200, 403, 200, 403)]
        [InlineData(nameof(GenerateOrdinaryTimelineUrl), 0, 200, 200, 200, 200, 200)]
        [InlineData(nameof(GeneratePersonalTimelineUrl), -1, 200, 401, 401, 401, 401)]
        [InlineData(nameof(GeneratePersonalTimelineUrl), 1, 200, 200, 403, 200, 403)]
        [InlineData(nameof(GeneratePersonalTimelineUrl), 0, 200, 200, 200, 200, 200)]

        public async Task Permission_Timeline(string generatorName, int userNumber, int get, int opPatchUser, int opPatchAdmin, int opMemberUser, int opMemberAdmin)
        {
            var method = GetType().GetMethod(generatorName, System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
            Func<int, string, string> generator = (int id, string subpath) => (string)method.Invoke(null, new object[] { id, subpath });

            using var client = await CreateClientAs(userNumber);
            {
                var res = await client.GetAsync("timelines/t1");
                res.Should().HaveStatusCode(get);
            }

            {
                var res = await client.PatchAsJsonAsync(generator(1, null), new TimelinePatchRequest { Description = "hahaha" });
                res.Should().HaveStatusCode(opPatchUser);
            }

            {
                var res = await client.PatchAsJsonAsync(generator(0, null), new TimelinePatchRequest { Description = "hahaha" });
                res.Should().HaveStatusCode(opPatchAdmin);
            }

            {
                var res = await client.PutAsync(generator(1, "members/user2"), null);
                res.Should().HaveStatusCode(opMemberUser);
            }

            {
                var res = await client.DeleteAsync(generator(1, "members/user2"));
                res.Should().HaveStatusCode(opMemberUser);
            }

            {
                var res = await client.PutAsync(generator(0, "members/user2"), null);
                res.Should().HaveStatusCode(opMemberAdmin);
            }

            {
                var res = await client.DeleteAsync(generator(0, "members/user2"));
                res.Should().HaveStatusCode(opMemberAdmin);
            }
        }

        [Theory]
        [MemberData(nameof(TimelineUrlGeneratorData))]
        public async Task Visibility_Test(Func<int, string, string> generator)
        {
            var userUrl = generator(1, "posts");
            var adminUrl = generator(0, "posts");
            {

                using var client = await CreateClientAsUser();
                using var content = new StringContent(@"{""visibility"":""abcdefg""}", System.Text.Encoding.UTF8, System.Net.Mime.MediaTypeNames.Application.Json);
                var res = await client.PatchAsync(generator(1, null), content);
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
                    var res = await client.PatchAsJsonAsync(generator(1, null),
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
                        var res = await client.PatchAsJsonAsync(generator(1, null),
                        new TimelinePatchRequest { Visibility = TimelineVisibility.Private });
                        res.Should().HaveStatusCode(200);
                    }
                    {
                        var res = await client.PatchAsJsonAsync(generator(0, null),
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
                    var res = await client.PutAsync(generator(0, "members/user1"), null);
                    res.Should().HaveStatusCode(200);
                }
                { // now user can read admin's
                    using var client = await CreateClientAsUser();
                    var res = await client.GetAsync(adminUrl);
                    res.Should().HaveStatusCode(200);
                }
            }
        }

        [Theory]
        [MemberData(nameof(TimelineUrlGeneratorData))]
        public async Task Permission_Post_Create(Func<int, string, string> generator)
        {
            using (var client = await CreateClientAsUser())
            {
                var res = await client.PutAsync(generator(1, "members/user2"), null);
                res.Should().HaveStatusCode(200);
            }

            using (var client = await CreateDefaultClient())
            {
                { // no auth should get 401
                    var res = await client.PostAsJsonAsync(generator(1, "posts"),
                        TimelineHelper.TextPostCreateRequest("aaa"));
                    res.Should().HaveStatusCode(401);
                }
            }

            using (var client = await CreateClientAsUser())
            {
                { // post self's
                    var res = await client.PostAsJsonAsync(generator(1, "posts"),
                        TimelineHelper.TextPostCreateRequest("aaa"));
                    res.Should().HaveStatusCode(200);
                }
                { // post other not as a member should get 403
                    var res = await client.PostAsJsonAsync(generator(0, "posts"),
                        TimelineHelper.TextPostCreateRequest("aaa"));
                    res.Should().HaveStatusCode(403);
                }
            }

            using (var client = await CreateClientAsAdministrator())
            {
                { // post as admin
                    var res = await client.PostAsJsonAsync(generator(1, "posts"),
                        TimelineHelper.TextPostCreateRequest("aaa"));
                    res.Should().HaveStatusCode(200);
                }
            }

            using (var client = await CreateClientAs(2))
            {
                { // post as member
                    var res = await client.PostAsJsonAsync(generator(1, "posts"),
                        TimelineHelper.TextPostCreateRequest("aaa"));
                    res.Should().HaveStatusCode(200);
                }
            }
        }

        [Theory]
        [MemberData(nameof(TimelineUrlGeneratorData))]
        public async Task Permission_Post_Delete(Func<int, string, string> generator)
        {
            async Task<long> CreatePost(int userNumber)
            {
                using var client = await CreateClientAs(userNumber);
                var res = await client.PostAsJsonAsync(generator(1, "posts"),
                    TimelineHelper.TextPostCreateRequest("aaa"));
                return res.Should().HaveStatusCode(200)
                    .And.HaveJsonBody<TimelinePostInfo>()
                    .Which.Id;
            }

            using (var client = await CreateClientAsUser())
            {
                {
                    var res = await client.PutAsync(generator(1, "members/user2"), null);
                    res.Should().HaveStatusCode(200);
                }
                {
                    var res = await client.PutAsync(generator(1, "members/user3"), null);
                    res.Should().HaveStatusCode(200);
                }
            }

            { // no auth should get 401
                using var client = await CreateDefaultClient();
                var res = await client.DeleteAsync(generator(1, "posts/12"));
                res.Should().HaveStatusCode(401);
            }

            { // self can delete self
                var postId = await CreatePost(1);
                using var client = await CreateClientAsUser();
                var res = await client.DeleteAsync(generator(1, $"posts/{postId}"));
                res.Should().HaveStatusCode(200);
            }

            { // admin can delete any
                var postId = await CreatePost(1);
                using var client = await CreateClientAsAdministrator();
                var res = await client.DeleteAsync(generator(1, $"posts/{postId}"));
                res.Should().HaveStatusCode(200);
            }

            { // owner can delete other
                var postId = await CreatePost(2);
                using var client = await CreateClientAsUser();
                var res = await client.DeleteAsync(generator(1, $"posts/{postId}"));
                res.Should().HaveStatusCode(200);
            }

            { // author can delete self
                var postId = await CreatePost(2);
                using var client = await CreateClientAs(2);
                var res = await client.DeleteAsync(generator(1, $"posts/{postId}"));
                res.Should().HaveStatusCode(200);
            }

            { // otherwise is forbidden
                var postId = await CreatePost(2);
                using var client = await CreateClientAs(3);
                var res = await client.DeleteAsync(generator(1, $"posts/{postId}"));
                res.Should().HaveStatusCode(403);
            }
        }

        [Theory]
        [MemberData(nameof(TimelineUrlGeneratorData))]
        public async Task Post_Op_Should_Work(Func<int, string, string> generator)
        {
            {
                using var client = await CreateClientAsUser();
                {
                    var res = await client.GetAsync(generator(1, "posts"));
                    res.Should().HaveStatusCode(200)
                        .And.HaveJsonBody<TimelinePostInfo[]>()
                        .Which.Should().NotBeNull().And.BeEmpty();
                }
                {
                    var res = await client.PostAsJsonAsync(generator(1, "posts"),
                        TimelineHelper.TextPostCreateRequest(null));
                    res.Should().BeInvalidModel();
                }
                const string mockContent = "aaa";
                TimelinePostInfo createRes;
                {
                    var res = await client.PostAsJsonAsync(generator(1, "posts"),
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
                    var res = await client.GetAsync(generator(1, "posts"));
                    res.Should().HaveStatusCode(200)
                        .And.HaveJsonBody<TimelinePostInfo[]>()
                        .Which.Should().NotBeNull().And.BeEquivalentTo(createRes);
                }
                const string mockContent2 = "bbb";
                var mockTime2 = DateTime.Now.AddDays(-1);
                TimelinePostInfo createRes2;
                {
                    var res = await client.PostAsJsonAsync(generator(1, "posts"),
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
                    var res = await client.GetAsync(generator(1, "posts"));
                    res.Should().HaveStatusCode(200)
                        .And.HaveJsonBody<TimelinePostInfo[]>()
                        .Which.Should().NotBeNull().And.BeEquivalentTo(createRes, createRes2);
                }
                {
                    var res = await client.DeleteAsync(generator(1, $"posts/{createRes.Id}"));
                    res.Should().BeDelete(true);
                }
                {
                    var res = await client.DeleteAsync(generator(1, $"posts/{createRes.Id}"));
                    res.Should().BeDelete(false);
                }
                {
                    var res = await client.DeleteAsync(generator(1, "posts/30000"));
                    res.Should().BeDelete(false);
                }
                {
                    var res = await client.GetAsync(generator(1, "posts"));
                    res.Should().HaveStatusCode(200)
                        .And.HaveJsonBody<TimelinePostInfo[]>()
                        .Which.Should().NotBeNull().And.BeEquivalentTo(createRes2);
                }
            }
        }

        [Theory]
        [MemberData(nameof(TimelineUrlGeneratorData))]
        public async Task GetPost_Should_Ordered(Func<int, string, string> generator)
        {
            using var client = await CreateClientAsUser();

            async Task<long> CreatePost(DateTime time)
            {
                var res = await client.PostAsJsonAsync(generator(1, "posts"),
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
                var res = await client.GetAsync(generator(1, "posts"));
                res.Should().HaveStatusCode(200)
                    .And.HaveJsonBody<TimelinePostInfo[]>()
                    .Which.Select(p => p.Id).Should().Equal(id1, id2, id0);
            }
        }

        [Theory]
        [MemberData(nameof(TimelineUrlGeneratorData))]
        public async Task CreatePost_InvalidModel(Func<int, string, string> generator)
        {
            using var client = await CreateClientAsUser();

            {
                var res = await client.PostAsJsonAsync(generator(1, "posts"), new TimelinePostCreateRequest { Content = null });
                res.Should().BeInvalidModel();
            }

            {
                var res = await client.PostAsJsonAsync(generator(1, "posts"), new TimelinePostCreateRequest { Content = new TimelinePostCreateRequestContent { Type = null } });
                res.Should().BeInvalidModel();
            }

            {
                var res = await client.PostAsJsonAsync(generator(1, "posts"), new TimelinePostCreateRequest { Content = new TimelinePostCreateRequestContent { Type = "hahaha" } });
                res.Should().BeInvalidModel();
            }

            {
                var res = await client.PostAsJsonAsync(generator(1, "posts"), new TimelinePostCreateRequest { Content = new TimelinePostCreateRequestContent { Type = "text", Text = null } });
                res.Should().BeInvalidModel();
            }

            {
                var res = await client.PostAsJsonAsync(generator(1, "posts"), new TimelinePostCreateRequest { Content = new TimelinePostCreateRequestContent { Type = "image", Data = null } });
                res.Should().BeInvalidModel();
            }

            {
                // image not base64
                var res = await client.PostAsJsonAsync(generator(1, "posts"), new TimelinePostCreateRequest { Content = new TimelinePostCreateRequestContent { Type = "image", Data = "!!!" } });
                res.Should().BeInvalidModel();
            }

            {
                // image base64 not image
                var res = await client.PostAsJsonAsync(generator(1, "posts"), new TimelinePostCreateRequest { Content = new TimelinePostCreateRequestContent { Type = "image", Data = Convert.ToBase64String(new byte[] { 0x01, 0x02, 0x03 }) } });
                res.Should().BeInvalidModel();
            }
        }
    }
}
