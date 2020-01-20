using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Timeline.Controllers;
using Timeline.Models;
using Timeline.Models.Http;
using Timeline.Services;
using Timeline.Tests.Helpers;
using Xunit;

namespace Timeline.Tests.Controllers
{
    public class UserControllerTest : IDisposable
    {
        private readonly Mock<IUserService> _mockUserService = new Mock<IUserService>();

        private readonly UserController _controller;

        public UserControllerTest()
        {
            _controller = new UserController(NullLogger<UserController>.Instance, _mockUserService.Object);
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
            _mockUserService.Setup(s => s.GetUserByUsername(username)).ReturnsAsync(MockUser.User.Info);
            var action = await _controller.Get(username);
            action.Result.Should().BeAssignableTo<OkObjectResult>()
                .Which.Value.Should().BeEquivalentTo(MockUser.User.Info);
        }

        [Fact]
        public async Task Get_NotFound()
        {
            const string username = "aaa";
            _mockUserService.Setup(s => s.GetUserByUsername(username)).Returns(Task.FromResult<User>(null));
            var action = await _controller.Get(username);
            action.Result.Should().BeAssignableTo<NotFoundObjectResult>()
                .Which.Value.Should().BeAssignableTo<CommonResponse>()
                .Which.Code.Should().Be(ErrorCodes.UserCommon.NotExist);
        }

        [Theory]
        [InlineData(PutResult.Create, true)]
        [InlineData(PutResult.Modify, false)]
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
        public async Task Patch_Success()
        {
            const string username = "aaa";
            const string password = "ppp";
            const bool administrator = true;
            _mockUserService.Setup(s => s.PatchUser(username, password, administrator)).Returns(Task.CompletedTask);
            var action = await _controller.Patch(new UserPatchRequest
            {
                Password = password,
                Administrator = administrator
            }, username);
            action.Should().BeAssignableTo<OkResult>();
        }

        [Fact]
        public async Task Patch_NotExist()
        {
            const string username = "aaa";
            const string password = "ppp";
            const bool administrator = true;
            _mockUserService.Setup(s => s.PatchUser(username, password, administrator)).ThrowsAsync(new UserNotExistException());
            var action = await _controller.Patch(new UserPatchRequest
            {
                Password = password,
                Administrator = administrator
            }, username);
            action.Should().BeAssignableTo<NotFoundObjectResult>()
                .Which.Value.Should().BeAssignableTo<CommonResponse>()
                .Which.Code.Should().Be(ErrorCodes.UserCommon.NotExist);
        }

        [Fact]
        public async Task Delete_Delete()
        {
            const string username = "aaa";
            _mockUserService.Setup(s => s.DeleteUser(username)).Returns(Task.CompletedTask);
            var action = await _controller.Delete(username);
            var body = action.Result.Should().BeAssignableTo<OkObjectResult>()
                .Which.Value.Should().BeAssignableTo<CommonDeleteResponse>()
                .Which;
            body.Code.Should().Be(0);
            body.Data.Delete.Should().BeTrue();
        }

        [Fact]
        public async Task Delete_NotExist()
        {
            const string username = "aaa";
            _mockUserService.Setup(s => s.DeleteUser(username)).ThrowsAsync(new UserNotExistException());
            var action = await _controller.Delete(username);
            var body = action.Result.Should().BeAssignableTo<OkObjectResult>()
                .Which.Value.Should().BeAssignableTo<CommonDeleteResponse>()
                .Which;
            body.Code.Should().Be(0);
            body.Data.Delete.Should().BeFalse();
        }

        [Fact]
        public async Task Op_ChangeUsername_Success()
        {
            const string oldUsername = "aaa";
            const string newUsername = "bbb";
            _mockUserService.Setup(s => s.ChangeUsername(oldUsername, newUsername)).Returns(Task.CompletedTask);
            var action = await _controller.ChangeUsername(new ChangeUsernameRequest { OldUsername = oldUsername, NewUsername = newUsername });
            action.Should().BeAssignableTo<OkResult>();
        }

        [Theory]
        [InlineData(typeof(UserNotExistException), ErrorCodes.UserCommon.NotExist)]
        [InlineData(typeof(UsernameConfictException), ErrorCodes.UserController.ChangeUsername_Conflict)]
        public async Task Op_ChangeUsername_Failure(Type exceptionType, int code)
        {
            const string oldUsername = "aaa";
            const string newUsername = "bbb";
            _mockUserService.Setup(s => s.ChangeUsername(oldUsername, newUsername)).ThrowsAsync(Activator.CreateInstance(exceptionType) as Exception);
            var action = await _controller.ChangeUsername(new ChangeUsernameRequest { OldUsername = oldUsername, NewUsername = newUsername });
            action.Should().BeAssignableTo<BadRequestObjectResult>()
                .Which.Value.Should().BeAssignableTo<CommonResponse>()
                .Which.Code.Should().Be(code);
        }

        [Fact]
        public async Task Op_ChangePassword_Success()
        {
            const string username = "aaa";
            const string oldPassword = "aaa";
            const string newPassword = "bbb";
            _mockUserService.Setup(s => s.ChangePassword(username, oldPassword, newPassword)).Returns(Task.CompletedTask);

            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
                    {
                        new Claim(ClaimTypes.Name, username)
                    }, "TestAuthType"))
                }
            };

            var action = await _controller.ChangePassword(new ChangePasswordRequest { OldPassword = oldPassword, NewPassword = newPassword });
            action.Should().BeAssignableTo<OkResult>();
        }

        [Fact]
        public async Task Op_ChangePassword_BadPassword()
        {
            const string username = "aaa";
            const string oldPassword = "aaa";
            const string newPassword = "bbb";
            _mockUserService.Setup(s => s.ChangePassword(username, oldPassword, newPassword)).ThrowsAsync(new BadPasswordException());

            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
                    {
                        new Claim(ClaimTypes.Name, username)
                    }, "TestAuthType"))
                }
            };

            var action = await _controller.ChangePassword(new ChangePasswordRequest { OldPassword = oldPassword, NewPassword = newPassword });
            action.Should().BeAssignableTo<BadRequestObjectResult>()
                .Which.Value.Should().BeAssignableTo<CommonResponse>()
                .Which.Code.Should().Be(ErrorCodes.UserController.ChangePassword_BadOldPassword);
        }
    }
}
