using FluentAssertions;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using Timeline.Entities;
using Timeline.Models;
using Timeline.Services;
using Timeline.Tests.Helpers;
using Timeline.Tests.Mock.Data;
using Xunit;
using Xunit.Abstractions;

namespace Timeline.Tests
{
    public class UserDetailServiceTest : IDisposable
    {
        private readonly LoggerFactory _loggerFactory;
        private readonly TestDatabase _database;

        private readonly UserDetailService _service;

        public UserDetailServiceTest(ITestOutputHelper outputHelper)
        {
            _loggerFactory = MyTestLoggerFactory.Create(outputHelper);
            _database = new TestDatabase();

            _service = new UserDetailService(_loggerFactory.CreateLogger<UserDetailService>(), _database.DatabaseContext);
        }

        public void Dispose()
        {
            _loggerFactory.Dispose();
            _database.Dispose();
        }

        [Fact]
        public void GetDetail_ShouldThrow_ArgumentException()
        {
            // no need to await because arguments are checked syncronizedly.
            _service.Invoking(s => s.GetUserDetail(null)).Should().Throw<ArgumentException>()
                .Where(e => e.ParamName == "username" && e.Message.Contains("null", StringComparison.OrdinalIgnoreCase));
            _service.Invoking(s => s.GetUserDetail("")).Should().Throw<ArgumentException>()
                .Where(e => e.ParamName == "username" && e.Message.Contains("empty", StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public void GetDetail_ShouldThrow_UserNotExistException()
        {
            const string username = "usernotexist";
            _service.Awaiting(s => s.GetUserDetail(username)).Should().Throw<UserNotExistException>()
                .Where(e => e.Username == username);
        }

        [Fact]
        public async Task GetDetail_Should_Create_And_ReturnDefault()
        {
            {
                var detail = await _service.GetUserDetail(MockUsers.UserUsername);
                detail.Should().BeEquivalentTo(new UserDetail());
            }

            {
                var context = _database.DatabaseContext;
                var userId = await DatabaseExtensions.CheckAndGetUser(context.Users, MockUsers.UserUsername);
                var detail = context.UserDetails.Where(e => e.UserId == userId).Single();
                detail.QQ.Should().BeNullOrEmpty();
                detail.EMail.Should().BeNullOrEmpty();
                detail.PhoneNumber.Should().BeNullOrEmpty();
                detail.Description.Should().BeNullOrEmpty();
            }
        }

        [Fact]
        public async Task GetDetail_Should_ReturnData()
        {
            const string email = "ha@aaa.net";
            const string description = "hahaha";

            var context = _database.DatabaseContext;
            UserDetailEntity entity;

            {
                var userId = await DatabaseExtensions.CheckAndGetUser(context.Users, MockUsers.UserUsername);
                entity = new UserDetailEntity
                {
                    EMail = email,
                    Description = description,
                    UserId = userId
                };
                context.Add(entity);
                await context.SaveChangesAsync();
            }

            {
                var detail = await _service.GetUserDetail(MockUsers.UserUsername);
                detail.Should().BeEquivalentTo(new UserDetail
                {
                    EMail = email,
                    Description = description
                });
            }
        }

        [Fact]
        public void UpdateDetail_ShouldThrow_ArgumentException()
        {
            // no need to await because arguments are checked syncronizedly.
            _service.Invoking(s => s.UpdateUserDetail(null, new UserDetail())).Should().Throw<ArgumentException>()
                .Where(e => e.ParamName == "username" && e.Message.Contains("null", StringComparison.OrdinalIgnoreCase));
            _service.Invoking(s => s.UpdateUserDetail("", new UserDetail())).Should().Throw<ArgumentException>()
                .Where(e => e.ParamName == "username" && e.Message.Contains("empty", StringComparison.OrdinalIgnoreCase));
            _service.Invoking(s => s.UpdateUserDetail("aaa", null)).Should().Throw<ArgumentException>()
                .Where(e => e.ParamName == "detail");
        }

        [Fact]
        public void UpdateDetail_ShouldThrow_UserNotExistException()
        {
            const string username = "usernotexist";
            _service.Awaiting(s => s.UpdateUserDetail(username, new UserDetail())).Should().Throw<UserNotExistException>()
                .Where(e => e.Username == username);
        }

        [Fact]
        public async Task UpdateDetail_Should_Work()
        {
            UserDetailEntity entity;

            await _service.UpdateUserDetail(MockUsers.UserUsername, new UserDetail());

            {
                var context = _database.DatabaseContext;
                var userId = await DatabaseExtensions.CheckAndGetUser(context.Users, MockUsers.UserUsername);
                entity = context.UserDetails.Where(e => e.UserId == userId).Single();
                entity.QQ.Should().BeNullOrEmpty();
                entity.EMail.Should().BeNullOrEmpty();
                entity.PhoneNumber.Should().BeNullOrEmpty();
                entity.Description.Should().BeNullOrEmpty();
            }

            const string email = "ha@aaa.net";
            const string phoneNumber = "12345678910";
            const string description = "hahaha";

            await _service.UpdateUserDetail(MockUsers.UserUsername, new UserDetail
            {
                EMail = email,
                PhoneNumber = phoneNumber,
                Description = description
            });

            {
                var context = _database.DatabaseContext;
                var userId = await DatabaseExtensions.CheckAndGetUser(context.Users, MockUsers.UserUsername);
                entity = context.UserDetails.Where(e => e.UserId == userId).Single();
                entity.QQ.Should().BeNullOrEmpty();
                entity.EMail.Should().Be(email);
                entity.PhoneNumber.Should().Be(phoneNumber);
                entity.Description.Should().Be(description);
            }

            const string newDescription = "new description";

            await _service.UpdateUserDetail(MockUsers.UserUsername, new UserDetail
            {
                EMail = null,
                PhoneNumber = "",
                Description = newDescription
            });

            {
                entity.QQ.Should().BeNullOrEmpty();
                entity.EMail.Should().Be(email);
                entity.PhoneNumber.Should().BeNullOrEmpty();
                entity.Description.Should().Be(newDescription);
            }
        }
    }
}
