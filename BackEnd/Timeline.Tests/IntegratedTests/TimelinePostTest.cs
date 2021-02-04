﻿using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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
        public static HttpTimelinePostContent TextPostContent(string text)
        {
            return new HttpTimelinePostContent("text", text, null, null);
        }

        public static HttpTimelinePostCreateRequest TextPostCreateRequest(string text, DateTime? time = null)
        {
            return new HttpTimelinePostCreateRequest
            {
                Content = new HttpTimelinePostCreateRequestContent
                {
                    Type = "text",
                    Text = text
                },
                Time = time
            };
        }
    }

    public class TimelinePostTest : BaseTimelineTest
    {
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

            var postContentList = new List<string> { "a", "b", "c", "d" };
            var posts = new List<HttpTimelinePost>();

            foreach (var content in postContentList)
            {
                var post = await client.TestPostAsync<HttpTimelinePost>($"timelines/{generator(1)}/posts",
                    new HttpTimelinePostCreateRequest { Content = new HttpTimelinePostCreateRequestContent { Text = content, Type = TimelinePostContentTypes.Text } });
                posts.Add(post);
                await Task.Delay(1000);
            }

            await client.TestDeleteAsync($"timelines/{generator(1)}/posts/{posts[2].Id}");

            {
                var body = await client.TestGetAsync<List<HttpTimelinePost>>($"timelines/{generator(1)}/posts?modifiedSince={posts[1].LastUpdated.ToString("s", CultureInfo.InvariantCulture) }");
                body.Should().HaveCount(2)
                    .And.Subject.Select(p => p.Content!.Text).Should().Equal("b", "d");
            }
        }

        [Theory]
        [MemberData(nameof(TimelineNameGeneratorTestData))]
        public async Task Post_ModifiedSince_And_IncludeDeleted(TimelineNameGenerator generator)
        {
            using var client = await CreateClientAsUser();

            var postContentList = new List<string> { "a", "b", "c", "d" };
            var posts = new List<HttpTimelinePost>();

            foreach (var (content, index) in postContentList.Select((v, i) => (v, i)))
            {
                var post = await client.TestPostAsync<HttpTimelinePost>($"timelines/{generator(1)}/posts",
                    new HttpTimelinePostCreateRequest { Content = new HttpTimelinePostCreateRequestContent { Text = content, Type = TimelinePostContentTypes.Text } });
                posts.Add(post);
                await Task.Delay(1000);
            }

            await client.TestDeleteAsync($"timelines/{generator(1)}/posts/{posts[2].Id}");

            {

                posts = await client.TestGetAsync<List<HttpTimelinePost>>($"timelines/{generator(1)}/posts?modifiedSince={posts[1].LastUpdated.ToString("s", CultureInfo.InvariantCulture)}&includeDeleted=true");
                posts.Should().HaveCount(3);
                posts.Select(p => p.Deleted).Should().Equal(false, true, false);
                posts.Select(p => p.Content == null).Should().Equal(false, true, false);
            }
        }

        [Theory]
        [MemberData(nameof(TimelineNameGeneratorTestData))]
        public async Task PostList_IncludeDeleted(TimelineNameGenerator generator)
        {
            using var client = await CreateClientAsUser();

            var postContentList = new List<string> { "a", "b", "c", "d" };
            var posts = new List<HttpTimelinePost>();

            foreach (var content in postContentList)
            {
                var body = await client.TestPostAsync<HttpTimelinePost>($"timelines/{generator(1)}/posts",
                    new HttpTimelinePostCreateRequest { Content = new HttpTimelinePostCreateRequestContent { Text = content, Type = TimelinePostContentTypes.Text } });
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
                posts.Select(p => p.Content == null).Should().Equal(true, false, true, false);
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
                var body = await client.TestPostAsync<HttpTimelinePost>($"timelines/{generator(1)}/posts", TimelineHelper.TextPostCreateRequest("aaa"));
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
                var body = await client.TestGetAsync<List<HttpTimelinePost>>($"timelines/{generator(1)}/posts");
                body.Should().BeEmpty();
            }

            const string mockContent = "aaa";
            HttpTimelinePost createRes;
            {
                var body = await client.TestPostAsync<HttpTimelinePost>($"timelines/{generator(1)}/posts", TimelineHelper.TextPostCreateRequest(mockContent));
                body.Content.Should().BeEquivalentTo(TimelineHelper.TextPostContent(mockContent));
                body.Author.Should().BeEquivalentTo(await client.GetUserAsync("user1"));
                body.Deleted.Should().BeFalse();
                createRes = body;
            }
            {
                var body = await client.TestGetAsync<List<HttpTimelinePost>>($"timelines/{generator(1)}/posts");
                body.Should().BeEquivalentTo(createRes);
            }
            const string mockContent2 = "bbb";
            var mockTime2 = DateTime.UtcNow.AddDays(-1);
            HttpTimelinePost createRes2;
            {
                var body = await client.TestPostAsync<HttpTimelinePost>($"timelines/{generator(1)}/posts", TimelineHelper.TextPostCreateRequest(mockContent2, mockTime2));
                body.Should().NotBeNull();
                body.Content.Should().BeEquivalentTo(TimelineHelper.TextPostContent(mockContent2));
                body.Author.Should().BeEquivalentTo(await client.GetUserAsync("user1"));
                body.Time.Should().BeCloseTo(mockTime2, 1000);
                body.Deleted.Should().BeFalse();
                createRes2 = body;
            }
            {
                var body = await client.TestGetAsync<List<HttpTimelinePost>>($"timelines/{generator(1)}/posts");
                body.Should().BeEquivalentTo(createRes, createRes2);
            }
            {
                await client.TestDeleteAsync($"timelines/{generator(1)}/posts/{createRes.Id}");
                await client.TestDeleteAssertErrorAsync($"timelines/{generator(1)}/posts/{createRes.Id}");
                await client.TestDeleteAssertErrorAsync($"timelines/{generator(1)}/posts/30000");
            }
            {
                var body = await client.TestGetAsync<List<HttpTimelinePost>>($"timelines/{generator(1)}/posts");
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
                var body = await client.TestPostAsync<HttpTimelinePost>($"timelines/{generator(1)}/posts", TimelineHelper.TextPostCreateRequest("aaa", time));
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
        public async Task CreatePost_InvalidModel(TimelineNameGenerator generator)
        {
            using var client = await CreateClientAsUser();
            var postUrl = $"timelines/{generator(1)}/posts";
            await client.TestPostAssertInvalidModelAsync(postUrl, new HttpTimelinePostCreateRequest { Content = null! });
            await client.TestPostAssertInvalidModelAsync(postUrl, new HttpTimelinePostCreateRequest { Content = new HttpTimelinePostCreateRequestContent { Type = null! } });
            await client.TestPostAssertInvalidModelAsync(postUrl, new HttpTimelinePostCreateRequest { Content = new HttpTimelinePostCreateRequestContent { Type = "hahaha" } });
            await client.TestPostAssertInvalidModelAsync(postUrl, new HttpTimelinePostCreateRequest { Content = new HttpTimelinePostCreateRequestContent { Type = "text", Text = null } });
            await client.TestPostAssertInvalidModelAsync(postUrl, new HttpTimelinePostCreateRequest { Content = new HttpTimelinePostCreateRequestContent { Type = "image", Data = null } });
            // image not base64
            await client.TestPostAssertInvalidModelAsync(postUrl, new HttpTimelinePostCreateRequest { Content = new HttpTimelinePostCreateRequestContent { Type = "image", Data = "!!!" } });
            // image base64 not image
            await client.TestPostAssertInvalidModelAsync(postUrl, new HttpTimelinePostCreateRequest { Content = new HttpTimelinePostCreateRequestContent { Type = "image", Data = Convert.ToBase64String(new byte[] { 0x01, 0x02, 0x03 }) } });
        }

        [Theory]
        [MemberData(nameof(TimelineNameGeneratorTestData))]
        public async Task ImagePost_ShouldWork(TimelineNameGenerator generator)
        {
            var imageData = ImageHelper.CreatePngWithSize(100, 200);

            long postId;
            string postImageUrl;

            void AssertPostContent(HttpTimelinePostContent content)
            {
                content.Type.Should().Be(TimelinePostContentTypes.Image);
                content.Url.Should().EndWith($"timelines/{generator(1)}/posts/{postId}/data");
                content.Text.Should().Be(null);
            }

            using var client = await CreateClientAsUser();

            {
                var body = await client.TestPostAsync<HttpTimelinePost>($"timelines/{generator(1)}/posts",
                    new HttpTimelinePostCreateRequest
                    {
                        Content = new HttpTimelinePostCreateRequestContent
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
                var body = await client.TestGetAsync<List<HttpTimelinePost>>($"timelines/{generator(1)}/posts");
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
            await client.TestDeleteAsync($"timelines/{generator(1)}/posts/{postId}");
            await client.TestDeleteAssertErrorAsync($"timelines/{generator(1)}/posts/{postId}");

            {
                var body = await client.TestGetAsync<List<HttpTimelinePost>>($"timelines/{generator(1)}/posts");
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

            await client.TestGetAssertNotFoundAsync($"timelines/{generator(1)}/posts/11234/data", errorCode: ErrorCodes.TimelineController.PostNotExist);

            long postId;
            {
                var body = await client.TestPostAsync<HttpTimelinePost>($"timelines/{generator(1)}/posts", TimelineHelper.TextPostCreateRequest("aaa"));
                postId = body.Id;
            }

            await client.TestGetAssertErrorAsync($"timelines/{generator(1)}/posts/{postId}/data", errorCode: ErrorCodes.TimelineController.PostNoData);
        }

        [Theory]
        [MemberData(nameof(TimelineNameGeneratorTestData))]
        public async Task PostDataETag(TimelineNameGenerator generator)
        {
            using var client = await CreateClientAsUser();

            long id;
            string etag;

            {
                var body = await client.TestPostAsync<HttpTimelinePost>($"timelines/{generator(1)}/posts", new HttpTimelinePostCreateRequest
                {
                    Content = new HttpTimelinePostCreateRequestContent
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
                var res = await client.GetAsync($"timelines/{generator(1)}/posts/{id}/data");
                res.StatusCode.Should().Be(200);
                res.Headers.ETag.Should().NotBeNull();
                res.Headers.ETag!.ToString().Should().Be(etag);
            }
        }

        [Theory]
        [MemberData(nameof(TimelineNameGeneratorTestData))]
        public async Task Color(TimelineNameGenerator generator)
        {
            using var client = await CreateClientAsUser();

            HttpTimelinePostCreateRequestContent CreateRequestContent() => new()
            {
                Type = "text",
                Text = "aaa"
            };

            await client.TestPostAssertInvalidModelAsync($"timelines/{generator(1)}/posts", new HttpTimelinePostCreateRequest
            {
                Content = CreateRequestContent(),
                Color = "#1"
            });

            {
                var post = await client.TestPostAsync<HttpTimelinePost>($"timelines/{generator(1)}/posts", new HttpTimelinePostCreateRequest
                {
                    Content = CreateRequestContent(),
                    Color = "#aabbcc"
                });
                post.Color.Should().Be("#aabbcc");
            }
        }
    }
}
