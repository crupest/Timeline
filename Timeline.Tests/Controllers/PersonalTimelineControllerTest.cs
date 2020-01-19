using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Timeline.Controllers;
using Timeline.Filters;
using Timeline.Models;
using Timeline.Models.Http;
using Timeline.Models.Validation;
using Timeline.Services;
using Timeline.Tests.Helpers;
using Xunit;

namespace Timeline.Tests.Controllers
{
    public class PersonalTimelineControllerTest : IDisposable
    {
        private readonly Mock<IPersonalTimelineService> _service;

        private readonly PersonalTimelineController _controller;

        public PersonalTimelineControllerTest()
        {
            _service = new Mock<IPersonalTimelineService>();
            _controller = new PersonalTimelineController(NullLogger<PersonalTimelineController>.Instance, _service.Object);
        }

        public void Dispose()
        {
            _controller.Dispose();
        }

        [Fact]
        public void AttributeTest()
        {
            static void AssertUsernameParameter(MethodInfo m)
            {
                m.GetParameter("username")
                .Should().BeDecoratedWith<FromRouteAttribute>()
                .And.BeDecoratedWith<UsernameAttribute>();
            }

            static void AssertBodyParamter<TBody>(MethodInfo m)
            {
                var p = m.GetParameter("body");
                p.Should().BeDecoratedWith<FromBodyAttribute>();
                p.ParameterType.Should().Be(typeof(TBody));
            }

            var type = typeof(PersonalTimelineController);
            type.Should().BeDecoratedWith<ApiControllerAttribute>();

            {
                var m = type.GetMethod(nameof(PersonalTimelineController.TimelineGet));
                m.Should().BeDecoratedWith<CatchTimelineNotExistExceptionAttribute>()
                    .And.BeDecoratedWith<HttpGetAttribute>();
                AssertUsernameParameter(m);
            }

            {
                var m = type.GetMethod(nameof(PersonalTimelineController.PostListGet));
                m.Should().BeDecoratedWith<CatchTimelineNotExistExceptionAttribute>()
                    .And.BeDecoratedWith<HttpGetAttribute>();
                AssertUsernameParameter(m);
            }

            {
                var m = type.GetMethod(nameof(PersonalTimelineController.PostOperationCreate));
                m.Should().BeDecoratedWith<CatchTimelineNotExistExceptionAttribute>()
                    .And.BeDecoratedWith<AuthorizeAttribute>()
                    .And.BeDecoratedWith<HttpPostAttribute>();
                AssertUsernameParameter(m);
                AssertBodyParamter<TimelinePostCreateRequest>(m);
            }

            {
                var m = type.GetMethod(nameof(PersonalTimelineController.PostOperationDelete));
                m.Should().BeDecoratedWith<CatchTimelineNotExistExceptionAttribute>()
                    .And.BeDecoratedWith<AuthorizeAttribute>()
                    .And.BeDecoratedWith<HttpPostAttribute>();
                AssertUsernameParameter(m);
                AssertBodyParamter<TimelinePostDeleteRequest>(m);
            }

            {
                var m = type.GetMethod(nameof(PersonalTimelineController.TimelineChangeProperty));
                m.Should().BeDecoratedWith<CatchTimelineNotExistExceptionAttribute>()
                    .And.BeDecoratedWith<AuthorizeAttribute>()
                    .And.BeDecoratedWith<SelfOrAdminAttribute>()
                    .And.BeDecoratedWith<HttpPostAttribute>();
                AssertUsernameParameter(m);
                AssertBodyParamter<TimelinePropertyChangeRequest>(m);
            }

            {
                var m = type.GetMethod(nameof(PersonalTimelineController.TimelineChangeMember));
                m.Should().BeDecoratedWith<CatchTimelineNotExistExceptionAttribute>()
                    .And.BeDecoratedWith<AuthorizeAttribute>()
                    .And.BeDecoratedWith<SelfOrAdminAttribute>()
                    .And.BeDecoratedWith<HttpPostAttribute>();
                AssertUsernameParameter(m);
                AssertBodyParamter<TimelineMemberChangeRequest>(m);
            }
        }

