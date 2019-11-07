using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Timeline.Controllers;
using Timeline.Services;
using Moq;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Timeline.Filters;
using Timeline.Tests.Helpers;
using Timeline.Models.Validation;
using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Timeline.Models.Http;

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
    }
}
