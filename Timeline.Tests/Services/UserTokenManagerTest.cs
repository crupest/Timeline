using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System;
using System.Threading.Tasks;
using Timeline.Models;
using Timeline.Services;
using Timeline.Tests.Helpers;
using Xunit;

namespace Timeline.Tests.Services
{
    public class UserTokenManagerTest
    {
        private readonly UserTokenManager _service;

        private readonly Mock<IUserService> _mockUserService;
        private readonly Mock<IUserTokenService> _mockUserTokenService;
        private readonly TestClock _mockClock;

        public UserTokenManagerTest()
        {
            _mockUserService = new Mock<IUserService>();
            _mockUserTokenService = new Mock<IUserTokenService>();
            _mockClock = new TestClock();

            _service = new UserTokenManager(NullLogger<UserTokenManager>.Instance, _mockUserService.Object, _mockUserTokenService.Object, _mockClock);
        }

        [Theory]
        [InlineData(null, "aaa", "username")]
        [InlineData("aaa", null, "password")]
        public void CreateToken_NullArgument(string username, string password, string paramName)
        {
            _service.Invoking(s => s.CreateToken(username, password)).Should().Throw<ArgumentNullException>()
                .Which.ParamName.Should().Be(paramName);
        }

        [Theory]
        [InlineData(typeof(UsernameBadFormatException))]
        [InlineData(typeof(UserNotExistException))]
        [InlineData(typeof(BadPasswordException))]
        public async Task CreateToken_VerifyCredential_Throw(Type exceptionType)
        {
            const string username = "uuu";
            const string password = "ppp";
            _mockUserService.Setup(s => s.VerifyCredential(username, password)).ThrowsAsync((Exception)Activator.CreateInstance(exceptionType));
            await _service.Awaiting(s => s.CreateToken(username, password)).Should().ThrowAsync(exceptionType);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task CreateToken_Success(bool setExpireTime)
        {
            const string username = "uuu";
            const string password = "ppp";
            var mockExpireTime = setExpireTime ? (DateTime?)DateTime.Now : null;
            var mockUserInfo = new User
            {
                Id = 1,
                Username = username,
                Administrator = false,
                Version = 1
            };
            const string mockToken = "mocktokenaaaaaaa";

            _mockUserService.Setup(s => s.VerifyCredential(username, password)).ReturnsAsync(mockUserInfo);
            _mockUserTokenService.Setup(s => s.GenerateToken(
                It.Is<UserTokenInfo>(userTokenInfo =>
                userTokenInfo.Id == mockUserInfo.Id &&
                userTokenInfo.Version == mockUserInfo.Version &&
                userTokenInfo.ExpireAt == mockExpireTime))).Returns(mockToken);
            (await _service.CreateToken(username, password, mockExpireTime))
                .Should().BeEquivalentTo(new UserTokenCreateResult
                {
                    Token = mockToken,
                    User = mockUserInfo
                });
        }

        [Fact]
        public void VerifyToken_NullArgument()
        {
            _service.Invoking(s => s.VerifyToken(null)).Should().ThrowAsync<ArgumentNullException>();
        }

        [Fact]
        public async Task VerifyToken_BadFormat()
        {
            const string mockToken = "mocktokenaaaaaa";
            _mockUserTokenService.Setup(s => s.VerifyToken(mockToken)).Throws(new UserTokenBadFormatException());

            await _service.Awaiting(s => s.VerifyToken(mockToken)).Should().ThrowAsync<UserTokenBadFormatException>();
        }

        [Fact]
        public async Task VerifyToken_TimeExpire()
        {
            const string mockToken = "mocktokenaaaaaa";
            var mockTime = DateTime.Now;
            _mockClock.MockCurrentTime = mockTime;
            var mockTokenInfo = new UserTokenInfo
            {
                Id = 1,
                Version = 1,
                ExpireAt = mockTime.AddDays(-1)
            };
            _mockUserTokenService.Setup(s => s.VerifyToken(mockToken)).Returns(mockTokenInfo);

            await _service.Awaiting(s => s.VerifyToken(mockToken)).Should().ThrowAsync<UserTokenTimeExpireException>();
        }

        [Fact]
        public async Task VerifyToken_BadVersion()
        {
            const string mockToken = "mocktokenaaaaaa";
            var mockTime = DateTime.Now;
            _mockClock.MockCurrentTime = mockTime;
            var mockTokenInfo = new UserTokenInfo
            {
                Id = 1,
                Version = 1,
                ExpireAt = mockTime.AddDays(1)
            };
            _mockUserTokenService.Setup(s => s.VerifyToken(mockToken)).Returns(mockTokenInfo);
            _mockUserService.Setup(s => s.GetUserById(1)).ReturnsAsync(new User
            {
                Id = 1,
                Username = "aaa",
                Administrator = false,
                Version = 2
            });

            await _service.Awaiting(s => s.VerifyToken(mockToken)).Should().ThrowAsync<UserTokenBadVersionException>();
        }

        [Fact]
        public async Task VerifyToken_Success()
        {
            const string mockToken = "mocktokenaaaaaa";
            var mockTime = DateTime.Now;
            _mockClock.MockCurrentTime = mockTime;
            var mockTokenInfo = new UserTokenInfo
            {
                Id = 1,
                Version = 1,
                ExpireAt = mockTime.AddDays(1)
            };
            var mockUserInfo = new User
            {
                Id = 1,
                Username = "aaa",
                Administrator = false,
                Version = 1
            };
            _mockUserTokenService.Setup(s => s.VerifyToken(mockToken)).Returns(mockTokenInfo);
            _mockUserService.Setup(s => s.GetUserById(1)).ReturnsAsync(mockUserInfo);

            (await _service.VerifyToken(mockToken)).Should().BeEquivalentTo(mockUserInfo);
        }
    }
}
