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
using Timeline.Tests.Helpers.Authentication;
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
                var m = type.GetMethod(nameof(PersonalTimelineController.PostsGet));
                m.Should().BeDecoratedWith<CatchTimelineNotExistExceptionAttribute>()
                    .And.BeDecoratedWith<HttpGetAttribute>();
                AssertUsernameParameter(m);
            }

            {
                var m = type.GetMethod(nameof(PersonalTimelineController.TimelinePost));
                m.Should().BeDecoratedWith<CatchTimelineNotExistExceptionAttribute>()
                    .And.BeDecoratedWith<AuthorizeAttribute>()
                    .And.BeDecoratedWith<HttpPostAttribute>();
                AssertUsernameParameter(m);
                AssertBodyParamter<TimelinePostCreateRequest>(m);
            }

            {
                var m = type.GetMethod(nameof(PersonalTimelineController.TimelinePostDelete));
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
        public async Task PostsGet_Forbid()
        {
            const string username = "username";
            SetUser(false);
            _service.Setup(s => s.HasReadPermission(username, authUsername)).ReturnsAsync(false);
            (await _controller.PostsGet(username)).Result
                .Should().BeAssignableTo<ObjectResult>()
                .Which.Value.Should().BeAssignableTo<CommonResponse>()
                .Which.Code.Should().Be(ErrorCodes.Http.Timeline.PostsGetForbid);
            _service.VerifyAll();
        }

        [Fact]
        public async Task PostsGet_Admin_Success()
        {
            const string username = "username";
            SetUser(true);
            _service.Setup(s => s.GetPosts(username)).ReturnsAsync(new List<TimelinePostInfo>());
            (await _controller.PostsGet(username)).Value
                .Should().BeAssignableTo<IList<TimelinePostInfo>>()
                .Which.Should().NotBeNull().And.BeEmpty();
            _service.VerifyAll();
        }

        [Fact]
        public async Task PostsGet_User_Success()
        {
            const string username = "username";
            SetUser(false);
            _service.Setup(s => s.HasReadPermission(username, authUsername)).ReturnsAsync(true);
            _service.Setup(s => s.GetPosts(username)).ReturnsAsync(new List<TimelinePostInfo>());
            (await _controller.PostsGet(username)).Value
                .Should().BeAssignableTo<IList<TimelinePostInfo>>()
                .Which.Should().NotBeNull().And.BeEmpty();
            _service.VerifyAll();
        }

        //TODO! Write all the other tests.
    }
}