        const string authUsername = "authuser";
        private void SetUser(bool administrator)
        {
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = PrincipalHelper.Create(authUsername, administrator)
                }
            };
        }

        [Fact]
        public async Task TimelineGet()
        {
            const string username = "username";
            var timelineInfo = new BaseTimelineInfo();
            _service.Setup(s => s.GetTimeline(username)).ReturnsAsync(timelineInfo);
            (await _controller.TimelineGet(username)).Value.Should().Be(timelineInfo);
            _service.VerifyAll();
        }

        [Fact]
        public async Task PostListGet_Forbid()
        {
            const string username = "username";
            SetUser(false);
            _service.Setup(s => s.HasReadPermission(username, authUsername)).ReturnsAsync(false);
            var result = (await _controller.PostListGet(username)).Result
                .Should().BeAssignableTo<ObjectResult>()
                .Which;
            result.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
            result.Value.Should().BeAssignableTo<CommonResponse>()
            .Which.Code.Should().Be(ErrorCodes.Common.Forbid);
            _service.VerifyAll();
        }

        [Fact]
        public async Task PostListGet_Admin_Success()
        {
            const string username = "username";
            SetUser(true);
            _service.Setup(s => s.GetPosts(username)).ReturnsAsync(new List<TimelinePostInfo>());
            (await _controller.PostListGet(username)).Value
                .Should().BeAssignableTo<IList<TimelinePostInfo>>()
                .Which.Should().NotBeNull().And.BeEmpty();
            _service.VerifyAll();
        }

        [Fact]
        public async Task PostListGet_User_Success()
        {
            const string username = "username";
            SetUser(false);
            _service.Setup(s => s.HasReadPermission(username, authUsername)).ReturnsAsync(true);
            _service.Setup(s => s.GetPosts(username)).ReturnsAsync(new List<TimelinePostInfo>());
            (await _controller.PostListGet(username)).Value
                .Should().BeAssignableTo<IList<TimelinePostInfo>>()
                .Which.Should().NotBeNull().And.BeEmpty();
            _service.VerifyAll();
        }

        [Fact]
        public async Task PostOperationCreate_Forbid()
        {
            const string username = "username";
            const string content = "cccc";
            SetUser(false);
            _service.Setup(s => s.IsMemberOf(username, authUsername)).ReturnsAsync(false);
            var result = (await _controller.PostOperationCreate(username, new TimelinePostCreateRequest
            {
                Content = content,
                Time = null
            })).Result.Should().NotBeNull().And.BeAssignableTo<ObjectResult>().Which;
            result.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
            result.Value.Should().BeAssignableTo<CommonResponse>()
                .Which.Code.Should().Be(ErrorCodes.Common.Forbid);
            _service.VerifyAll();
        }

        [Fact]
        public async Task PostOperationCreate_Admin_Success()
        {
            const string username = "username";
            const string content = "cccc";
            var response = new TimelinePostCreateResponse
            {
                Id = 3,
                Time = DateTime.Now
            };
            SetUser(true);
            _service.Setup(s => s.CreatePost(username, authUsername, content, null)).ReturnsAsync(response);
            var resultValue = (await _controller.PostOperationCreate(username, new TimelinePostCreateRequest
            {
                Content = content,
                Time = null
            })).Value;
            resultValue.Should().NotBeNull()
                .And.BeAssignableTo<TimelinePostCreateResponse>()
                .And.BeEquivalentTo(response);
            _service.VerifyAll();
        }

        [Fact]
        public async Task PostOperationCreate_User_Success()
        {
            const string username = "username";
            const string content = "cccc";
            var response = new TimelinePostCreateResponse
            {
                Id = 3,
                Time = DateTime.Now
            };
            SetUser(false);
            _service.Setup(s => s.IsMemberOf(username, authUsername)).ReturnsAsync(true);
            _service.Setup(s => s.CreatePost(username, authUsername, content, null)).ReturnsAsync(response);
            var resultValue = (await _controller.PostOperationCreate(username, new TimelinePostCreateRequest
            {
                Content = content,
                Time = null
            })).Value;
            resultValue.Should().NotBeNull()
                .And.BeAssignableTo<TimelinePostCreateResponse>()
                .And.BeEquivalentTo(response);
            _service.VerifyAll();
        }

        [Fact]
        public async Task PostOperationDelete_Forbid()
        {
            const string username = "username";
            const long postId = 2;
            SetUser(false);
            _service.Setup(s => s.HasPostModifyPermission(username, postId, authUsername)).ReturnsAsync(false);
            var result = (await _controller.PostOperationDelete(username, new TimelinePostDeleteRequest
            {
                Id = postId
            })).Should().NotBeNull().And.BeAssignableTo<ObjectResult>().Which;
            result.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
            result.Value.Should().BeAssignableTo<CommonResponse>()
                .Which.Code.Should().Be(ErrorCodes.Common.Forbid);
            _service.VerifyAll();
        }

        [Fact]
        public async Task PostOperationDelete_NotExist()
        {
            const string username = "username";
            const long postId = 2;
            SetUser(true);
            _service.Setup(s => s.DeletePost(username, postId)).ThrowsAsync(new TimelinePostNotExistException());
            var result = (await _controller.PostOperationDelete(username, new TimelinePostDeleteRequest
            {
                Id = postId
            })).Should().NotBeNull().And.BeAssignableTo<ObjectResult>().Which;
            result.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
            result.Value.Should().BeAssignableTo<CommonResponse>()
                .Which.Code.Should().Be(ErrorCodes.TimelineController.PostOperationDelete_NotExist);
            _service.VerifyAll();
        }

        [Fact]
        public async Task PostOperationDelete_Admin_Success()
        {
            const string username = "username";
            const long postId = 2;
            SetUser(true);
            _service.Setup(s => s.DeletePost(username, postId)).Returns(Task.CompletedTask);
            var result = await _controller.PostOperationDelete(username, new TimelinePostDeleteRequest
            {
                Id = postId
            });
            result.Should().NotBeNull().And.BeAssignableTo<OkResult>();
            _service.VerifyAll();
        }

        [Fact]
        public async Task PostOperationDelete_User_Success()
        {
            const string username = "username";
            const long postId = 2;
            SetUser(false);
            _service.Setup(s => s.DeletePost(username, postId)).Returns(Task.CompletedTask);
            _service.Setup(s => s.HasPostModifyPermission(username, postId, authUsername)).ReturnsAsync(true);
            var result = await _controller.PostOperationDelete(username, new TimelinePostDeleteRequest
            {
                Id = postId
            });
            result.Should().NotBeNull().And.BeAssignableTo<OkResult>();
            _service.VerifyAll();
        }

        [Fact]
        public async Task TimelineChangeProperty_Success()
        {
            const string username = "username";
            var req = new TimelinePropertyChangeRequest
            {
                Description = "",
                Visibility = TimelineVisibility.Private
            };
            _service.Setup(s => s.ChangeProperty(username, req)).Returns(Task.CompletedTask);
            var result = await _controller.TimelineChangeProperty(username, req);
            result.Should().NotBeNull().And.BeAssignableTo<OkResult>();
            _service.VerifyAll();
        }

        [Fact]
        public async Task TimelineChangeMember_Success()
        {
            const string username = "username";
            var add = new List<string> { "aaa" };
            var remove = new List<string> { "rrr" };
            _service.Setup(s => s.ChangeMember(username, add, remove)).Returns(Task.CompletedTask);
            var result = await _controller.TimelineChangeMember(username, new TimelineMemberChangeRequest
            {
                Add = add,
                Remove = remove
            });
            result.Should().NotBeNull().And.BeAssignableTo<OkResult>();
            _service.VerifyAll();
        }

        [Fact]
        public async Task TimelineChangeMember_UsernameBadFormat()
        {
            const string username = "username";
            var add = new List<string> { "aaa" };
            var remove = new List<string> { "rrr" };
            _service.Setup(s => s.ChangeMember(username, add, remove)).ThrowsAsync(
                new TimelineMemberOperationUserException("test", new UsernameBadFormatException()));
            var result = await _controller.TimelineChangeMember(username, new TimelineMemberChangeRequest
            {
                Add = add,
                Remove = remove
            });
            result.Should().NotBeNull().And.BeAssignableTo<BadRequestObjectResult>()
                .Which.Value.Should().BeAssignableTo<CommonResponse>()
                .Which.Code.Should().Be(ErrorCodes.Common.InvalidModel);
            _service.VerifyAll();
        }

        [Fact]
        public async Task TimelineChangeMember_AddNotExist()
        {
            const string username = "username";
            var add = new List<string> { "aaa" };
            var remove = new List<string> { "rrr" };
            _service.Setup(s => s.ChangeMember(username, add, remove)).ThrowsAsync(
                new TimelineMemberOperationUserException("test", new UserNotExistException()));
            var result = await _controller.TimelineChangeMember(username, new TimelineMemberChangeRequest
            {
                Add = add,
                Remove = remove
            });
            result.Should().NotBeNull().And.BeAssignableTo<BadRequestObjectResult>()
                .Which.Value.Should().BeAssignableTo<CommonResponse>()
                .Which.Code.Should().Be(ErrorCodes.UserCommon.NotExist);
            _service.VerifyAll();
        }

        [Fact]
        public async Task TimelineChangeMember_UnknownTimelineMemberOperationUserException()
        {
            const string username = "username";
            var add = new List<string> { "aaa" };
            var remove = new List<string> { "rrr" };
            _service.Setup(s => s.ChangeMember(username, add, remove)).ThrowsAsync(
                new TimelineMemberOperationUserException("test", null));
            await _controller.Awaiting(c => c.TimelineChangeMember(username, new TimelineMemberChangeRequest
            {
                Add = add,
                Remove = remove
            })).Should().ThrowAsync<TimelineMemberOperationUserException>(); // Should rethrow.
        }
    }
}
