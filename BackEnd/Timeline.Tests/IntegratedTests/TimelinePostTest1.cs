using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Timeline.Models;
using Timeline.Models.Http;
using Xunit;
using Xunit.Abstractions;

namespace Timeline.Tests.IntegratedTests
{
    public class TimelinePostTest1 : TimelinePostTest
    {
        public TimelinePostTest1(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {

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
        public async Task Post_ModifiedSince(TimelineNameGenerator generator)
        {
            using var client = await CreateClientAsUser();

            var posts = new List<HttpTimelinePost>();

            for (int i = 0; i < 4; i++)
            {
                var post = await client.TestPostAsync<HttpTimelinePost>($"timelines/{generator(1)}/posts", CreateTextPostRequest("a"));
                posts.Add(post);
                await Task.Delay(TimeSpan.FromSeconds(1));
            }

            await client.TestDeleteAsync($"timelines/{generator(1)}/posts/{posts[2].Id}");

            {
                var body = await client.TestGetAsync<List<HttpTimelinePost>>($"timelines/{generator(1)}/posts?modifiedSince={posts[1].LastUpdated.ToString("s", CultureInfo.InvariantCulture) }");
                body.Should().HaveCount(2)
                    .And.Subject.Select(p => p.Id).Should().Equal(posts[1].Id, posts[3].Id);
            }
        }

        [Theory]
        [MemberData(nameof(TimelineNameGeneratorTestData))]
        public async Task PostList_IncludeDeleted(TimelineNameGenerator generator)
        {
            using var client = await CreateClientAsUser();

            var posts = new List<HttpTimelinePost>();

            for (int i = 0; i < 4; i++)
            {
                var body = await client.TestPostAsync<HttpTimelinePost>($"timelines/{generator(1)}/posts", CreateTextPostRequest("a"));
                posts.Add(body);
            }

            foreach (var id in new long[] { posts[0].Id, posts[2].Id })
            {
                await client.TestDeleteAsync($"timelines/{generator(1)}/posts/{id}");
            }

            {
                posts = await client.TestGetAsync<List<HttpTimelinePost>>($"timelines/{generator(1)}/posts?includeDeleted=true");
                posts.Should().HaveCount(4);
                posts.Select(p => p.Deleted).Should().Equal(true, false, true, false);
            }
        }

        [Theory]
        [MemberData(nameof(TimelineNameGeneratorTestData))]
        public async Task Post_ModifiedSince_And_IncludeDeleted(TimelineNameGenerator generator)
        {
            using var client = await CreateClientAsUser();

            var posts = new List<HttpTimelinePost>();

            for (int i = 0; i < 4; i++)
            {
                var post = await client.TestPostAsync<HttpTimelinePost>($"timelines/{generator(1)}/posts", CreateTextPostRequest("a"));
                posts.Add(post);
                await Task.Delay(TimeSpan.FromSeconds(1));
            }

            await client.TestDeleteAsync($"timelines/{generator(1)}/posts/{posts[2].Id}");

            {
                posts = await client.TestGetAsync<List<HttpTimelinePost>>($"timelines/{generator(1)}/posts?modifiedSince={posts[1].LastUpdated.ToString("s", CultureInfo.InvariantCulture)}&includeDeleted=true");
                posts.Should().HaveCount(3);
                posts.Select(p => p.Deleted).Should().Equal(false, true, false);
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
                await client.TestPostAssertUnauthorizedAsync($"timelines/{generator(1)}/posts", CreateTextPostRequest("aaa"));
            }

            using (var client = await CreateClientAsUser())
            {
                // post self's
                await client.TestPostAsync($"timelines/{generator(1)}/posts", CreateTextPostRequest("aaa"));
                // post other not as a member should get 403
                await client.TestPostAssertForbiddenAsync($"timelines/{generator(0)}/posts", CreateTextPostRequest("aaa"));
            }

            using (var client = await CreateClientAsAdministrator())
            { // post as admin
                await client.TestPostAsync($"timelines/{generator(1)}/posts", CreateTextPostRequest("aaa"));
            }

            using (var client = await CreateClientAs(2))
            { // post as member
                await client.TestPostAsync($"timelines/{generator(1)}/posts", CreateTextPostRequest("aaa"));
            }
        }

        [Theory]
        [MemberData(nameof(TimelineNameGeneratorTestData))]
        public async Task DeletePostPermission_Should_Work(TimelineNameGenerator generator)
        {
            async Task<long> CreatePost(int userNumber)
            {
                using var client = await CreateClientAs(userNumber);
                var body = await client.TestPostAsync<HttpTimelinePost>($"timelines/{generator(1)}/posts", CreateTextPostRequest("aaa"));
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
        public async Task GetPost_Should_Ordered(TimelineNameGenerator generator)
        {
            using var client = await CreateClientAsUser();

            async Task<long> CreatePost(DateTime time)
            {
                var body = await client.TestPostAsync<HttpTimelinePost>($"timelines/{generator(1)}/posts", CreateTextPostRequest("aaa", time));
                return body.Id;
            }

            var now = DateTime.UtcNow;
            var id0 = await CreatePost(now.AddDays(1));
            var id1 = await CreatePost(now.AddDays(-1));
            var id2 = await CreatePost(now);

            {
                var body = await client.TestGetAsync<List<HttpTimelinePost>>($"timelines/{generator(1)}/posts");
                body.Select(p => p.Id).Should().Equal(id1, id2, id0);
            }
        }

        [Theory]
        [MemberData(nameof(TimelineNameGeneratorTestData))]
        public async Task Color(TimelineNameGenerator generator)
        {
            using var client = await CreateClientAsUser();

            await client.TestPostAssertInvalidModelAsync($"timelines/{generator(1)}/posts", CreateTextPostRequest("a", color: "aa"));

            long id;

            {
                var post = await client.TestPostAsync<HttpTimelinePost>($"timelines/{generator(1)}/posts",
                    CreateTextPostRequest("a", color: "#aabbcc"));
                post.Color.Should().Be("#aabbcc");
                id = post.Id;
            }

            {
                var post = await client.TestGetAsync<HttpTimelinePost>($"timelines/{generator(1)}/posts/{id}");
                post.Color.Should().Be("#aabbcc");
            }
        }

        [Theory]
        [MemberData(nameof(TimelineNameGeneratorTestData))]
        public async Task GetPost(TimelineNameGenerator generator)
        {
            using var client = await CreateClientAsUser();


            await client.TestGetAssertNotFoundAsync($"timelines/{generator(1)}/posts/1");

            var post = await client.TestPostAsync<HttpTimelinePost>($"timelines/{generator(1)}/posts", CreateTextPostRequest("a"));

            var post2 = await client.TestGetAsync<HttpTimelinePost>($"timelines/{generator(1)}/posts/{post.Id}");
            post2.Should().BeEquivalentTo(post);

            await client.TestDeleteAsync($"timelines/{generator(1)}/posts/{post.Id}");

            await client.TestGetAssertNotFoundAsync($"timelines/{generator(1)}/posts/{post.Id}");
        }

        [Theory]
        [MemberData(nameof(TimelineNameGeneratorTestData))]
        public async Task PatchPost(TimelineNameGenerator generator)
        {
            using var client = await CreateClientAsUser();

            var post = await client.TestPostAsync<HttpTimelinePost>($"timelines/{generator(1)}/posts",
                CreateTextPostRequest("a"));

            var date = new DateTime(2000, 10, 1);

            var post2 = await client.TestPatchAsync<HttpTimelinePost>($"timelines/{generator(1)}/posts/{post.Id}", new HttpTimelinePostPatchRequest
            {
                Time = date,
                Color = "#aabbcc"
            });
            post2.Time.Should().Be(date);
            post2.Color.Should().Be("#aabbcc");

            var post3 = await client.TestGetAsync<HttpTimelinePost>($"timelines/{generator(1)}/posts/{post.Id}");
            post3.Time.Should().Be(date);
            post3.Color.Should().Be("#aabbcc");
        }
    }
}
