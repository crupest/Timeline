using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Timeline.Entities;
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
        public TimelineTest() : base(3)
        {
        }

        protected override async Task OnInitializeAsync()
        {
            await CreateTestTimelines();
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

        private static string CalculateUrlTail(string subpath, ICollection<KeyValuePair<string, string>> query)
        {
            StringBuilder result = new StringBuilder();
            if (subpath != null)
            {
                if (!subpath.StartsWith("/", StringComparison.OrdinalIgnoreCase))
                    result.Append("/");
                result.Append(subpath);
            }

            if (query != null && query.Count != 0)
            {
                result.Append("?");
                foreach (var (key, value, index) in query.Select((pair, index) => (pair.Key, pair.Value, index)))
                {
                    result.Append(WebUtility.UrlEncode(key));
                    result.Append("=");
                    result.Append(WebUtility.UrlEncode(value));
                    if (index != query.Count - 1)
                        result.Append("&");
                }
            }

            return result.ToString();
        }

        private static string GeneratePersonalTimelineUrl(int id, string subpath = null, ICollection<KeyValuePair<string, string>> query = null)
        {
            return $"timelines/@{(id == 0 ? "admin" : ("user" + id))}{CalculateUrlTail(subpath, query)}";
        }

        private static string GenerateOrdinaryTimelineUrl(int id, string subpath = null, ICollection<KeyValuePair<string, string>> query = null)
        {
            return $"timelines/t{id}{CalculateUrlTail(subpath, query)}";
        }

        public delegate string TimelineUrlGenerator(int userId, string subpath = null, ICollection<KeyValuePair<string, string>> query = null);

        public static IEnumerable<object[]> TimelineUrlGeneratorData()
        {
            yield return new[] { new TimelineUrlGenerator(GeneratePersonalTimelineUrl) };
            yield return new[] { new TimelineUrlGenerator(GenerateOrdinaryTimelineUrl) };
        }

        private static string GeneratePersonalTimelineUrlByName(string name, string subpath = null)
        {
            return $"timelines/@{name}{(subpath == null ? "" : "/" + subpath)}";
        }

        private static string GenerateOrdinaryTimelineUrlByName(string name, string subpath = null)
        {
            return $"timelines/{name}{(subpath == null ? "" : "/" + subpath)}";
        }

        public static IEnumerable<object[]> TimelineUrlByNameGeneratorData()
        {
            yield return new[] { new Func<string, string, string>(GeneratePersonalTimelineUrlByName) };
            yield return new[] { new Func<string, string, string>(GenerateOrdinaryTimelineUrlByName) };
        }

        [Fact]
        public async Task TimelineGet_Should_Work()
        {
            using var client = await CreateDefaultClient();
            {
                var res = await client.GetAsync("timelines/@user1");
                var body = res.Should().HaveStatusCode(200)
                    .And.HaveJsonBody<TimelineInfo>().Which;
                body.Owner.Should().BeEquivalentTo(UserInfos[1]);
                body.Visibility.Should().Be(TimelineVisibility.Register);
                body.Description.Should().Be("");
                body.Members.Should().NotBeNull().And.BeEmpty();
                var links = body._links;
                links.Should().NotBeNull();
                links.Self.Should().EndWith("timelines/@user1");
                links.Posts.Should().EndWith("timelines/@user1/posts");
            }

            {
                var res = await client.GetAsync("timelines/t1");
                var body = res.Should().HaveStatusCode(200)
                    .And.HaveJsonBody<TimelineInfo>().Which;
                body.Owner.Should().BeEquivalentTo(UserInfos[1]);
                body.Visibility.Should().Be(TimelineVisibility.Register);
                body.Description.Should().Be("");
                body.Members.Should().NotBeNull().And.BeEmpty();
                var links = body._links;
                links.Should().NotBeNull();
                links.Self.Should().EndWith("timelines/t1");
                links.Posts.Should().EndWith("timelines/t1/posts");
            }
        }

        [Fact]
        public async Task TimelineList()
        {
            TimelineInfo user1Timeline;

            var client = await CreateDefaultClient();

            {
                var res = await client.GetAsync("timelines/@user1");
                user1Timeline = res.Should().HaveStatusCode(200)
                    .And.HaveJsonBody<TimelineInfo>().Which;
            }

            {
                var testResult = new List<TimelineInfo>();
                testResult.Add(user1Timeline);
                testResult.AddRange(_testTimelines);

                var res = await client.GetAsync("timelines");
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
                    var res = await client.PutAsync("timelines/@user1/members/user3", null);
                    res.Should().HaveStatusCode(200);
                }

                {
                    var res = await client.PutAsync("timelines/t1/members/user3", null);
                    res.Should().HaveStatusCode(200);
                }

                {
                    var res = await client.PatchAsJsonAsync("timelines/@user1", new TimelinePatchRequest { Visibility = TimelineVisibility.Public });
                    res.Should().HaveStatusCode(200);
                }

                {
                    var res = await client.PatchAsJsonAsync("timelines/t1", new TimelinePatchRequest { Visibility = TimelineVisibility.Register });
                    res.Should().HaveStatusCode(200);
                }

                {
                    var res = await client.GetAsync("timelines/@user1");
                    var timeline = res.Should().HaveStatusCode(200)
                        .And.HaveJsonBody<TimelineInfo>().Which;
                    testResultRelate.Add(timeline);
                    testResultJoin.Add(timeline);
                    testResultRelatePublic.Add(timeline);
                    testResultPublic.Add(timeline);
                }

                {
                    var res = await client.GetAsync("timelines/t1");
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
                    var res = await client.PutAsync("timelines/@user2/members/user3", null);
                    res.Should().HaveStatusCode(200);
                }

                {
                    var res = await client.PutAsync("timelines/t2/members/user3", null);
                    res.Should().HaveStatusCode(200);
                }

                {
                    var res = await client.PatchAsJsonAsync("timelines/@user2", new TimelinePatchRequest { Visibility = TimelineVisibility.Register });
                    res.Should().HaveStatusCode(200);
                }

                {
                    var res = await client.PatchAsJsonAsync("timelines/t2", new TimelinePatchRequest { Visibility = TimelineVisibility.Private });
                    res.Should().HaveStatusCode(200);
                }

                {
                    var res = await client.GetAsync("timelines/@user2");
                    var timeline = res.Should().HaveStatusCode(200)
                        .And.HaveJsonBody<TimelineInfo>().Which;
                    testResultRelate.Add(timeline);
                    testResultJoin.Add(timeline);
                    testResultRelateRegister.Add(timeline);
                }

                {
                    var res = await client.GetAsync("timelines/t2");
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
                    var res = await client.PatchAsJsonAsync("timelines/@user3", new TimelinePatchRequest { Visibility = TimelineVisibility.Private });
                    res.Should().HaveStatusCode(200);
                }

                {
                    var res = await client.PatchAsJsonAsync("timelines/t3", new TimelinePatchRequest { Visibility = TimelineVisibility.Register });
                    res.Should().HaveStatusCode(200);
                }

                {
                    var res = await client.GetAsync("timelines/@user3");
                    var timeline = res.Should().HaveStatusCode(200)
                        .And.HaveJsonBody<TimelineInfo>().Which;
                    testResultRelate.Add(timeline);
                    testResultOwn.Add(timeline);
                    testResultOwnPrivate.Add(timeline);
                }

                {
                    var res = await client.GetAsync("timelines/t3");
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
                    var res = await client.GetAsync("timelines?relate=user3");
                    var body = res.Should().HaveStatusCode(200)
                        .And.HaveJsonBody<List<TimelineInfo>>()
                        .Which;
                    body.Should().BeEquivalentTo(testResultRelate);
                }

                {
                    var res = await client.GetAsync("timelines?relate=user3&relateType=own");
                    var body = res.Should().HaveStatusCode(200)
                        .And.HaveJsonBody<List<TimelineInfo>>()
                        .Which;
                    body.Should().BeEquivalentTo(testResultOwn);
                }

                {
                    var res = await client.GetAsync("timelines?relate=user3&visibility=public");
                    var body = res.Should().HaveStatusCode(200)
                        .And.HaveJsonBody<List<TimelineInfo>>()
                        .Which;
                    body.Should().BeEquivalentTo(testResultRelatePublic);
                }

                {
                    var res = await client.GetAsync("timelines?relate=user3&visibility=register");
                    var body = res.Should().HaveStatusCode(200)
                        .And.HaveJsonBody<List<TimelineInfo>>()
                        .Which;
                    body.Should().BeEquivalentTo(testResultRelateRegister);
                }

                {
                    var res = await client.GetAsync("timelines?relate=user3&relateType=join&visibility=private");
                    var body = res.Should().HaveStatusCode(200)
                        .And.HaveJsonBody<List<TimelineInfo>>()
                        .Which;
                    body.Should().BeEquivalentTo(testResultJoinPrivate);
                }

                {
                    var res = await client.GetAsync("timelines?relate=user3&relateType=own&visibility=private");
                    var body = res.Should().HaveStatusCode(200)
                        .And.HaveJsonBody<List<TimelineInfo>>()
                        .Which;
                    body.Should().BeEquivalentTo(testResultOwnPrivate);
                }
            }

            {
                var client = await CreateDefaultClient();
                {
                    var res = await client.GetAsync("timelines?visibility=public");
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
                var res = await client.GetAsync("timelines?relate=us!!");
                res.Should().BeInvalidModel();
            }

            {
                var res = await client.GetAsync("timelines?relateType=aaa");
                res.Should().BeInvalidModel();
            }

            {
                var res = await client.GetAsync("timelines?visibility=aaa");
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

        [Theory]
        [MemberData(nameof(TimelineUrlByNameGeneratorData))]
        public async Task InvalidModel_BadName(Func<string, string, string> generator)
        {
            using var client = await CreateClientAsAdministrator();
            {
                var res = await client.GetAsync(generator("aaa!!!", null));
                res.Should().BeInvalidModel();
            }
            {
                var res = await client.PatchAsJsonAsync(generator("aaa!!!", null), new TimelinePatchRequest { });
                res.Should().BeInvalidModel();
            }
            {
                var res = await client.PutAsync(generator("aaa!!!", "members/user1"), null);
                res.Should().BeInvalidModel();
            }
            {
                var res = await client.DeleteAsync(generator("aaa!!!", "members/user1"));
                res.Should().BeInvalidModel();
            }
            {
                var res = await client.GetAsync(generator("aaa!!!", "posts"));
                res.Should().BeInvalidModel();
            }
            {
                var res = await client.PostAsJsonAsync(generator("aaa!!!", "posts"), TimelineHelper.TextPostCreateRequest("aaa"));
                res.Should().BeInvalidModel();
            }
            {
                var res = await client.DeleteAsync(generator("aaa!!!", "posts/123"));
                res.Should().BeInvalidModel();
            }
            {
                var res = await client.GetAsync(generator("aaa!!!", "posts/123/data"));
                res.Should().BeInvalidModel();
            }
        }

        [Theory]
        [MemberData(nameof(TimelineUrlByNameGeneratorData))]
        public async Task Ordinary_NotFound(Func<string, string, string> generator)
        {
            var errorCode = generator == GenerateOrdinaryTimelineUrlByName ? ErrorCodes.TimelineController.NotExist : ErrorCodes.UserCommon.NotExist;

            using var client = await CreateClientAsAdministrator();
            {
                var res = await client.GetAsync(generator("notexist", null));
                res.Should().HaveStatusCode(404).And.HaveCommonBody(errorCode);
            }
            {
                var res = await client.PatchAsJsonAsync(generator("notexist", null), new TimelinePatchRequest { });
                res.Should().HaveStatusCode(404).And.HaveCommonBody(errorCode);
            }
            {
                var res = await client.PutAsync(generator("notexist", "members/user1"), null);
                res.Should().HaveStatusCode(404).And.HaveCommonBody(errorCode);
            }
            {
                var res = await client.DeleteAsync(generator("notexist", "members/user1"));
                res.Should().HaveStatusCode(404).And.HaveCommonBody(errorCode);
            }
            {
                var res = await client.GetAsync(generator("notexist", "posts"));
                res.Should().HaveStatusCode(404).And.HaveCommonBody(errorCode);
            }
            {
                var res = await client.PostAsJsonAsync(generator("notexist", "posts"), TimelineHelper.TextPostCreateRequest("aaa"));
                res.Should().HaveStatusCode(404).And.HaveCommonBody(errorCode);
            }
            {
                var res = await client.DeleteAsync(generator("notexist", "posts/123"));
                res.Should().HaveStatusCode(404).And.HaveCommonBody(errorCode);
            }
            {
                var res = await client.GetAsync(generator("notexist", "posts/123/data"));
                res.Should().HaveStatusCode(404).And.HaveCommonBody(errorCode);
            }
        }

        [Theory]
        [MemberData(nameof(TimelineUrlGeneratorData))]
        public async Task Description_Should_Work(TimelineUrlGenerator generator)
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
        public async Task Member_Should_Work(TimelineUrlGenerator generator)
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

        public static IEnumerable<object[]> Permission_Timeline_Data()
        {
            yield return new object[] { new TimelineUrlGenerator(GenerateOrdinaryTimelineUrl), -1, 200, 401, 401, 401, 401 };
            yield return new object[] { new TimelineUrlGenerator(GenerateOrdinaryTimelineUrl), 1, 200, 200, 403, 200, 403 };
            yield return new object[] { new TimelineUrlGenerator(GenerateOrdinaryTimelineUrl), 0, 200, 200, 200, 200, 200 };
            yield return new object[] { new TimelineUrlGenerator(GeneratePersonalTimelineUrl), -1, 200, 401, 401, 401, 401 };
            yield return new object[] { new TimelineUrlGenerator(GeneratePersonalTimelineUrl), 1, 200, 200, 403, 200, 403 };
            yield return new object[] { new TimelineUrlGenerator(GeneratePersonalTimelineUrl), 0, 200, 200, 200, 200, 200 };
        }

        [Theory]
        [MemberData(nameof(Permission_Timeline_Data))]
        public async Task Permission_Timeline(TimelineUrlGenerator generator, int userNumber, int get, int opPatchUser, int opPatchAdmin, int opMemberUser, int opMemberAdmin)
        {
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
        public async Task Visibility_Test(TimelineUrlGenerator generator)
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
        public async Task Permission_Post_Create(TimelineUrlGenerator generator)
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
        public async Task Permission_Post_Delete(TimelineUrlGenerator generator)
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
        public async Task TextPost_ShouldWork(TimelineUrlGenerator generator)
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
                    body.Deleted.Should().BeFalse();
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
                    body.Deleted.Should().BeFalse();
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
        public async Task GetPost_Should_Ordered(TimelineUrlGenerator generator)
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
        public async Task CreatePost_InvalidModel(TimelineUrlGenerator generator)
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

        [Theory]
        [MemberData(nameof(TimelineUrlGeneratorData))]
        public async Task ImagePost_ShouldWork(TimelineUrlGenerator generator)
        {
            var imageData = ImageHelper.CreatePngWithSize(100, 200);

            long postId;
            string postImageUrl;

            void AssertPostContent(TimelinePostContentInfo content)
            {
                content.Type.Should().Be(TimelinePostContentTypes.Image);
                content.Url.Should().EndWith(generator(1, $"posts/{postId}/data"));
                content.Text.Should().Be(null);
            }

            using var client = await CreateClientAsUser();

            {
                var res = await client.PostAsJsonAsync(generator(1, "posts"),
                    new TimelinePostCreateRequest
                    {
                        Content = new TimelinePostCreateRequestContent
                        {
                            Type = TimelinePostContentTypes.Image,
                            Data = Convert.ToBase64String(imageData)
                        }
                    });
                var body = res.Should().HaveStatusCode(200)
                    .And.HaveJsonBody<TimelinePostInfo>().Which;
                postId = body.Id;
                postImageUrl = body.Content.Url;
                AssertPostContent(body.Content);
            }

            {
                var res = await client.GetAsync(generator(1, "posts"));
                var body = res.Should().HaveStatusCode(200)
                    .And.HaveJsonBody<TimelinePostInfo[]>().Which;
                body.Should().HaveCount(1);
                var post = body[0];
                post.Id.Should().Be(postId);
                AssertPostContent(post.Content);
            }

            {
                var res = await client.GetAsync(generator(1, $"posts/{postId}/data"));
                res.Content.Headers.ContentType.MediaType.Should().Be("image/png");
                var data = await res.Content.ReadAsByteArrayAsync();
                var image = Image.Load(data, out var format);
                image.Width.Should().Be(100);
                image.Height.Should().Be(200);
                format.Name.Should().Be(PngFormat.Instance.Name);
            }

            {
                await CacheTestHelper.TestCache(client, generator(1, $"posts/{postId}/data"));
            }

            {
                var res = await client.DeleteAsync(generator(1, $"posts/{postId}"));
                res.Should().BeDelete(true);
            }

            {
                var res = await client.DeleteAsync(generator(1, $"posts/{postId}"));
                res.Should().BeDelete(false);
            }

            {
                var res = await client.GetAsync(generator(1, "posts"));
                res.Should().HaveStatusCode(200)
                    .And.HaveJsonBody<TimelinePostInfo[]>()
                    .Which.Should().BeEmpty();
            }

            {
                using var scope = TestApp.Host.Services.CreateScope();
                var database = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
                var count = await database.Data.CountAsync();
                count.Should().Be(0);
            }
        }

        [Theory]
        [MemberData(nameof(TimelineUrlGeneratorData))]
        public async Task ImagePost_400(TimelineUrlGenerator generator)
        {
            using var client = await CreateClientAsUser();

            {
                var res = await client.GetAsync(generator(1, "posts/11234/data"));
                res.Should().HaveStatusCode(404)
                    .And.HaveCommonBody(ErrorCodes.TimelineController.PostNotExist);
            }

            long postId;
            {
                var res = await client.PostAsJsonAsync(generator(1, "posts"),
                    TimelineHelper.TextPostCreateRequest("aaa"));
                var body = res.Should().HaveStatusCode(200)
                    .And.HaveJsonBody<TimelinePostInfo>()
                    .Which;
                postId = body.Id;
            }

            {
                var res = await client.GetAsync(generator(1, $"posts/{postId}/data"));
                res.Should().HaveStatusCode(400)
                    .And.HaveCommonBody(ErrorCodes.TimelineController.PostNoData);
            }
        }

        [Theory]
        [MemberData(nameof(TimelineUrlGeneratorData))]
        public async Task Timeline_LastModified(TimelineUrlGenerator generator)
        {
            using var client = await CreateClientAsUser();

            DateTime lastModified;

            {
                var res = await client.GetAsync(generator(1));
                lastModified = res.Should().HaveStatusCode(200)
                    .And.HaveJsonBody<TimelineInfo>()
                    .Which.LastModified;
            }

            await Task.Delay(1000);

            {
                var res = await client.PatchAsJsonAsync(generator(1), new TimelinePatchRequest { Description = "123" });
                lastModified = res.Should().HaveStatusCode(200)
                    .And.HaveJsonBody<TimelineInfo>()
                    .Which.LastModified.Should().BeAfter(lastModified).And.Subject.Value;
            }

            {
                var res = await client.GetAsync(generator(1));
                res.Should().HaveStatusCode(200)
                    .And.HaveJsonBody<TimelineInfo>()
                    .Which.LastModified.Should().Be(lastModified);
            }

            await Task.Delay(1000);

            {
                var res = await client.PutAsync(generator(1, "members/user2"), null);
                res.Should().HaveStatusCode(200);
            }

            {
                var res = await client.GetAsync(generator(1));
                res.Should().HaveStatusCode(200)
                    .And.HaveJsonBody<TimelineInfo>()
                    .Which.LastModified.Should().BeAfter(lastModified);
            }
        }

        [Theory]
        [MemberData(nameof(TimelineUrlGeneratorData))]
        public async Task Post_ModifiedSince(TimelineUrlGenerator generator)
        {
            using var client = await CreateClientAsUser();

            var postContentList = new List<string> { "a", "b", "c", "d" };
            var posts = new List<TimelinePostInfo>();

            foreach (var content in postContentList)
            {
                var res = await client.PostAsJsonAsync(generator(1, "posts"),
                    new TimelinePostCreateRequest { Content = new TimelinePostCreateRequestContent { Text = content, Type = TimelinePostContentTypes.Text } });
                var post = res.Should().HaveStatusCode(200)
                    .And.HaveJsonBody<TimelinePostInfo>().Which;
                posts.Add(post);
                await Task.Delay(1000);
            }

            {
                var res = await client.DeleteAsync(generator(1, $"posts/{posts[2].Id}"));
                res.Should().BeDelete(true);
            }

            {
                var res = await client.GetAsync(generator(1, "posts",
                    new Dictionary<string, string> { { "modifiedSince", posts[1].LastUpdated.ToString("s", CultureInfo.InvariantCulture) } }));
                res.Should().HaveStatusCode(200)
                    .And.HaveJsonBody<List<TimelinePostInfo>>()
                    .Which.Should().HaveCount(2)
                    .And.Subject.Select(p => p.Content.Text).Should().Equal("b", "d");
            }
        }

        [Theory]
        [MemberData(nameof(TimelineUrlGeneratorData))]
        public async Task PostList_IncludeDeleted(TimelineUrlGenerator urlGenerator)
        {
            using var client = await CreateClientAsUser();

            var postContentList = new List<string> { "a", "b", "c", "d" };
            var posts = new List<TimelinePostInfo>();

            foreach (var content in postContentList)
            {
                var res = await client.PostAsJsonAsync(urlGenerator(1, "posts"),
                    new TimelinePostCreateRequest { Content = new TimelinePostCreateRequestContent { Text = content, Type = TimelinePostContentTypes.Text } });
                posts.Add(res.Should().HaveStatusCode(200)
                    .And.HaveJsonBody<TimelinePostInfo>().Which);
            }

            foreach (var id in new long[] { posts[0].Id, posts[2].Id })
            {
                var res = await client.DeleteAsync(urlGenerator(1, $"posts/{id}"));
                res.Should().BeDelete(true);
            }

            {
                var res = await client.GetAsync(urlGenerator(1, "posts", new Dictionary<string, string> { ["includeDeleted"] = "true" }));
                posts = res.Should().HaveStatusCode(200)
                    .And.HaveJsonBody<List<TimelinePostInfo>>()
                    .Which;
                posts.Should().HaveCount(4);
                posts.Select(p => p.Deleted).Should().Equal(true, false, true, false);
                posts.Select(p => p.Content == null).Should().Equal(true, false, true, false);
            }
        }

        [Theory]
        [MemberData(nameof(TimelineUrlGeneratorData))]
        public async Task Post_ModifiedSince_And_IncludeDeleted(TimelineUrlGenerator urlGenerator)
        {
            using var client = await CreateClientAsUser();

            var postContentList = new List<string> { "a", "b", "c", "d" };
            var posts = new List<TimelinePostInfo>();

            foreach (var (content, index) in postContentList.Select((v, i) => (v, i)))
            {
                var res = await client.PostAsJsonAsync(urlGenerator(1, "posts"),
                    new TimelinePostCreateRequest { Content = new TimelinePostCreateRequestContent { Text = content, Type = TimelinePostContentTypes.Text } });
                var post = res.Should().HaveStatusCode(200)
                    .And.HaveJsonBody<TimelinePostInfo>().Which;
                posts.Add(post);
                await Task.Delay(1000);
            }

            {
                var res = await client.DeleteAsync(urlGenerator(1, $"posts/{posts[2].Id}"));
                res.Should().BeDelete(true);
            }

            {

                var res = await client.GetAsync(urlGenerator(1, "posts",
                    new Dictionary<string, string> {
                        { "modifiedSince", posts[1].LastUpdated.ToString("s", CultureInfo.InvariantCulture) },
                        { "includeDeleted", "true" }
                    }));
                posts = res.Should().HaveStatusCode(200)
                    .And.HaveJsonBody<List<TimelinePostInfo>>()
                    .Which;
                posts.Should().HaveCount(3);
                posts.Select(p => p.Deleted).Should().Equal(false, true, false);
                posts.Select(p => p.Content == null).Should().Equal(false, true, false);
            }
        }

        [Theory]
        [MemberData(nameof(TimelineUrlGeneratorData))]
        public async Task Timeline_Get_IfModifiedSince_And_CheckUniqueId(TimelineUrlGenerator urlGenerator)
        {
            using var client = await CreateClientAsUser();

            DateTime lastModifiedTime;
            TimelineInfo timeline;
            string uniqueId;

            {
                var res = await client.GetAsync(urlGenerator(1));
                var body = res.Should().HaveStatusCode(200)
                    .And.HaveJsonBody<TimelineInfo>().Which;
                timeline = body;
                lastModifiedTime = body.LastModified;
                uniqueId = body.UniqueId;
            }

            {
                using var req = new HttpRequestMessage
                {
                    RequestUri = new Uri(client.BaseAddress, urlGenerator(1)),
                    Method = HttpMethod.Get,
                };
                req.Headers.IfModifiedSince = lastModifiedTime.AddSeconds(1);
                var res = await client.SendAsync(req);
                res.Should().HaveStatusCode(304);
            }

            {
                using var req = new HttpRequestMessage
                {
                    RequestUri = new Uri(client.BaseAddress, urlGenerator(1)),
                    Method = HttpMethod.Get,
                };
                req.Headers.IfModifiedSince = lastModifiedTime.AddSeconds(-1);
                var res = await client.SendAsync(req);
                res.Should().HaveStatusCode(200)
                    .And.HaveJsonBody<TimelineInfo>()
                    .Which.Should().BeEquivalentTo(timeline);
            }

            {
                var res = await client.GetAsync(urlGenerator(1, null,
                    new Dictionary<string, string> { { "ifModifiedSince", lastModifiedTime.AddSeconds(1).ToString("s", CultureInfo.InvariantCulture) } }));
                res.Should().HaveStatusCode(304);
            }

            {
                var res = await client.GetAsync(urlGenerator(1, null,
                    new Dictionary<string, string> { { "ifModifiedSince", lastModifiedTime.AddSeconds(-1).ToString("s", CultureInfo.InvariantCulture) } }));
                res.Should().HaveStatusCode(200)
                    .And.HaveJsonBody<TimelineInfo>()
                    .Which.Should().BeEquivalentTo(timeline);
            }

            {
                var res = await client.GetAsync(urlGenerator(1, null,
                    new Dictionary<string, string> { { "ifModifiedSince", lastModifiedTime.AddSeconds(1).ToString("s", CultureInfo.InvariantCulture) },
                        {"checkUniqueId", uniqueId } }));
                res.Should().HaveStatusCode(304);
            }

            {
                var testUniqueId = (uniqueId[0] == 'a' ? "b" : "a") + uniqueId[1..];
                var res = await client.GetAsync(urlGenerator(1, null,
                    new Dictionary<string, string> { { "ifModifiedSince", lastModifiedTime.AddSeconds(1).ToString("s", CultureInfo.InvariantCulture) },
                        {"checkUniqueId", testUniqueId } }));
                res.Should().HaveStatusCode(200)
                    .And.HaveJsonBody<TimelineInfo>()
                    .Which.Should().BeEquivalentTo(timeline);
            }
        }
    }
}
