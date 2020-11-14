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

        private async Task CreateTestTimelines()
        {
            for (int i = 0; i <= 3; i++)
            {
                using var client = await CreateClientAs(i);
                await client.TestPostAsync("timelines", new TimelineCreateRequest { Name = $"t{i}" });
            }
        }

        [Fact]
        public async Task TimelineGet_Should_Work()
        {
            using var client = await CreateDefaultClient();

            await client.TestGetAssertInvalidModelAsync("timelines/@!!!");
            await client.TestGetAssertInvalidModelAsync("timelines/!!!");


            {
                var body = await client.TestGetAsync<TimelineInfo>("timelines/@user1");
                body.Owner.Should().BeEquivalentTo(await client.GetUserAsync("user1"));
                body.Visibility.Should().Be(TimelineVisibility.Register);
                body.Description.Should().Be("");
                body.Members.Should().NotBeNull().And.BeEmpty();
                var links = body._links;
                links.Should().NotBeNull();
                links.Self.Should().EndWith("timelines/@user1");
                links.Posts.Should().EndWith("timelines/@user1/posts");
            }

            {
                var body = await client.TestGetAsync<TimelineInfo>("timelines/t1");
                body.Owner.Should().BeEquivalentTo(await client.GetUserAsync("user1"));
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
        public async Task TimelineList_Should_Work()
        {
            using var client = await CreateDefaultClient();

            var result = new List<TimelineInfo>
            {
                await client.GetTimelineAsync("@user1")
            };

            for (int i = 0; i <= TestUserCount; i++)
            {
                result.Add(await client.GetTimelineAsync($"t{i}"));
            }


            var body = await client.TestGetAsync<List<TimelineInfo>>("timelines");
            body.Should().BeEquivalentTo(result);
        }

        [Fact]
        public async Task TimelineListWithQuery_Should_Work()
        {
            {
                using var client = await CreateDefaultClient();

                await client.TestGetAssertInvalidModelAsync("timelines?relate=us!!");
                await client.TestGetAssertInvalidModelAsync("timelines?relateType=aaa");
                await client.TestGetAssertInvalidModelAsync("timelines?visibility=aaa");
            }

            var testResultRelate = new List<TimelineInfo>();
            var testResultOwn = new List<TimelineInfo>();
            var testResultJoin = new List<TimelineInfo>();
            var testResultOwnPrivate = new List<TimelineInfo>();
            var testResultRelatePublic = new List<TimelineInfo>();
            var testResultRelateRegister = new List<TimelineInfo>();
            var testResultJoinPrivate = new List<TimelineInfo>();
            var testResultPublic = new List<TimelineInfo>();

            {
                using var client = await CreateClientAsUser();

                await client.PutTimelineMemberAsync("@user1", "user3");
                await client.PutTimelineMemberAsync("t1", "user3");
                await client.PatchTimelineAsync("@user1", new() { Visibility = TimelineVisibility.Public });
                await client.PatchTimelineAsync("t1", new() { Visibility = TimelineVisibility.Register });

                {
                    var timeline = await client.GetTimelineAsync("@user1");
                    testResultRelate.Add(timeline);
                    testResultJoin.Add(timeline);
                    testResultRelatePublic.Add(timeline);
                    testResultPublic.Add(timeline);
                }

                {
                    var timeline = await client.GetTimelineAsync("t1");
                    testResultRelate.Add(timeline);
                    testResultJoin.Add(timeline);
                    testResultRelateRegister.Add(timeline);
                }
            }

            {
                using var client = await CreateClientAs(2);

                await client.PutTimelineMemberAsync("@user2", "user3");
                await client.PutTimelineMemberAsync("t2", "user3");
                await client.PatchTimelineAsync("@user2", new() { Visibility = TimelineVisibility.Register });
                await client.PatchTimelineAsync("t2", new() { Visibility = TimelineVisibility.Private });

                {
                    var timeline = await client.GetTimelineAsync("@user2");
                    testResultRelate.Add(timeline);
                    testResultJoin.Add(timeline);
                    testResultRelateRegister.Add(timeline);
                }

                {
                    var timeline = await client.GetTimelineAsync("t2");
                    testResultRelate.Add(timeline);
                    testResultJoin.Add(timeline);
                    testResultJoinPrivate.Add(timeline);
                }
            }

            {
                using var client = await CreateClientAs(3);
                await client.PatchTimelineAsync("@user3", new TimelinePatchRequest { Visibility = TimelineVisibility.Private });
                await client.PatchTimelineAsync("t3", new TimelinePatchRequest { Visibility = TimelineVisibility.Register });

                {
                    var timeline = await client.GetTimelineAsync("@user3");
                    testResultRelate.Add(timeline);
                    testResultOwn.Add(timeline);
                    testResultOwnPrivate.Add(timeline);
                }

                {
                    var timeline = await client.GetTimelineAsync("t3");
                    testResultRelate.Add(timeline);
                    testResultOwn.Add(timeline);
                    testResultRelateRegister.Add(timeline);
                }
            }

            {
                using var client = await CreateDefaultClient();

                async Task TestAgainst(string url, List<TimelineInfo> against)
                {
                    var body = await client.TestGetAsync<List<TimelineInfo>>(url);
                    body.Should().BeEquivalentTo(against);
                }

                await TestAgainst("timelines?relate=user3", testResultRelate);
                await TestAgainst("timelines?relate=user3&relateType=own", testResultOwn);
                await TestAgainst("timelines?relate=user3&visibility=public", testResultRelatePublic);
                await TestAgainst("timelines?relate=user3&visibility=register", testResultRelateRegister);
                await TestAgainst("timelines?relate=user3&relateType=join&visibility=private", testResultJoinPrivate);
                await TestAgainst("timelines?relate=user3&relateType=own&visibility=private", testResultOwnPrivate);
                await TestAgainst("timelines?visibility=public", testResultPublic);
            }
        }

        [Fact]
        public async Task TimelineCreate_Should_Work()
        {
            {
                using var client = await CreateDefaultClient();
                await client.TestPostAssertUnauthorizedAsync("timelines", new TimelineCreateRequest { Name = "aaa" });
            }

            {
                using var client = await CreateClientAsUser();

                await client.TestPostAssertInvalidModelAsync("timelines", new TimelineCreateRequest { Name = "!!!" });

                {
                    var body = await client.TestPostAsync<TimelineInfo>("timelines", new TimelineCreateRequest { Name = "aaa" });
                    body.Should().BeEquivalentTo(await client.GetTimelineAsync("aaa"));
                }

                await client.TestPostAssertErrorAsync("timelines", new TimelineCreateRequest { Name = "aaa" }, errorCode: ErrorCodes.TimelineController.NameConflict);
            }
        }

        [Fact]
        public async Task TimelineDelete_Should_Work()
        {
            {
                using var client = await CreateDefaultClient();
                await client.TestDeleteAssertUnauthorizedAsync("timelines/t1");
            }

            {
                using var client = await CreateClientAs(2);
                await client.TestDeleteAssertForbiddenAsync("timelines/t1");
            }

            {
                using var client = await CreateClientAsAdministrator();

                await client.TestDeleteAssertInvalidModelAsync("timelines/!!!");
                await client.TestDeleteAsync("timelines/t2", true);
                await client.TestDeleteAsync("timelines/t2", false);
            }

            {
                using var client = await CreateClientAs(1);

                await client.TestDeleteAssertInvalidModelAsync("timelines/!!!");
                await client.TestDeleteAsync("timelines/t1", true);
                await client.TestDeleteAssertErrorAsync("timelines/t1");
            }
        }

        public static string CreatePersonalTimelineName(int i) => i == 0 ? "@admin" : $"@user{i}";
        public static string CreateOrdinaryTimelineName(int i) => $"t{i}";
        public delegate string TimelineNameGenerator(int i);

        public static IEnumerable<object[]> TimelineNameGeneratorTestData()
        {
            yield return new object[] { new TimelineNameGenerator(CreatePersonalTimelineName) };
            yield return new object[] { new TimelineNameGenerator(CreateOrdinaryTimelineName) };
        }

        [Theory]
        [MemberData(nameof(TimelineNameGeneratorTestData))]
        public async Task TimelineDescription_Should_Work(TimelineNameGenerator generator)
        {
            // TODO! Permission tests.

            using var client = await CreateClientAsUser();
            var timelineName = generator(1);

            {
                var timeline = await client.GetTimelineAsync(timelineName);
                timeline.Description.Should().BeEmpty();
            }

            const string mockDescription = "haha";

            {
                var timeline = await client.PatchTimelineAsync(timelineName, new() { Description = mockDescription });
                timeline.Description.Should().Be(mockDescription);
            }

            {
                var timeline = await client.GetTimelineAsync(timelineName);
                timeline.Description.Should().Be(mockDescription);
            }

            {
                var timeline = await client.PatchTimelineAsync(timelineName, new() { Description = null });
                timeline.Description.Should().Be(mockDescription);
            }

            {
                var timeline = await client.GetTimelineAsync(timelineName);
                timeline.Description.Should().Be(mockDescription);
            }

            {
                var timeline = await client.PatchTimelineAsync(timelineName, new() { Description = "" });
                timeline.Description.Should().BeEmpty();
            }

            {
                var timeline = await client.GetTimelineAsync(timelineName);
                timeline.Description.Should().BeEmpty();
            }
        }

        [Theory]
        [MemberData(nameof(TimelineNameGeneratorTestData))]
        public async Task Member_Should_Work(TimelineNameGenerator generator)
        {
            // TODO! Invalid model tests.
            // TODO! Permission tests.

            using var client = await CreateClientAsUser();

            var timelineName = generator(1);

            async Task AssertMembers(List<UserInfo> members)
            {
                var body = await client.GetTimelineAsync(timelineName);
                body.Members.Should().NotBeNull().And.BeEquivalentTo(members);
            }

            async Task AssertEmptyMembers()
            {
                var body = await client.GetTimelineAsync(timelineName);
                body.Members.Should().NotBeNull().And.BeEmpty();
            }

            await AssertEmptyMembers();
            await client.TestPutAssertErrorAsync($"timelines/{timelineName}/members/usernotexist", errorCode: ErrorCodes.TimelineController.MemberPut_NotExist);
            await AssertEmptyMembers();
            await client.PutTimelineMemberAsync(timelineName, "user2");
            await AssertMembers(new List<UserInfo> { await client.GetUserAsync("user2") });
            await client.DeleteTimelineMemberAsync(timelineName, "user2", true);
            await AssertEmptyMembers();
            await client.DeleteTimelineMemberAsync(timelineName, "aaa", false);
            await AssertEmptyMembers();
        }

        [Theory]
        [MemberData(nameof(TimelineNameGeneratorTestData))]
        public async Task GetPostsAndVisibility_Should_Work(TimelineNameGenerator generator)
        {
            { // default visibility is registered
                {
                    using var client = await CreateDefaultClient();
                    await client.TestGetAssertForbiddenAsync($"timelines/{generator(1)}/posts");
                }

                {
                    using var client = await CreateClientAsUser();
                    await client.TestGetAsync($"timelines/{generator(0)}/posts");
                }
            }

            { // change visibility to public
                {
                    using var client = await CreateClientAsUser();
                    await client.PatchTimelineAsync(generator(1), new() { Visibility = TimelineVisibility.Public });
                }

                {
                    using var client = await CreateDefaultClient();
                    await client.TestGetAsync($"timelines/{generator(1)}/posts");
                }
            }

            { // change visibility to private
                {
                    using var client = await CreateClientAsAdministrator();
                    await client.PatchTimelineAsync(generator(1), new() { Visibility = TimelineVisibility.Private });
                    await client.PatchTimelineAsync(generator(0), new() { Visibility = TimelineVisibility.Private });
                }
                {
                    using var client = await CreateDefaultClient();
                    await client.TestGetAssertForbiddenAsync($"timelines/{generator(1)}/posts");
                }
                { // user can't read admin's
                    using var client = await CreateClientAsUser();
                    await client.TestGetAssertForbiddenAsync($"timelines/{generator(0)}/posts");
                }
                { // admin can read user's
                    using var client = await CreateClientAsAdministrator();
                    await client.TestGetAsync($"timelines/{generator(1)}/posts");
                }
                { // add member
                    using var client = await CreateClientAsAdministrator();
                    await client.PutTimelineMemberAsync(generator(0), "user1");
                }
                { // now user can read admin's
                    using var client = await CreateClientAsUser();
                    await client.TestGetAsync($"timelines/{generator(0)}/posts");
                }
            }
        }

        [Theory]
        [MemberData(nameof(TimelineNameGeneratorTestData))]
        public async Task CreatePostPermission_Should_Work(TimelineNameGenerator generator)
        {
            using (var client = await CreateClientAsUser())
            {
                await client.PutTimelineMemberAsync(generator(1), "user2");
            }

            using (var client = await CreateDefaultClient())
            { // no auth should get 401
                await client.TestPostAssertUnauthorizedAsync($"timelines/{generator(1)}/posts", TimelineHelper.TextPostCreateRequest("aaa"));
            }

            using (var client = await CreateClientAsUser())
            {
                // post self's
                await client.TestPostAsync($"timelines/{generator(1)}/posts", TimelineHelper.TextPostCreateRequest("aaa"));
                // post other not as a member should get 403
                await client.TestPostAssertForbiddenAsync($"timelines/{generator(0)}/posts", TimelineHelper.TextPostCreateRequest("aaa"));
            }

            using (var client = await CreateClientAsAdministrator())
            { // post as admin
                await client.TestPostAsync($"timelines/{generator(1)}/posts", TimelineHelper.TextPostCreateRequest("aaa"));
            }

            using (var client = await CreateClientAs(2))
            { // post as member
                await client.TestPostAsync($"timelines/{generator(1)}/posts", TimelineHelper.TextPostCreateRequest("aaa"));
            }
        }

        [Theory]
        [MemberData(nameof(TimelineNameGeneratorTestData))]
        public async Task DeletePostPermission_Should_Work(TimelineNameGenerator generator)
        {
            async Task<long> CreatePost(int userNumber)
            {
                using var client = await CreateClientAs(userNumber);
                var body = await client.TestPostAsync<TimelinePostInfo>($"timelines/{generator(1)}/posts", TimelineHelper.TextPostCreateRequest("aaa"));
                return body.Id;
            }

            using (var client = await CreateClientAsUser())
            {
                await client.PutTimelineMemberAsync(generator(1), "user2");
                await client.PutTimelineMemberAsync(generator(1), "user3");
            }

            { // no auth should get 401
                using var client = await CreateDefaultClient();
                await client.TestDeleteAssertUnauthorizedAsync($"timelines/{generator(1)}/posts/12");
            }

            { // self can delete self
                var postId = await CreatePost(1);
                using var client = await CreateClientAsUser();
                await client.TestDeleteAsync($"timelines/{generator(1)}/posts/{postId}");
            }

            { // admin can delete any
                var postId = await CreatePost(1);
                using var client = await CreateClientAsAdministrator();
                await client.TestDeleteAsync($"timelines/{generator(1)}/posts/{postId}");
            }

            { // owner can delete other
                var postId = await CreatePost(2);
                using var client = await CreateClientAsUser();
                await client.TestDeleteAsync($"timelines/{generator(1)}/posts/{postId}");
            }

            { // author can delete self
                var postId = await CreatePost(2);
                using var client = await CreateClientAs(2);
                await client.TestDeleteAsync($"timelines/{generator(1)}/posts/{postId}");
            }

            { // otherwise is forbidden
                var postId = await CreatePost(2);
                using var client = await CreateClientAs(3);
                await client.TestDeleteAssertForbiddenAsync($"timelines/{generator(1)}/posts/{postId}");
            }
        }

        [Theory]
        [MemberData(nameof(TimelineNameGeneratorTestData))]
        public async Task TextPost_Should_Work(TimelineNameGenerator generator)
        {
            using var client = await CreateClientAsUser();

            {
                var body = await client.TestGetAsync<List<TimelinePostInfo>>($"timelines/{generator(1)}/posts");
                body.Should().BeEmpty();
            }

            const string mockContent = "aaa";
            TimelinePostInfo createRes;
            {
                var body = await client.TestPostAsync<TimelinePostInfo>($"timelines/{generator(1)}/posts", TimelineHelper.TextPostCreateRequest(mockContent));
                body.Content.Should().BeEquivalentTo(TimelineHelper.TextPostContent(mockContent));
                body.Author.Should().BeEquivalentTo(await client.GetUserAsync("user1"));
                body.Deleted.Should().BeFalse();
                createRes = body;
            }
            {
                var body = await client.TestGetAsync<List<TimelinePostInfo>>($"timelines/{generator(1)}/posts");
                body.Should().BeEquivalentTo(createRes);
            }
            const string mockContent2 = "bbb";
            var mockTime2 = DateTime.UtcNow.AddDays(-1);
            TimelinePostInfo createRes2;
            {
                var body = await client.TestPostAsync<TimelinePostInfo>($"timelines/{generator(1)}/posts", TimelineHelper.TextPostCreateRequest(mockContent2, mockTime2));
                body.Should().NotBeNull();
                body.Content.Should().BeEquivalentTo(TimelineHelper.TextPostContent(mockContent2));
                body.Author.Should().BeEquivalentTo(await client.GetUserAsync("user1"));
                body.Time.Should().BeCloseTo(mockTime2, 1000);
                body.Deleted.Should().BeFalse();
                createRes2 = body;
            }
            {
                var body = await client.TestGetAsync<List<TimelinePostInfo>>($"timelines/{generator(1)}/posts");
                body.Should().BeEquivalentTo(createRes, createRes2);
            }
            {
                await client.TestDeleteAsync($"timelines/{generator(1)}/posts/{createRes.Id}", true);
                await client.TestDeleteAsync($"timelines/{generator(1)}/posts/{createRes.Id}", false);
                await client.TestDeleteAsync($"timelines/{generator(1)}/posts/30000", false);
            }
            {
                var body = await client.TestGetAsync<List<TimelinePostInfo>>($"timelines/{generator(1)}/posts");
                body.Should().BeEquivalentTo(createRes2);
            }
        }

        [Theory]
        [MemberData(nameof(TimelineNameGeneratorTestData))]
        public async Task GetPost_Should_Ordered(TimelineNameGenerator generator)
        {
            using var client = await CreateClientAsUser();

            async Task<long> CreatePost(DateTime time)
            {
                var body = await client.TestPostAsync<TimelinePostInfo>($"timelines/{generator(1)}/posts", TimelineHelper.TextPostCreateRequest("aaa", time));
                return body.Id;
            }

            var now = DateTime.UtcNow;
            var id0 = await CreatePost(now.AddDays(1));
            var id1 = await CreatePost(now.AddDays(-1));
            var id2 = await CreatePost(now);

            {
                var body = await client.TestGetAsync<List<TimelinePostInfo>>($"timelines/{generator(1)}/posts");
                body.Select(p => p.Id).Should().Equal(id1, id2, id0);
            }
        }

        [Theory]
        [MemberData(nameof(TimelineNameGeneratorTestData))]
        public async Task CreatePost_InvalidModel(TimelineNameGenerator generator)
        {
            using var client = await CreateClientAsUser();
            var postUrl = $"timelines/{generator(1)}/posts";
            await client.TestPostAssertInvalidModelAsync(postUrl, new TimelinePostCreateRequest { Content = null! });
            await client.TestPostAssertInvalidModelAsync(postUrl, new TimelinePostCreateRequest { Content = new TimelinePostCreateRequestContent { Type = null! } });
            await client.TestPostAssertInvalidModelAsync(postUrl, new TimelinePostCreateRequest { Content = new TimelinePostCreateRequestContent { Type = "hahaha" } });
            await client.TestPostAssertInvalidModelAsync(postUrl, new TimelinePostCreateRequest { Content = new TimelinePostCreateRequestContent { Type = "text", Text = null } });
            await client.TestPostAssertInvalidModelAsync(postUrl, new TimelinePostCreateRequest { Content = new TimelinePostCreateRequestContent { Type = "image", Data = null } });
            // image not base64
            await client.TestPostAssertInvalidModelAsync(postUrl, new TimelinePostCreateRequest { Content = new TimelinePostCreateRequestContent { Type = "image", Data = "!!!" } });
            // image base64 not image
            await client.TestPostAssertInvalidModelAsync(postUrl, new TimelinePostCreateRequest { Content = new TimelinePostCreateRequestContent { Type = "image", Data = Convert.ToBase64String(new byte[] { 0x01, 0x02, 0x03 }) } });
        }

        [Theory]
        [MemberData(nameof(TimelineNameGeneratorTestData))]
        public async Task ImagePost_ShouldWork(TimelineNameGenerator generator)
        {
            var imageData = ImageHelper.CreatePngWithSize(100, 200);

            long postId;
            string postImageUrl;

            void AssertPostContent(TimelinePostContentInfo content)
            {
                content.Type.Should().Be(TimelinePostContentTypes.Image);
                content.Url.Should().EndWith($"timelines/{generator(1)}/posts/{postId}/data");
                content.Text.Should().Be(null);
            }

            using var client = await CreateClientAsUser();

            {
                var body = await client.TestPostAsync<TimelinePostInfo>($"timelines/{generator(1)}/posts",
                    new TimelinePostCreateRequest
                    {
                        Content = new TimelinePostCreateRequestContent
                        {
                            Type = TimelinePostContentTypes.Image,
                            Data = Convert.ToBase64String(imageData)
                        }
                    });
                postId = body.Id;
                postImageUrl = body.Content!.Url!;
                AssertPostContent(body.Content);
            }

            {
                var body = await client.TestGetAsync<List<TimelinePostInfo>>($"timelines/{generator(1)}/posts");
                body.Should().HaveCount(1);
                var post = body[0];
                post.Id.Should().Be(postId);
                AssertPostContent(post.Content!);
            }

            {
                var res = await client.GetAsync($"timelines/{generator(1)}/posts/{postId}/data");
                res.Content.Headers.ContentType!.MediaType.Should().Be("image/png");
                var data = await res.Content.ReadAsByteArrayAsync();
                var image = Image.Load(data, out var format);
                image.Width.Should().Be(100);
                image.Height.Should().Be(200);
                format.Name.Should().Be(PngFormat.Instance.Name);
            }

            await CacheTestHelper.TestCache(client, $"timelines/{generator(1)}/posts/{postId}/data");
            await client.TestDeleteAsync($"timelines/{generator(1)}/posts/{postId}/data", true);
            await client.TestDeleteAsync($"timelines/{generator(1)}/posts/{postId}/data", false);

            {
                var body = await client.TestGetAsync<List<TimelinePostInfo>>($"timelines/{generator(1)}/posts/{postId}/data");
                body.Should().BeEmpty();
            }

            {
                using var scope = TestApp.Host.Services.CreateScope();
                var database = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
                var count = await database.Data.CountAsync();
                count.Should().Be(0);
            }
        }

        [Theory]
        [MemberData(nameof(TimelineNameGeneratorTestData))]
        public async Task ImagePost_400(TimelineNameGenerator generator)
        {
            using var client = await CreateClientAsUser();

            await client.TestGetAssertErrorAsync($"timelines/{generator(1)}/posts/11234/data", errorCode: ErrorCodes.TimelineController.PostNotExist);

            long postId;
            {
                var body = await client.TestPostAsync<TimelinePostInfo>($"timelines/{generator(1)}/posts", TimelineHelper.TextPostCreateRequest("aaa"));
                postId = body.Id;
            }

            await client.TestGetAssertErrorAsync($"timelines/{generator(1)}/posts/{postId}/data", errorCode: ErrorCodes.TimelineController.PostNoData);
        }

        [Theory]
        [MemberData(nameof(TimelineNameGeneratorTestData))]
        public async Task Timeline_LastModified(TimelineNameGenerator generator)
        {
            using var client = await CreateClientAsUser();

            DateTime lastModified;

            {
                var body = await client.GetTimelineAsync(generator(1));
                lastModified = body.LastModified;
            }

            await Task.Delay(1000);

            {
                var body = await client.PatchTimelineAsync(generator(1), new() { Description = "123" });
                lastModified = body.LastModified.Should().BeAfter(lastModified).And.Subject!.Value;
            }

            {
                var body = await client.GetTimelineAsync(generator(1));
                body.LastModified.Should().Be(lastModified);
            }

            await Task.Delay(1000);

            await client.PutTimelineMemberAsync(generator(1), "user2");

            {
                var body = await client.GetTimelineAsync(generator(1));
                body.LastModified.Should().BeAfter(lastModified);
            }
        }

        [Theory]
        [MemberData(nameof(TimelineNameGeneratorTestData))]
        public async Task Post_ModifiedSince(TimelineNameGenerator generator)
        {
            using var client = await CreateClientAsUser();

            var postContentList = new List<string> { "a", "b", "c", "d" };
            var posts = new List<TimelinePostInfo>();

            foreach (var content in postContentList)
            {
                var post = await client.TestPostAsync<TimelinePostInfo>($"timelines/{generator(1)}/posts",
                    new TimelinePostCreateRequest { Content = new TimelinePostCreateRequestContent { Text = content, Type = TimelinePostContentTypes.Text } });
                posts.Add(post);
                await Task.Delay(1000);
            }

            await client.TestDeleteAsync($"timelines/{generator(1)}/posts/{posts[2].Id}", true);

            {
                var body = await client.TestGetAsync<List<TimelinePostInfo>>($"timelines/{generator(1)}/posts?modifiedSince={posts[1].LastUpdated.ToString("s", CultureInfo.InvariantCulture) }");
                body.Should().HaveCount(2)
                    .And.Subject.Select(p => p.Content!.Text).Should().Equal("b", "d");
            }
        }

        [Theory]
        [MemberData(nameof(TimelineNameGeneratorTestData))]
        public async Task PostList_IncludeDeleted(TimelineNameGenerator generator)
        {
            using var client = await CreateClientAsUser();

            var postContentList = new List<string> { "a", "b", "c", "d" };
            var posts = new List<TimelinePostInfo>();

            foreach (var content in postContentList)
            {
                var body = await client.TestPostAsync<TimelinePostInfo>($"timelines/{generator(1)}/posts",
                    new TimelinePostCreateRequest { Content = new TimelinePostCreateRequestContent { Text = content, Type = TimelinePostContentTypes.Text } });
                posts.Add(body);
            }

            foreach (var id in new long[] { posts[0].Id, posts[2].Id })
            {
                await client.TestDeleteAsync($"timelines/{generator(1)}/posts/{id}", true);
            }

            {
                posts = await client.TestGetAsync<List<TimelinePostInfo>>($"timelines/{generator(1)}/posts?includeDeleted=true");
                posts.Should().HaveCount(4);
                posts.Select(p => p.Deleted).Should().Equal(true, false, true, false);
                posts.Select(p => p.Content == null).Should().Equal(true, false, true, false);
            }
        }

        [Theory]
        [MemberData(nameof(TimelineNameGeneratorTestData))]
        public async Task Post_ModifiedSince_And_IncludeDeleted(TimelineNameGenerator generator)
        {
            using var client = await CreateClientAsUser();

            var postContentList = new List<string> { "a", "b", "c", "d" };
            var posts = new List<TimelinePostInfo>();

            foreach (var (content, index) in postContentList.Select((v, i) => (v, i)))
            {
                var post = await client.TestPostAsync<TimelinePostInfo>($"timelines/{generator(1)}/posts",
                    new TimelinePostCreateRequest { Content = new TimelinePostCreateRequestContent { Text = content, Type = TimelinePostContentTypes.Text } });
                posts.Add(post);
                await Task.Delay(1000);
            }

            await client.TestDeleteAsync($"timelines/{generator(1)}/posts/{posts[2].Id}", true);

            {

                posts = await client.TestGetAsync<List<TimelinePostInfo>>($"timelines/{generator(1)}/posts?modifiedSince={posts[1].LastUpdated.ToString("s", CultureInfo.InvariantCulture)}&includeDeleted=true");
                posts.Should().HaveCount(3);
                posts.Select(p => p.Deleted).Should().Equal(false, true, false);
                posts.Select(p => p.Content == null).Should().Equal(false, true, false);
            }
        }

        [Theory]
        [MemberData(nameof(TimelineNameGeneratorTestData))]
        public async Task Timeline_Get_IfModifiedSince_And_CheckUniqueId(TimelineNameGenerator urlGenerator)
        {
            using var client = await CreateClientAsUser();

            DateTime lastModifiedTime;
            TimelineInfo timeline;
            string uniqueId;

            {
                var body = await client.GetTimelineAsync(urlGenerator(1));
                timeline = body;
                lastModifiedTime = body.LastModified;
                uniqueId = body.UniqueId;
            }

            {
                await client.TestGetAsync($"timelines/{urlGenerator(1)}",
                    expectedStatusCode: HttpStatusCode.NotModified,
                    headerSetup: (headers, _) =>
                    {
                        headers.IfModifiedSince = lastModifiedTime.AddSeconds(1);
                    });
            }

            {

                var body = await client.TestGetAsync<TimelineInfo>($"timelines/{urlGenerator(1)}",
                    expectedStatusCode: HttpStatusCode.NotModified,
                    headerSetup: (headers, _) =>
                    {
                        headers.IfModifiedSince = lastModifiedTime.AddSeconds(-1);
                    });
                body.Should().BeEquivalentTo(timeline);
            }

            {
                await client.TestGetAsync($"timelines/{urlGenerator(1)}?ifModifiedSince={lastModifiedTime.AddSeconds(1).ToString("s", CultureInfo.InvariantCulture) }", expectedStatusCode: HttpStatusCode.NotModified);
            }

            {
                var body = await client.TestGetAsync<TimelineInfo>($"timelines/{urlGenerator(1)}?ifModifiedSince={lastModifiedTime.AddSeconds(-1).ToString("s", CultureInfo.InvariantCulture) }");
                body.Should().BeEquivalentTo(timeline);
            }

            {
                await client.TestGetAsync($"timelines/{urlGenerator(1)}?ifModifiedSince={lastModifiedTime.AddSeconds(1).ToString("s", CultureInfo.InvariantCulture) }&checkUniqueId={uniqueId}", expectedStatusCode: HttpStatusCode.NotModified);
            }

            {
                var testUniqueId = (uniqueId[0] == 'a' ? "b" : "a") + uniqueId[1..];
                var body = await client.TestGetAsync<TimelineInfo>($"timelines/{urlGenerator(1)}?ifModifiedSince={lastModifiedTime.AddSeconds(1).ToString("s", CultureInfo.InvariantCulture) }&checkUniqueId={testUniqueId}", expectedStatusCode: HttpStatusCode.NotModified);
                body.Should().BeEquivalentTo(timeline);
            }
        }

        [Theory]
        [MemberData(nameof(TimelineNameGeneratorTestData))]
        public async Task Title(TimelineNameGenerator generator)
        {
            using var client = await CreateClientAsUser();

            {
                var body = await client.GetTimelineAsync(generator(1));
                body.Title.Should().Be(body.Name);
            }

            {
                var body = await client.PatchTimelineAsync(generator(1), new TimelinePatchRequest { Title = "atitle" });
                body.Title.Should().Be("atitle");
            }

            {
                var body = await client.GetTimelineAsync(generator(1));
                body.Title.Should().Be("atitle");
            }
        }

        [Fact]
        public async Task ChangeName()
        {
            {
                using var client = await CreateDefaultClient();
                await client.TestPostAssertUnauthorizedAsync("timelineop/changename", new TimelineChangeNameRequest { OldName = "t1", NewName = "tttttttt" });
            }

            {
                using var client = await CreateClientAs(2);
                await client.TestPostAssertForbiddenAsync("timelineop/changename", new TimelineChangeNameRequest { OldName = "t1", NewName = "tttttttt" });
            }

            using (var client = await CreateClientAsUser())
            {
                await client.TestPostAssertInvalidModelAsync("timelineop/changename", new TimelineChangeNameRequest { OldName = "!!!", NewName = "tttttttt" });

                await client.TestPostAssertInvalidModelAsync("timelineop/changename", new TimelineChangeNameRequest { OldName = "ttt", NewName = "!!!!" });

                await client.TestPostAssertInvalidModelAsync("timelineop/changename", new TimelineChangeNameRequest { OldName = "ttttt", NewName = "tttttttt" });
                await client.TestPostAssertInvalidModelAsync("timelineop/changename", new TimelineChangeNameRequest { OldName = "t1", NewName = "newt" });

                await client.TestGetAsync("timelines/t1", expectedStatusCode: HttpStatusCode.NotFound);

                {
                    var body = await client.TestGetAsync<TimelineInfo>("timelines/newt");
                    body.Name.Should().Be("newt");
                }
            }
        }

        [Theory]
        [MemberData(nameof(TimelineNameGeneratorTestData))]
        public async Task PostDataETag(TimelineNameGenerator generator)
        {
            using var client = await CreateClientAsUser();

            long id;
            string etag;

            {
                var body = await client.TestPostAsync<TimelinePostInfo>($"timelines/{generator(1)}/posts", new TimelinePostCreateRequest
                {
                    Content = new TimelinePostCreateRequestContent
                    {
                        Type = TimelinePostContentTypes.Image,
                        Data = Convert.ToBase64String(ImageHelper.CreatePngWithSize(100, 50))
                    }
                });
                body.Content!.ETag.Should().NotBeNullOrEmpty();

                id = body.Id;
                etag = body.Content.ETag!;
            }

            {
                var res = await client.GetAsync($"timelines/{generator(1)}/posts{id}/data");
                res.StatusCode.Should().Be(HttpStatusCode.OK);
                res.Headers.ETag.Should().NotBeNull();
                res.Headers.ETag!.ToString().Should().Be(etag);
            }
        }
    }
}
