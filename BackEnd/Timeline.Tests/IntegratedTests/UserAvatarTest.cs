using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Timeline.Models.Http;
using Timeline.Services;
using Timeline.Tests.Helpers;
using Xunit;

namespace Timeline.Tests.IntegratedTests
{
    public class UserAvatarTest : IntegratedTestBase
    {
        [Fact]
        public async Task Test()
        {
            Avatar mockAvatar = new Avatar
            {
                Data = ImageHelper.CreatePngWithSize(100, 100),
                Type = PngFormat.Instance.DefaultMimeType
            };

            using (var client = await CreateClientAsUser())
            {
                await client.TestGetAssertNotFoundAsync("users/usernotexist/avatar", errorCode: ErrorCodes.UserCommon.NotExist);

                var env = TestApp.Host.Services.GetRequiredService<IWebHostEnvironment>();
                var defaultAvatarData = await File.ReadAllBytesAsync(Path.Combine(env.ContentRootPath, "default-avatar.png"));

                async Task TestAvatar(string username, byte[] data)
                {
                    var res = await client.GetAsync($"users/{username}/avatar");
                    res.StatusCode.Should().Be(HttpStatusCode.OK);
                    var contentTypeHeader = res.Content.Headers.ContentType;
                    contentTypeHeader.Should().NotBeNull();
                    contentTypeHeader!.MediaType.Should().Be("image/png");
                    var body = await res.Content.ReadAsByteArrayAsync();
                    body.Should().Equal(data);
                }

                await TestAvatar("user1", defaultAvatarData);

                await CacheTestHelper.TestCache(client, "users/user1/avatar");

                await TestAvatar("admin", defaultAvatarData);

                {
                    using var content = new ByteArrayContent(new[] { (byte)0x00 });
                    content.Headers.ContentLength = null;
                    content.Headers.ContentType = new MediaTypeHeaderValue("image/png");
                    await client.TestSendAssertInvalidModelAsync(HttpMethod.Put, "users/user1/avatar", content);
                }

                {
                    using var content = new ByteArrayContent(new[] { (byte)0x00 });
                    content.Headers.ContentLength = 1;
                    await client.TestSendAssertInvalidModelAsync(HttpMethod.Put, "users/user1/avatar", content);
                }

                {
                    using var content = new ByteArrayContent(new[] { (byte)0x00 });
                    content.Headers.ContentLength = 0;
                    content.Headers.ContentType = new MediaTypeHeaderValue("image/png");
                    await client.TestSendAssertInvalidModelAsync(HttpMethod.Put, "users/user1/avatar", content);
                }

                {
                    await client.TestPutByteArrayAsync("users/user1/avatar", new[] { (byte)0x00 }, "image/notaccept", expectedStatusCode: HttpStatusCode.UnsupportedMediaType);
                }

                {
                    using var content = new ByteArrayContent(new[] { (byte)0x00 });
                    content.Headers.ContentLength = 1000 * 1000 * 11;
                    content.Headers.ContentType = new MediaTypeHeaderValue("image/png");
                    await client.TestSendAssertErrorAsync(HttpMethod.Put, "users/user1/avatar", content, errorCode: ErrorCodes.Common.Content.TooBig);
                }

                {
                    using var content = new ByteArrayContent(new[] { (byte)0x00 });
                    content.Headers.ContentLength = 2;
                    content.Headers.ContentType = new MediaTypeHeaderValue("image/png");
                    await client.TestSendAssertInvalidModelAsync(HttpMethod.Put, "users/user1/avatar", content);
                }

                {
                    using var content = new ByteArrayContent(new[] { (byte)0x00, (byte)0x01 });
                    content.Headers.ContentLength = 1;
                    content.Headers.ContentType = new MediaTypeHeaderValue("image/png");
                    await client.TestSendAssertInvalidModelAsync(HttpMethod.Put, "users/user1/avatar", content);

                }

                {
                    await client.TestPutByteArrayAssertErrorAsync("users/user1/avatar", new[] { (byte)0x00 }, "image/png", errorCode: ErrorCodes.UserAvatar.BadFormat_CantDecode);
                    await client.TestPutByteArrayAssertErrorAsync("users/user1/avatar", mockAvatar.Data, "image/png", errorCode: ErrorCodes.UserAvatar.BadFormat_UnmatchedFormat);
                    await client.TestPutByteArrayAssertErrorAsync("users/user1/avatar", ImageHelper.CreatePngWithSize(100, 200), "image/png", errorCode: ErrorCodes.UserAvatar.BadFormat_BadSize);
                }

                {
                    await client.TestPutByteArrayAsync("users/user1/avatar", mockAvatar.Data, mockAvatar.Type);
                    await TestAvatar("user1", mockAvatar.Data);
                }

                IEnumerable<(string, IImageFormat)> formats = new (string, IImageFormat)[]
                {
                    ("image/jpeg", JpegFormat.Instance),
                    ("image/gif", GifFormat.Instance),
                    ("image/png", PngFormat.Instance),
                };

                foreach ((var mimeType, var format) in formats)
                {
                    await client.TestPutByteArrayAsync("users/user1/avatar", ImageHelper.CreateImageWithSize(100, 100, format), mimeType);
                }

                await client.TestPutByteArrayAssertErrorAsync("users/admin/avatar", new[] { (byte)0x00 }, "image/png",
                    expectedStatusCode: HttpStatusCode.Forbidden, errorCode: ErrorCodes.Common.Forbid);

                await client.TestDeleteAssertForbiddenAsync("users/admin/avatar", errorCode: ErrorCodes.Common.Forbid);

                for (int i = 0; i < 2; i++) // double delete should work.
                {
                    await client.TestDeleteAsync("users/user1/avatar");
                    await TestAvatar("user1", defaultAvatarData);
                }
            }

            // Authorization check.
            using (var client = await CreateClientAsAdministrator())
            {
                await client.TestPutByteArrayAsync("users/user1/avatar", mockAvatar.Data, mockAvatar.Type);
                await client.TestDeleteAsync("users/user1/avatar");
                await client.TestPutByteArrayAssertErrorAsync("users/usernotexist/avatar", new[] { (byte)0x00 }, "image/png", errorCode: ErrorCodes.UserCommon.NotExist);
                await client.TestDeleteAssertErrorAsync("users/usernotexist/avatar", errorCode: ErrorCodes.UserCommon.NotExist);
            }

            // bad username check
            using (var client = await CreateClientAsAdministrator())
            {
                await client.TestGetAssertInvalidModelAsync("users/u!ser/avatar");
                await client.TestPutByteArrayAssertInvalidModelAsync("users/u!ser/avatar", ImageHelper.CreatePngWithSize(100, 100), "image/png");
                await client.TestDeleteAssertInvalidModelAsync("users/u!ser/avatar");
            }
        }

        [Fact]
        public async Task AvatarPutReturnETag()
        {
            using var client = await CreateClientAsUser();

            EntityTagHeaderValue? etag;

            {
                var image = ImageHelper.CreatePngWithSize(100, 100);
                var res = await client.TestPutByteArrayAsync("users/user1/avatar", image, PngFormat.Instance.DefaultMimeType);
                etag = res.Headers.ETag;
                etag.Should().NotBeNull();
                etag!.Tag.Should().NotBeNullOrEmpty();
            }

            {
                var res = await client.GetAsync("users/user1/avatar");
                res.StatusCode.Should().Be(HttpStatusCode.OK);
                res.Headers.ETag.Should().Be(etag);
                res.Headers.ETag!.Tag.Should().Be(etag.Tag);
            }
        }
    }
}