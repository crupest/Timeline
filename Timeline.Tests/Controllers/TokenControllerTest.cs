using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System;
using System.Threading.Tasks;
using Timeline.Controllers;
using Timeline.Models.Http;
using Timeline.Services;
using Timeline.Tests.Mock.Data;
using Timeline.Tests.Mock.Services;
using Xunit;
using static Timeline.ErrorCodes.Http.Token;

namespace Timeline.Tests.Controllers
{
    public class TokenControllerTest
    {
        private readonly Mock<IUserService> _mockUserService = new Mock<IUserService>();
        private readonly TestClock _mockClock = new TestClock();

        private readonly TokenController _controller;

        public TokenControllerTest()
        {
            _controller = new TokenController(_mockUserService.Object, NullLogger<TokenController>.Instance, _mockClock);
        }

        [Theory]
        [InlineData(null)]
        [InlineData(100)]
        public async Task Create_Ok(int? expire)
        {
            var mockCurrentTime = DateTime.Now;
            _mockClock.MockCurrentTime = mockCurrentTime;
            var createResult = new CreateTokenResult
            {
                Token = "mocktokenaaaaa",
                User = MockUser.User.Info
            };
            _mockUserService.Setup(s => s.CreateToken("u", "p", expire == null ? null : (DateTime?)mockCurrentTime.AddDays(expire.Value))).ReturnsAsync(createResult);
            var action = await _controller.Create(new CreateTokenRequest
            {
                Username = "u",
                Password = "p",
                Expire = expire
            });
            action.Should().BeAssignableTo<OkObjectResult>()
                .Which.Value.Should().BeEquivalentTo(createResult);
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
            action.Should().BeAssignableTo<BadRequestObjectResult>()
                .Which.Value.Should().BeAssignableTo<CommonResponse>()
                .Which.Code.Should().Be(Create.BadCredential);
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
            action.Should().BeAssignableTo<BadRequestObjectResult>()
                .Which.Value.Should().BeAssignableTo<CommonResponse>()
                .Which.Code.Should().Be(Create.BadCredential);
        }

        [Fact]
        public async Task Verify_Ok()
        {
            const string token = "aaaaaaaaaaaaaa";
            _mockUserService.Setup(s => s.VerifyToken(token)).ReturnsAsync(MockUser.User.Info);
            var action = await _controller.Verify(new VerifyTokenRequest { Token = token });
            action.Should().BeAssignableTo<OkObjectResult>()
                .Which.Value.Should().BeAssignableTo<VerifyTokenResponse>()
                .Which.User.Should().BeEquivalentTo(MockUser.User.Info);
        }

        // TODO! Verify unit tests
    }
}
