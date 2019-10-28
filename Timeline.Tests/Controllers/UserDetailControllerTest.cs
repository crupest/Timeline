using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Timeline.Controllers;
using Timeline.Filters;
using Timeline.Models.Validation;
using Timeline.Services;
using Timeline.Tests.Helpers;
using Xunit;

namespace Timeline.Tests.Controllers
{
    public class UserDetailControllerTest : IDisposable
    {
        private readonly Mock<IUserDetailService> _mockUserDetailService;
        private readonly UserDetailController _controller;

        public UserDetailControllerTest()
        {
            _mockUserDetailService = new Mock<IUserDetailService>();
            _controller = new UserDetailController(_mockUserDetailService.Object);
        }

        public void Dispose()
        {
            _controller.Dispose();
        }

        [Fact]
        public void AttributeTest()
        {
            typeof(UserDetailController).Should().BeDecoratedWith<ApiControllerAttribute>();

            var getNickname = typeof(UserDetailController).GetMethod(nameof(UserDetailController.GetNickname));
            getNickname.Should().BeDecoratedWith<HttpGetAttribute>()
                .And.BeDecoratedWith<CatchUserNotExistExceptionAttribute>();
            getNickname.GetParameter("username").Should().BeDecoratedWith<UsernameAttribute>()
                .And.BeDecoratedWith<FromRouteAttribute>();

            var putNickname = typeof(UserDetailController).GetMethod(nameof(UserDetailController.PutNickname));
            putNickname.Should().BeDecoratedWith<HttpPutAttribute>()
                .And.BeDecoratedWith<CatchUserNotExistExceptionAttribute>();
            putNickname.GetParameter("username").Should().BeDecoratedWith<UsernameAttribute>()
                .And.BeDecoratedWith<FromRouteAttribute>();
            var stringLengthAttributeOnPutBody = putNickname.GetParameter("body").Should().BeDecoratedWith<FromBodyAttribute>()
                .And.BeDecoratedWith<StringLengthAttribute>()
                .Which;
            stringLengthAttributeOnPutBody.MinimumLength.Should().Be(1);
            stringLengthAttributeOnPutBody.MaximumLength.Should().Be(10);

            var deleteNickname = typeof(UserDetailController).GetMethod(nameof(UserDetailController.DeleteNickname));
            deleteNickname.Should().BeDecoratedWith<HttpDeleteAttribute>()
                .And.BeDecoratedWith<CatchUserNotExistExceptionAttribute>();
            deleteNickname.GetParameter("username").Should().BeDecoratedWith<UsernameAttribute>()
                .And.BeDecoratedWith<FromRouteAttribute>();
        }

        [Fact]
        public async Task GetNickname_ShouldWork()
        {
            const string username = "uuu";
            const string nickname = "nnn";
            _mockUserDetailService.Setup(s => s.GetNickname(username)).ReturnsAsync(nickname);
            var actionResult = await _controller.GetNickname(username);
            actionResult.Result.Should().BeAssignableTo<OkObjectResult>(nickname);
            _mockUserDetailService.VerifyAll();
        }

        [Fact]
        public async Task PutNickname_ShouldWork()
        {
            const string username = "uuu";
            const string nickname = "nnn";
            _mockUserDetailService.Setup(s => s.SetNickname(username, nickname)).Returns(Task.CompletedTask);
            var actionResult = await _controller.PutNickname(username, nickname);
            actionResult.Should().BeAssignableTo<OkResult>();
            _mockUserDetailService.VerifyAll();
        }

        [Fact]
        public async Task DeleteNickname_ShouldWork()
        {
            const string username = "uuu";
            _mockUserDetailService.Setup(s => s.SetNickname(username, null)).Returns(Task.CompletedTask);
            var actionResult = await _controller.DeleteNickname(username);
            actionResult.Should().BeAssignableTo<OkResult>();
            _mockUserDetailService.VerifyAll();
        }
    }
}
