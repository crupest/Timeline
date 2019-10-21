using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System;
using System.Linq;
using System.Threading.Tasks;
using Timeline.Controllers;
using Timeline.Models;
using Timeline.Models.Http;
using Timeline.Services;
using Timeline.Tests.Helpers;
using Timeline.Tests.Mock.Data;
using Timeline.Tests.Mock.Services;
using Xunit;
using static Timeline.ErrorCodes.Http.User;

namespace Timeline.Tests.Controllers
{
    public class UserControllerTest : IDisposable
    {
        private readonly Mock<IUserService> _mockUserService = new Mock<IUserService>();

        private readonly UserController _controller;

        public UserControllerTest()
        {
            _controller = new UserController(NullLogger<UserController>.Instance,
                _mockUserService.Object,
                TestStringLocalizerFactory.Create());
        }

        public void Dispose()
        {
            _controller.Dispose();
        }

        [Fact]
        public async Task GetList_Success()
        {
            var array = MockUser.UserInfoList.ToArray();
            _mockUserService.Setup(s => s.ListUsers()).ReturnsAsync(array);
            var action = await _controller.List();
            action.Result.Should().BeAssignableTo<OkObjectResult>()
                .Which.Value.Should().BeEquivalentTo(array);
        }

        [Fact]
        public async Task Get_Success()
        {
            const string username = "aaa";
            _mockUserService.Setup(s => s.GetUser(username)).ReturnsAsync(MockUser.User.Info);
            var action = await _controller.Get(username);
            action.Result.Should().BeAssignableTo<OkObjectResult>()
                .Which.Value.Should().BeEquivalentTo(MockUser.User.Info);
        }

        [Fact]
        public async Task Get_NotFound()
        {
            const string username = "aaa";
            _mockUserService.Setup(s => s.GetUser(username)).Returns(Task.FromResult<UserInfo>(null));
            var action = await _controller.Get(username);
            action.Result.Should().BeAssignableTo<NotFoundObjectResult>()
                .Which.Value.Should().BeAssignableTo<CommonResponse>()
                .Which.Code.Should().Be(Get.NotExist);
        }

        [Theory]
        [InlineData(PutResult.Created, true)]
        [InlineData(PutResult.Modified, false)]
        public async Task Put_Success(PutResult result, bool create)
        {
            const string username = "aaa";
            const string password = "ppp";
            const bool administrator = true;
            _mockUserService.Setup(s => s.PutUser(username, password, administrator)).ReturnsAsync(result);
            var action = await _controller.Put(new UserPutRequest
            {
                Password = password,
                Administrator = administrator
            }, username);
            var response = action.Result.Should().BeAssignableTo<ObjectResult>()
                .Which.Value.Should().BeAssignableTo<CommonPutResponse>()
                .Which;
            response.Code.Should().Be(0);
            response.Data.Create.Should().Be(create);
        }

        [Fact]
        public async Task Put_BadUsername()
        {
            const string username = "aaa";
            const string password = "ppp";
            const bool administrator = true;
            _mockUserService.Setup(s => s.PutUser(username, password, administrator)).ThrowsAsync(new UsernameBadFormatException());
            var action = await _controller.Put(new UserPutRequest
            {
                Password = password,
                Administrator = administrator
            }, username);
            action.Result.Should().BeAssignableTo<BadRequestObjectResult>()
                .Which.Value.Should().BeAssignableTo<CommonResponse>()
                .Which.Code.Should().Be(Put.BadUsername);
        }

        //TODO! Complete this.
    }
}
