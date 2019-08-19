﻿using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using SixLabors.ImageSharp.Formats.Png;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Timeline.Controllers;
using Timeline.Models.Http;
using Timeline.Services;
using Timeline.Tests.Helpers;
using Timeline.Tests.Helpers.Authentication;
using Xunit;
using Xunit.Abstractions;

namespace Timeline.Tests.IntegratedTests
{
    public class UserAvatarUnitTest : IClassFixture<MyWebApplicationFactory<Startup>>, IDisposable
    {
        private readonly WebApplicationFactory<Startup> _factory;
        private readonly Action _disposeAction;

        public UserAvatarUnitTest(MyWebApplicationFactory<Startup> factory, ITestOutputHelper outputHelper)
        {
            _factory = factory.WithTestConfig(outputHelper, out _disposeAction);
        }

        public void Dispose()
        {
            _disposeAction();
        }

        [Fact]
        public async Task Test()
        {
            Avatar mockAvatar = new Avatar
            {
                Data = ImageHelper.CreatePngWithSize(100, 100),
                Type = PngFormat.Instance.DefaultMimeType
            };

            using (var client = await _factory.CreateClientAsUser())
            {
                {
                    var res = await client.GetAsync("users/usernotexist/avatar");
                    res.Should().HaveStatusCodeNotFound()
                        .And.Should().HaveBodyAsCommonResponseWithCode(UserAvatarController.ErrorCodes.Get_UserNotExist);
                }

                var env = _factory.Server.Host.Services.GetRequiredService<IHostingEnvironment>();
                var defaultAvatarData = await File.ReadAllBytesAsync(Path.Combine(env.ContentRootPath, "default-avatar.png"));

                async Task GetReturnDefault(string username = "user")
                {
                    var res = await client.GetAsync($"users/{username}/avatar");
                    res.Should().HaveStatusCodeOk();
                    res.Content.Headers.ContentType.MediaType.Should().Be("image/png");
                    var body = await res.Content.ReadAsByteArrayAsync();
                    body.Should().Equal(defaultAvatarData);
                }

                await GetReturnDefault();
                await GetReturnDefault("admin");

                {
                    var request = new HttpRequestMessage()
                    {
                        RequestUri = new Uri(client.BaseAddress, "users/user/avatar"),
                        Method = HttpMethod.Get,
                    };
                    request.Headers.Add("If-Modified-Since", DateTime.Now.ToString("r"));
                    var res = await client.SendAsync(request);
                    res.Should().HaveStatusCode(HttpStatusCode.NotModified);
                }

                {
                    var content = new ByteArrayContent(new[] { (byte)0x00 });
                    content.Headers.ContentType = new MediaTypeHeaderValue("image/png");
                    var res = await client.PutAsync("users/user/avatar", content);
                    res.Should().HaveStatusCode(HttpStatusCode.BadRequest)
                        .And.Should().HaveBodyAsCommonResponseWithCode(CommonResponse.ErrorCodes.Header_Missing_ContentLength);
                }

                {
                    var content = new ByteArrayContent(new[] { (byte)0x00 });
                    content.Headers.ContentLength = 1;
                    var res = await client.PutAsync("users/user/avatar", content);
                    res.Should().HaveStatusCode(HttpStatusCode.BadRequest)
                        .And.Should().HaveBodyAsCommonResponseWithCode(CommonResponse.ErrorCodes.Header_Missing_ContentType);
                }

                {
                    var content = new ByteArrayContent(new[] { (byte)0x00 });
                    content.Headers.ContentLength = 0;
                    content.Headers.ContentType = new MediaTypeHeaderValue("image/png");
                    var res = await client.PutAsync("users/user/avatar", content);
                    res.Should().HaveStatusCode(HttpStatusCode.BadRequest)
                        .And.Should().HaveBodyAsCommonResponseWithCode(CommonResponse.ErrorCodes.Header_Zero_ContentLength);
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
                        .And.Should().HaveBodyAsCommonResponseWithCode(UserAvatarController.ErrorCodes.Put_Content_TooBig);
                }

                {
                    var content = new ByteArrayContent(new[] { (byte)0x00 });
                    content.Headers.ContentLength = 2;
                    content.Headers.ContentType = new MediaTypeHeaderValue("image/png");
                    var res = await client.PutAsync("users/user/avatar", content);
                    res.Should().HaveStatusCode(HttpStatusCode.BadRequest)
                        .And.Should().HaveBodyAsCommonResponseWithCode(UserAvatarController.ErrorCodes.Put_Content_UnmatchedLength_Less);
                }

                {
                    var content = new ByteArrayContent(new[] { (byte)0x00, (byte)0x01 });
                    content.Headers.ContentLength = 1;
                    content.Headers.ContentType = new MediaTypeHeaderValue("image/png");
                    var res = await client.PutAsync("users/user/avatar", content);
                    res.Should().HaveStatusCode(HttpStatusCode.BadRequest)
                        .And.Should().HaveBodyAsCommonResponseWithCode(UserAvatarController.ErrorCodes.Put_Content_UnmatchedLength_Bigger);
                }

                {
                    var res = await client.PutByteArrayAsync("users/user/avatar", new[] { (byte)0x00 }, "image/png");
                    res.Should().HaveStatusCode(HttpStatusCode.BadRequest)
                        .And.Should().HaveBodyAsCommonResponseWithCode(UserAvatarController.ErrorCodes.Put_BadFormat_CantDecode);
                }

                {
                    var res = await client.PutByteArrayAsync("users/user/avatar", mockAvatar.Data, "image/jpeg");
                    res.Should().HaveStatusCode(HttpStatusCode.BadRequest)
                        .And.Should().HaveBodyAsCommonResponseWithCode(UserAvatarController.ErrorCodes.Put_BadFormat_UnmatchedFormat);
                }

                {
                    var res = await client.PutByteArrayAsync("users/user/avatar", ImageHelper.CreatePngWithSize(100, 200), "image/png");
                    res.Should().HaveStatusCode(HttpStatusCode.BadRequest)
                        .And.Should().HaveBodyAsCommonResponseWithCode(UserAvatarController.ErrorCodes.Put_BadFormat_BadSize);
                }

                {
                    var res = await client.PutByteArrayAsync("users/user/avatar", mockAvatar.Data, mockAvatar.Type);
                    res.Should().HaveStatusCode(HttpStatusCode.OK);

                    var res2 = await client.GetAsync("users/user/avatar");
                    res2.Should().HaveStatusCodeOk();
                    res2.Content.Headers.ContentType.MediaType.Should().Be(mockAvatar.Type);
                    var body = await res2.Content.ReadAsByteArrayAsync();
                    body.Should().Equal(mockAvatar.Data);
                }

                {
                    var res = await client.PutByteArrayAsync("users/admin/avatar", new[] { (byte)0x00 }, "image/png");
                    res.Should().HaveStatusCode(HttpStatusCode.Forbidden)
                        .And.Should().HaveBodyAsCommonResponseWithCode(UserAvatarController.ErrorCodes.Put_Forbid);
                }

                {
                    var res = await client.DeleteAsync("users/admin/avatar");
                    res.Should().HaveStatusCode(HttpStatusCode.Forbidden)
                        .And.Should().HaveBodyAsCommonResponseWithCode(UserAvatarController.ErrorCodes.Delete_Forbid);
                }

                for (int i = 0; i < 2; i++) // double delete should work.
                {
                    var res = await client.DeleteAsync("users/user/avatar");
                    res.Should().HaveStatusCodeOk();
                    await GetReturnDefault();
                }
            }

            // Authorization check.
            using (var client = await _factory.CreateClientAsAdmin())
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
                    res.Should().HaveStatusCodeBadRequest()
                        .And.Should().HaveBodyAsCommonResponseWithCode(UserAvatarController.ErrorCodes.Put_UserNotExist);
                }

                {
                    var res = await client.DeleteAsync("users/usernotexist/avatar");
                    res.Should().HaveStatusCodeBadRequest()
                        .And.Should().HaveBodyAsCommonResponseWithCode(UserAvatarController.ErrorCodes.Delete_UserNotExist);
                }
            }
        }
    }
}