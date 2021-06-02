using FluentAssertions;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Timeline.Models;
using Timeline.Models.Http;
using Timeline.Tests.Helpers;
using Xunit;
using Xunit.Abstractions;

namespace Timeline.Tests.IntegratedTests
{
    public class TimelinePostTest2 : TimelinePostTest
    {
        public TimelinePostTest2(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {

        }

        public static IEnumerable<object?[]> CreatePost_InvalidModelTest_TestData()
        {
            var testDataList = new List<List<HttpTimelinePostCreateRequestData>?>()
            {
                null,
                new List<HttpTimelinePostCreateRequestData>(),
                Enumerable.Repeat<HttpTimelinePostCreateRequestData>(new HttpTimelinePostCreateRequestData
                    {
                        ContentType = "text/plain",
                        Data = Convert.ToBase64String(Encoding.UTF8.GetBytes("a"))
                    }, 200).ToList(),
            };

            var testData = new List<HttpTimelinePostCreateRequestData?>()
            {
                null,
                new HttpTimelinePostCreateRequestData
                {
                    ContentType = null!,
                    Data = Convert.ToBase64String(Encoding.UTF8.GetBytes("a"))
                },
                new HttpTimelinePostCreateRequestData
                {
                    ContentType = "text/plain",
                    Data = null!
                },
                new HttpTimelinePostCreateRequestData
                {
                    ContentType = "text/xxxxxxx",
                    Data = Convert.ToBase64String(Encoding.UTF8.GetBytes("a"))
                },
                new HttpTimelinePostCreateRequestData
                {
                    ContentType = "text/plain",
                    Data = "aaa"
                },
                new HttpTimelinePostCreateRequestData
                {
                    ContentType = "text/plain",
                    Data = Convert.ToBase64String(new byte[] {0xE4, 0x1, 0xA0})
                },
                new HttpTimelinePostCreateRequestData
                {
                    ContentType = "image/jpeg",
                    Data = Convert.ToBase64String(ImageHelper.CreatePngWithSize(100, 100))
                },
                new HttpTimelinePostCreateRequestData
                {
                    ContentType = "image/jpeg",
                    Data = Convert.ToBase64String(new byte[] { 100, 200 })
                }

            };

            testDataList.AddRange(testData.Select(d => new List<HttpTimelinePostCreateRequestData>() { d! }));

            return TimelineNameGeneratorTestData().AppendTestData(testDataList);
        }

        [Theory]
        [MemberData(nameof(CreatePost_InvalidModelTest_TestData))]
        public async Task CreatePost_InvalidModel(TimelineNameGenerator generator, List<HttpTimelinePostCreateRequestData> dataList)
        {
            using var client = await CreateClientAsUser();

            await client.TestPostAssertInvalidModelAsync(
                $"timelines/{generator(1)}/posts",
                new HttpTimelinePostCreateRequest
                {
                    DataList = dataList
                }
            );
        }

        public static IEnumerable<object?[]> CreatePost_ShouldWork_TestData()
        {
            var testByteDatas = new List<ByteData>()
            {
                new ByteData(Encoding.UTF8.GetBytes("aaa"), MimeTypes.TextPlain),
                new ByteData(Encoding.UTF8.GetBytes("aaa"), MimeTypes.TextMarkdown),
                new ByteData(ImageHelper.CreateImageWithSize(100, 50, PngFormat.Instance), MimeTypes.ImagePng),
                new ByteData(ImageHelper.CreateImageWithSize(100, 50, JpegFormat.Instance), MimeTypes.ImageJpeg),
                new ByteData(ImageHelper.CreateImageWithSize(100, 50, GifFormat.Instance), MimeTypes.ImageGif),
            };

            return TimelineNameGeneratorTestData().AppendTestData(testByteDatas);
        }

        [Theory]
        [MemberData(nameof(CreatePost_ShouldWork_TestData))]
        public async Task CreatePost_ShouldWork(TimelineNameGenerator generator, ByteData data)
        {
            using var client = await CreateClientAsUser();

            var post = await client.TestPostAsync<HttpTimelinePost>(
                $"timelines/{generator(1)}/posts",
                new HttpTimelinePostCreateRequest
                {
                    DataList = new List<HttpTimelinePostCreateRequestData>
                    {
                        new HttpTimelinePostCreateRequestData
                        {
                            ContentType = data.ContentType,
                            Data = Convert.ToBase64String(data.Data)
                        }
                    }
                }
            );

            post.DataList.Should().NotBeNull().And.HaveCount(1);
            var postData = post.DataList[0];
            postData.Should().NotBeNull();
            var postDataEtag = postData.ETag;
            postDataEtag.Should().NotBeNullOrEmpty();

            {
                var response = await client.GetAsync($"timelines/{generator(1)}/posts/{post.Id}/data");
                response.StatusCode.Should().Be(HttpStatusCode.OK);
                response.Headers.ETag.Should().NotBeNull();
                response.Headers.ETag!.Tag.Should().Be(postDataEtag);
                response.Content.Headers.ContentType.Should().NotBeNull();
                response.Content.Headers.ContentType!.MediaType.Should().Be(data.ContentType);

                var body = await response.Content.ReadAsByteArrayAsync();
                body.Should().Equal(data.Data);
            }

            {
                var response = await client.GetAsync($"timelines/{generator(1)}/posts/{post.Id}/data/0");
                response.StatusCode.Should().Be(HttpStatusCode.OK);
                response.Headers.ETag.Should().NotBeNull();
                response.Headers.ETag!.Tag.Should().Be(postDataEtag);
                response.Content.Headers.ContentType.Should().NotBeNull();
                response.Content.Headers.ContentType!.MediaType.Should().Be(data.ContentType);

                var body = await response.Content.ReadAsByteArrayAsync();
                body.Should().Equal(data.Data);
            }
        }

        [Theory]
        [MemberData(nameof(TimelineNameGeneratorTestData))]
        public async Task CreatePost_MultipleData_ShouldWork(TimelineNameGenerator generator)
        {
            using var client = await CreateClientAsUser();

            var textData = Encoding.UTF8.GetBytes("aaa");
            var imageData = ImageHelper.CreatePngWithSize(100, 50);

            var post = await client.TestPostAsync<HttpTimelinePost>(
                $"timelines/{generator(1)}/posts",
                new HttpTimelinePostCreateRequest
                {
                    DataList = new List<HttpTimelinePostCreateRequestData>
                    {
                        new HttpTimelinePostCreateRequestData
                        {
                            ContentType = MimeTypes.TextMarkdown,
                            Data = Convert.ToBase64String(textData)
                        },
                        new HttpTimelinePostCreateRequestData
                        {
                            ContentType = MimeTypes.ImagePng,
                            Data = Convert.ToBase64String(imageData)
                        }
                    }
                }
            );

            post.DataList.Should().NotBeNull().And.HaveCount(2);

            var postData0 = post.DataList[0];
            postData0.Should().NotBeNull();
            var postDataEtag0 = postData0.ETag;
            postDataEtag0.Should().NotBeNullOrEmpty();

            var postData1 = post.DataList[1];
            postData1.Should().NotBeNull();
            var postDataEtag1 = postData1.ETag;
            postDataEtag1.Should().NotBeNullOrEmpty();

            {
                var response = await client.GetAsync($"timelines/{generator(1)}/posts/{post.Id}/data");
                response.StatusCode.Should().Be(HttpStatusCode.OK);
                response.Headers.ETag.Should().NotBeNull();
                response.Headers.ETag!.Tag.Should().Be(postDataEtag0);
                response.Content.Headers.ContentType.Should().NotBeNull();
                response.Content.Headers.ContentType!.MediaType.Should().Be(MimeTypes.TextMarkdown);

                var body = await response.Content.ReadAsByteArrayAsync();
                body.Should().Equal(textData);
            }

            {
                var response = await client.GetAsync($"timelines/{generator(1)}/posts/{post.Id}/data/0");
                response.StatusCode.Should().Be(HttpStatusCode.OK);
                response.Headers.ETag.Should().NotBeNull();
                response.Headers.ETag!.Tag.Should().Be(postDataEtag0);
                response.Content.Headers.ContentType.Should().NotBeNull();
                response.Content.Headers.ContentType!.MediaType.Should().Be(MimeTypes.TextMarkdown);

                var body = await response.Content.ReadAsByteArrayAsync();
                body.Should().Equal(textData);
            }

            {
                var response = await client.GetAsync($"timelines/{generator(1)}/posts/{post.Id}/data/1");
                response.StatusCode.Should().Be(HttpStatusCode.OK);
                response.Headers.ETag.Should().NotBeNull();
                response.Headers.ETag!.Tag.Should().Be(postDataEtag1);
                response.Content.Headers.ContentType.Should().NotBeNull();
                response.Content.Headers.ContentType!.MediaType.Should().Be(MimeTypes.ImagePng);

                var body = await response.Content.ReadAsByteArrayAsync();
                body.Should().Equal(imageData);
            }
        }

        [Theory]
        [MemberData(nameof(TimelineNameGeneratorTestData))]
        public async Task Post_Editable(TimelineNameGenerator generator)
        {
            HttpTimelinePost post;

            {
                using var client = await CreateClientAsUser();
                post = await client.TestPostAsync<HttpTimelinePost>($"timelines/{generator(1)}/posts", CreateTextPostRequest("a"));

                post.Editable.Should().BeTrue();
            }

            {
                using var client = await CreateClientAs(2);
                var post2 = await client.TestGetAsync<HttpTimelinePost>($"timelines/{generator(1)}/posts/{post.Id}");
                post2.Editable.Should().BeFalse();
            }
        }

        [Theory]
        [MemberData(nameof(TimelineNameGeneratorTestData))]
        public async Task Post_Markdown_Url_Map(TimelineNameGenerator generator)
        {
            using var client = await CreateClientAsUser();
            var post = await client.TestPostAsync<HttpTimelinePost>($"timelines/{generator(1)}/posts", CreateMarkdownPostRequest("[aaa](1) ![bbb](2)"));

            var res = await client.GetAsync($"timelines/{generator(1)}/posts/{post.Id}/data");
            var markdown = await res.Content.ReadAsStringAsync();

            markdown.Should().MatchRegex(@$"\[aaa\]\(https?://.*/timelines/{generator(1)}/posts/{post.Id}/data/1\)");
            markdown.Should().MatchRegex(@$"\[bbb\]\(https?://.*/timelines/{generator(1)}/posts/{post.Id}/data/2\)");
        }

        [Theory]
        [MemberData(nameof(TimelineNameGeneratorTestData))]
        public async Task Post_Markdown_Delete_Test(TimelineNameGenerator generator)
        {
            {
                using var client = await CreateClientAs(2);
                var post = await client.TestPostAsync<HttpTimelinePost>($"timelines/{generator(2)}/posts", CreateMarkdownPostRequest("[aaa](https://crupest.life)"));
                await client.TestDeleteAsync($"timelines/{generator(2)}/posts/{post.Id}");
            }

            {
                using var client = await CreateClientAsUser();
                var post = await client.TestPostAsync<HttpTimelinePost>($"timelines/{generator(1)}/posts", CreateMarkdownPostRequest("[aaa](https://crupest.life)"));
                await client.TestDeleteAsync($"timelines/{generator(1)}/posts/{post.Id}");
            }
        }

        [Theory]
        [MemberData(nameof(TimelineNameGeneratorTestData))]
        public async Task Post_List_Pagination_Test(TimelineNameGenerator generator)
        {
            using var client = await CreateClientAsUser();
            var posts = new List<HttpTimelinePost>();
            for (int i = 0; i < 50; i++)
            {
                var post = await client.TestPostAsync<HttpTimelinePost>($"timelines/{generator(1)}/posts", CreateTextPostRequest(i.ToString()));
                posts.Add(post);
            }

            {
                var p = await client.TestGetAsync<List<HttpTimelinePost>>($"timelines/{generator(1)}/posts?page=2&numberPerPage=10");
                p.Should().BeEquivalentTo(posts.Skip(10).Take(10));
            }
        }

        [Theory]
        [InlineData("")]
        [InlineData("default")]
        public async Task Post_Color_Patch_Default(string value)
        {
            using var client = await CreateClientAsUser();

            var post = await client.TestPostAsync<HttpTimelinePost>("timelines/t1/posts", CreateTextPostRequest("aaa", color: "#111111"));

            var post2 = await client.TestPatchAsync<HttpTimelinePost>($"timelines/t1/posts/{post.Id}", new HttpTimelinePostPatchRequest { Color = value });
            post2.Color.Should().BeNull();
        }
    }
}
