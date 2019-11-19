using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Linq;
using System.Threading.Tasks;
using Timeline.Entities;
using Timeline.Services;
using Timeline.Tests.Helpers;
using Timeline.Tests.Mock.Data;
using Xunit;

namespace Timeline.Tests.Services
{
    public class UserDetailServiceTest : IDisposable
    {
        private readonly TestDatabase _testDatabase;

        private readonly UserDetailService _service;

        public UserDetailServiceTest()
        {
            _testDatabase = new TestDatabase();
            _service = new UserDetailService(_testDatabase.Context, NullLogger<UserDetailService>.Instance);
        }

        public void Dispose()
        {
            _testDatabase.Dispose();
        }

        [Theory]
        [InlineData(null, typeof(ArgumentNullException))]
        [InlineData("", typeof(UsernameBadFormatException))]
        [InlineData("a!a", typeof(UsernameBadFormatException))]
        [InlineData("usernotexist", typeof(UserNotExistException))]
        public async Task GetNickname_ShouldThrow(string username, Type exceptionType)
        {
            await _service.Awaiting(s => s.GetNickname(username)).Should().ThrowAsync(exceptionType);
        }

        [Fact]
        public async Task GetNickname_ShouldReturnUsername()
        {
            var result = await _service.GetNickname(MockUser.User.Username);
            result.Should().Be(MockUser.User.Username);
        }

        [Fact]
        public async Task GetNickname_ShouldReturnData()
        {
            const string nickname = "aaaaaa";
            {
                var context = _testDatabase.Context;
                var userId = (await context.Users.Where(u => u.Name == MockUser.User.Username).Select(u => new { u.Id }).SingleAsync()).Id;
                context.UserDetails.Add(new UserDetail
                {
                    Nickname = nickname,
                    UserId = userId
                });
                await context.SaveChangesAsync();
            }
            var result = await _service.GetNickname(MockUser.User.Username);
            result.Should().Be(nickname);
        }

        [Theory]
        [InlineData(null, typeof(ArgumentNullException))]
        [InlineData("", typeof(UsernameBadFormatException))]
        [InlineData("a!a", typeof(UsernameBadFormatException))]
        [InlineData("usernotexist", typeof(UserNotExistException))]
        public async Task SetNickname_ShouldThrow(string username, Type exceptionType)
        {
            await _service.Awaiting(s => s.SetNickname(username, null)).Should().ThrowAsync(exceptionType);
        }

        [Fact]
        public async Task SetNickname_ShouldThrow_ArgumentException()
        {
            await _service.Awaiting(s => s.SetNickname("uuu", new string('a', 50))).Should().ThrowAsync<ArgumentException>();
        }

        [Fact]
        public async Task SetNickname_ShouldWork()
        {
            var username = MockUser.User.Username;
            var user = await _testDatabase.Context.Users.Where(u => u.Name == username).Include(u => u.Detail).SingleAsync();

            var nickname1 = "nickname1";
            var nickname2 = "nickname2";

            await _service.SetNickname(username, null);
            user.Detail.Should().BeNull();

            await _service.SetNickname(username, nickname1);
            user.Detail.Should().NotBeNull();
            user.Detail.Nickname.Should().Be(nickname1);

            await _service.SetNickname(username, nickname2);
            user.Detail.Should().NotBeNull();
            user.Detail.Nickname.Should().Be(nickname2);

            await _service.SetNickname(username, null);
            user.Detail.Should().NotBeNull();
            user.Detail.Nickname.Should().BeNull();
        }
    }
}
