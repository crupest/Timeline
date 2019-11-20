﻿using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Timeline.Services;
using Timeline.Tests.Helpers;
using Xunit;
using static Timeline.ErrorCodes.Http.Common;
using static Timeline.ErrorCodes.Http.UserAvatar;

namespace Timeline.Tests.IntegratedTests
{
    public class UserAvatarTest : IntegratedTestBase
    {
        public UserAvatarTest(WebApplicationFactory<Startup> factory)
            : base(factory)
        {

        }

        [Fact]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "HttpMessageRequest should be disposed ???")]
        public async Task Test()
        {
            Avatar mockAvatar = new Avatar
            {
                Data = ImageHelper.CreatePngWithSize(100, 100),
                Type = PngFormat.Instance.DefaultMimeType
            };

            using (var client = await CreateClientAsUser())
            {
                {
                    var res = await client.GetAsync("users/usernotexist/avatar");
                    res.Should().HaveStatusCode(404)
                        .And.HaveCommonBody()
                        .Which.Code.Should().Be(Get.UserNotExist);
                }

                var env = Factory.Server.Host.Services.GetRequiredService<IWebHostEnvironment>();
                var defaultAvatarData = await File.ReadAllBytesAsync(Path.Combine(env.ContentRootPath, "default-avatar.png"));

                async Task GetReturnDefault(string username = "user")
                {
                    var res = await client.GetAsync($"users/{username}/avatar");
                    res.Should().HaveStatusCode(200);
                    res.Content.Headers.ContentType.MediaType.Should().Be("image/png");
                    var body = await res.Content.ReadAsByteArrayAsync();
                    body.Should().Equal(defaultAvatarData);
                }

                EntityTagHeaderValue eTag;
                {
                    var res = await client.GetAsync($"users/user/avatar");
                    res.Should().HaveStatusCode(200);
                    res.Content.Headers.ContentType.MediaType.Should().Be("image/png");
                    var body = await res.Content.ReadAsByteArrayAsync();
                    body.Should().Equal(defaultAvatarData);
                    var cacheControl = res.Headers.CacheControl;
                    cacheControl.NoCache.Should().BeTrue();
                    cacheControl.NoStore.Should().BeFalse();
                    cacheControl.MaxAge.Should().NotBeNull().And.Be(TimeSpan.Zero);
                    eTag = res.Headers.ETag;
                }

                await GetReturnDefault("admin");

                {
                    var request = new HttpRequestMessage()
                    {
                        RequestUri = new Uri(client.BaseAddress, "users/user/avatar"),
                        Method = HttpMethod.Get,
                    };
                    request.Headers.TryAddWithoutValidation("If-None-Match", "\"dsdfd");
                    var res = await client.SendAsync(request);
                    res.Should().HaveStatusCode(HttpStatusCode.BadRequest)
                        .And.HaveCommonBody().Which.Code.Should().Be(Header.IfNonMatch.BadFormat);
                }

                {
                    var request = new HttpRequestMessage()
                    {
                        RequestUri = new Uri(client.BaseAddress, "users/user/avatar"),
                        Method = HttpMethod.Get,
                    };
                    request.Headers.TryAddWithoutValidation("If-None-Match", "\"aaa\"");
                    var res = await client.SendAsync(request);
                    res.Should().HaveStatusCode(HttpStatusCode.OK);
                }

                {
                    var request = new HttpRequestMessage()
                    {
                        RequestUri = new Uri(client.BaseAddress, "users/user/avatar"),
                        Method = HttpMethod.Get,
                    };
                    request.Headers.Add("If-None-Match", eTag.ToString());
                    var res = await client.SendAsync(request);
                    res.Should().HaveStatusCode(HttpStatusCode.NotModified);
                }

                {
                    var content = new ByteArrayContent(new[] { (byte)0x00 });
                    content.Headers.ContentType = new MediaTypeHeaderValue("image/png");
                    var res = await client.PutAsync("users/user/avatar", content);
                    res.Should().HaveStatusCode(HttpStatusCode.BadRequest)
                        .And.HaveCommonBody().Which.Code.Should().Be(ErrorCodes.Http.Filter.Header.ContentLength.Missing); ;
                }

                {
                    var content = new ByteArrayContent(new[] { (byte)0x00 });
                    content.Headers.ContentLength = 1;
                    var res = await client.PutAsync("users/user/avatar", content);
                    res.Should().HaveStatusCode(HttpStatusCode.BadRequest)
                        .And.HaveCommonBody().Which.Code.Should().Be(ErrorCodes.Http.Filter.Header.ContentType.Missing);
                }

                {
                    var content = new ByteArrayContent(new[] { (byte)0x00 });
                    content.Headers.ContentLength = 0;
                    content.Headers.ContentType = new MediaTypeHeaderValue("image/png");
                    var res = await client.PutAsync("users/user/avatar", content);
                    res.Should().HaveStatusCode(HttpStatusCode.BadRequest)
                        .And.HaveCommonBody().Which.Code.Should().Be(ErrorCodes.Http.Filter.Header.ContentLength.Zero);
                }

                {
                    var res = await client.PutByteArrayAsync("users/user/avatar", new[] { (byte)0x00 }, "image/notaccept");
                    res.Should().HaveStatusCode(HttpStatusCode.UnsupportedMediaType);
                }

                {
                    var content = new ByteArrayContent(new[] { (byte)0x00 });
                    content.Headers.ContentLength = 1000 * 1000 * 11;
                    content.Headers.ContentType = new MediaTypeHeaderValue("image/png");
                    var res = await client.PutAsync("users/user/avatar", content);
                    res.Should().HaveStatusCode(HttpStatusCode.BadRequest)
                        .And.HaveCommonBody().Which.Code.Should().Be(Content.TooBig);
                }

                {
                    var content = new ByteArrayContent(new[] { (byte)0x00 });
                    content.Headers.ContentLength = 2;
                    content.Headers.ContentType = new MediaTypeHeaderValue("image/png");
                    var res = await client.PutAsync("users/user/avatar", content);
                    res.Should().HaveStatusCode(HttpStatusCode.BadRequest)
                        .And.HaveCommonBody().Which.Code.Should().Be(Content.UnmatchedLength_Smaller);
                }

                {
                    var content = new ByteArrayContent(new[] { (byte)0x00, (byte)0x01 });
                    content.Headers.ContentLength = 1;
                    content.Headers.ContentType = new MediaTypeHeaderValue("image/png");
                    var res = await client.PutAsync("users/user/avatar", content);
                    res.Should().HaveStatusCode(HttpStatusCode.BadRequest)
                        .And.HaveCommonBody().Which.Code.Should().Be(Content.UnmatchedLength_Bigger);
                }

                {
                    var res = await client.PutByteArrayAsync("users/user/avatar", new[] { (byte)0x00 }, "image/png");
                    res.Should().HaveStatusCode(HttpStatusCode.BadRequest)
                        .And.HaveCommonBody().Which.Code.Should().Be(Put.BadFormat_CantDecode);
                }

                {
                    var res = await client.PutByteArrayAsync("users/user/avatar", mockAvatar.Data, "image/jpeg");
                    res.Should().HaveStatusCode(HttpStatusCode.BadRequest)
                        .And.HaveCommonBody().Which.Code.Should().Be(Put.BadFormat_UnmatchedFormat);
                }

                {
                    var res = await client.PutByteArrayAsync("users/user/avatar", ImageHelper.CreatePngWithSize(100, 200), "image/png");
                    res.Should().HaveStatusCode(HttpStatusCode.BadRequest)
                        .And.HaveCommonBody().Which.Code.Should().Be(Put.BadFormat_BadSize);
                }

                {
                    var res = await client.PutByteArrayAsync("users/user/avatar", mockAvatar.Data, mockAvatar.Type);
                    res.Should().HaveStatusCode(HttpStatusCode.OK);

                    var res2 = await client.GetAsync("users/user/avatar");
                    res2.Should().HaveStatusCode(200);
                    res2.Content.Headers.ContentType.MediaType.Should().Be(mockAvatar.Type);
                    var body = await res2.Content.ReadAsByteArrayAsync();
                    body.Should().Equal(mockAvatar.Data);
                }

                IEnumerable<(string, IImageFormat)> formats = new (string, IImageFormat)[]
                {
                    ("image/jpeg", JpegFormat.Instance),
                    ("image/gif", GifFormat.Instance),
                    ("image/png", PngFormat.Instance),
                };

                foreach ((var mimeType, var format) in formats)
                {
                    var res = await client.PutByteArrayAsync("users/user/avatar", ImageHelper.CreateImageWithSize(100, 100, format), mimeType);
                    res.Should().HaveStatusCode(HttpStatusCode.OK);
                }

                {
                    var res = await client.PutByteArrayAsync("users/admin/avatar", new[] { (byte)0x00 }, "image/png");
                    res.Should().HaveStatusCode(HttpStatusCode.Forbidden)
                        .And.HaveCommonBody().Which.Code.Should().Be(Put.Forbid);
                }

                {
                    var res = await client.DeleteAsync("users/admin/avatar");
                    res.Should().HaveStatusCode(HttpStatusCode.Forbidden)
                        .And.HaveCommonBody().Which.Code.Should().Be(Delete.Forbid);
                }

                for (int i = 0; i < 2; i++) // double delete should work.
                {
                    var res = await client.DeleteAsync("users/user/avatar");
                    res.Should().HaveStatusCode(200);
                    await GetReturnDefault();
                }
            }

            // Authorization check.
            using (var client = await CreateClientAsAdmin())
            {
                {
                    var res = await client.PutByteArrayAsync("users/user/avatar", mockAvatar.Data, mockAvatar.Type);
                    res.Should().HaveStatusCode(HttpStatusCode.OK);
                }

                {
                    var res = await client.DeleteAsync("users/user/avatar");
                    res.Should().HaveStatusCode(HttpStatusCode.OK);
                }

                {
                    var res = await client.PutByteArrayAsync("users/usernotexist/avatar", new[] { (byte)0x00 }, "image/png");
                    res.Should().HaveStatusCode(400)
                        .And.HaveCommonBody()
                        .Which.Code.Should().Be(Put.UserNotExist);
                }

                {
                    var res = await client.DeleteAsync("users/usernotexist/avatar");
                    res.Should().HaveStatusCode(400)
                        .And.HaveCommonBody().Which.Code.Should().Be(Delete.UserNotExist);
                }
            }

            // bad username check
            using (var client = await CreateClientAsAdmin())
            {
                {
                    var res = await client.GetAsync("users/u!ser/avatar");
                    res.Should().BeInvalidModel();
                }

                {
                    var res = await client.PutByteArrayAsync("users/u!ser/avatar", ImageHelper.CreatePngWithSize(100, 100), "image/png");
                    res.Should().BeInvalidModel();
                }

                {
                    var res = await client.DeleteAsync("users/u!ser/avatar");
                    res.Should().BeInvalidModel();
                }
            }
        }
    }
}