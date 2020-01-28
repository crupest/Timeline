using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Timeline.Controllers;
using Timeline.Models;
using Timeline.Models.Http;
using Timeline.Services;
using Timeline.Tests.Helpers;
using Xunit;

namespace Timeline.Tests.Controllers
{
    public class TokenControllerTest : IDisposable
    {
        private readonly Mock<IUserTokenManager> _mockUserService = new Mock<IUserTokenManager>();
        private readonly TestClock _mockClock = new TestClock();


        private readonly TokenController _controller;

        public TokenControllerTest()
        {
            _controller = new TokenController(_mockUserService.Object, NullLogger<TokenController>.Instance, _mockClock);
        }

        public void Dispose()
        {
            _controller.Dispose();
        }

        [Theory]
        [InlineData(null)]
        [InlineData(100)]
        public async Task Create_Ok(int? expire)
        {
            var mockCurrentTime = DateTime.Now;
            _mockClock.MockCurrentTime = mockCurrentTime;
            var mockCreateResult = new UserTokenCreateResult
            {
                Token = "mocktokenaaaaa",
                User = new Models.User
                {
                    Id = 1,
                    Username = MockUser.User.Username,
                    Administrator = MockUser.User.Administrator,
                    Version = 1
                }
            };
            _mockUserService.Setup(s => s.CreateToken("u", "p", expire == null ? null : (DateTime?)mockCurrentTime.AddDays(expire.Value))).ReturnsAsync(mockCreateResult);
            var action = await _controller.Create(new CreateTokenRequest
            {
                Username = "u",
                Password = "p",
                Expire = expire
            });
            action.Result.Should().BeAssignableTo<OkObjectResult>()
                .Which.Value.Should().BeEquivalentTo(new CreateTokenResponse
                {
                    Token = mockCreateResult.Token,
                    User = MockUser.User.Info
                });
        }

        [Fact]
        public async Task Create_UserNotExist()
        {
            _mockUserService.Setup(s => s.CreateToken("u", "p", null)).ThrowsAsync(new UserNotExistException("u"));
            var action = await _controller.Create(new CreateTokenRequest
            {
                Username = "u",
                Password = "p",
                Expire = null
            });
            action.Result.Should().BeAssignableTo<BadRequestObjectResult>()
                .Which.Value.Should().BeAssignableTo<CommonResponse>()
                .Which.Code.Should().Be(ErrorCodes.TokenController.Create_BadCredential);
        }

        [Fact]
        public async Task Create_BadPassword()
        {
            _mockUserService.Setup(s => s.CreateToken("u", "p", null)).ThrowsAsync(new BadPasswordException("u"));
            var action = await _controller.Create(new CreateTokenRequest
            {
                Username = "u",
                Password = "p",
                Expire = null
            });
            action.Result.Should().BeAssignableTo<BadRequestObjectResult>()
                .Which.Value.Should().BeAssignableTo<CommonResponse>()
                .Which.Code.Should().Be(ErrorCodes.TokenController.Create_BadCredential);
        }

        [Fact]
        public async Task Verify_Ok()
        {
            const string token = "aaaaaaaaaaaaaa";
            _mockUserService.Setup(s => s.VerifyToken(token)).ReturnsAsync(new Models.User
            {
                Id = 1,
                Username = MockUser.User.Username,
                Administrator = MockUser.User.Administrator,
                Version = 1
            });
            var action = await _controller.Verify(new VerifyTokenRequest { Token = token });
            action.Result.Should().BeAssignableTo<OkObjectResult>()
                .Which.Value.Should().BeAssignableTo<VerifyTokenResponse>()
                .Which.User.Should().BeEquivalentTo(MockUser.User.Info);
        }

        public static IEnumerable<object[]> Verify_BadRequest_Data()
        {
            yield return new object[] { new UserTokenTimeExpireException(), ErrorCodes.TokenController.Verify_TimeExpired };
            yield return new object[] { new UserTokenBadVersionException(), ErrorCodes.TokenController.Verify_OldVersion };
            yield return new object[] { new UserTokenBadFormatException(), ErrorCodes.TokenController.Verify_BadFormat };
            yield return new object[] { new UserNotExistException(), ErrorCodes.TokenController.Verify_UserNotExist };
        }

        [Theory]
        [MemberData(nameof(Verify_BadRequest_Data))]
        public async Task Verify_BadRequest(Exception e, int code)
        {
            const string token = "aaaaaaaaaaaaaa";
            _mockUserService.Setup(s => s.VerifyToken(token)).ThrowsAsync(e);
            var action = await _controller.Verify(new VerifyTokenRequest { Token = token });
            action.Result.Should().BeAssignableTo<BadRequestObjectResult>()
                .Which.Value.Should().BeAssignableTo<CommonResponse>()
                .Which.Code.Should().Be(code);
        }
    }
}
