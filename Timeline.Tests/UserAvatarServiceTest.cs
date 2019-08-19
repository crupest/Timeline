using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp.Formats.Png;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Timeline.Entities;
using Timeline.Services;
using Timeline.Tests.Helpers;
using Timeline.Tests.Mock.Data;
using Xunit;
using Xunit.Abstractions;

namespace Timeline.Tests
{
    public class MockDefaultUserAvatarProvider : IDefaultUserAvatarProvider
    {
        public static AvatarInfo AvatarInfo { get; } = new AvatarInfo
        {
            Avatar = new Avatar { Type = "image/test", Data = Encoding.ASCII.GetBytes("test") },
            LastModified = DateTime.Now
        };

        public Task<AvatarInfo> GetDefaultAvatar()
        {
            return Task.FromResult(AvatarInfo);
        }
    }

    public class MockUserAvatarValidator : IUserAvatarValidator
    {
        public Task Validate(Avatar avatar)
        {
            return Task.CompletedTask;
        }
    }

    public class UserAvatarValidatorTest : IClassFixture<UserAvatarValidator>
    {
        private readonly UserAvatarValidator _validator;

        public UserAvatarValidatorTest(UserAvatarValidator validator)
        {
            _validator = validator;
        }

        [Fact]
        public void CantDecode()
        {
            Avatar avatar = new Avatar
            {
                Data = Encoding.ASCII.GetBytes("This is not a image."),
                Type = "image/png"
            };
            _validator.Awaiting(v => v.Validate(avatar))
                    .Should().Throw<AvatarDataException>()
                    .Where(e => e.Avatar == avatar && e.Error == AvatarDataException.ErrorReason.CantDecode);
        }

        [Fact]
        public void UnmatchedFormat()
        {
            Avatar avatar = new Avatar
            {
                Data = ImageHelper.CreatePngWithSize(100, 100),
                Type = "image/jpeg"
            };
            _validator.Awaiting(v => v.Validate(avatar))
                    .Should().Throw<AvatarDataException>()
                    .Where(e => e.Avatar == avatar && e.Error == AvatarDataException.ErrorReason.UnmatchedFormat);
        }

        [Fact]
        public void BadSize()
        {
            Avatar avatar = new Avatar
            {
                Data = ImageHelper.CreatePngWithSize(100, 200),
                Type = PngFormat.Instance.DefaultMimeType
            };
            _validator.Awaiting(v => v.Validate(avatar))
                    .Should().Throw<AvatarDataException>()
                    .Where(e => e.Avatar == avatar && e.Error == AvatarDataException.ErrorReason.BadSize);
        }

        [Fact]
        public void Success()
        {
            Avatar avatar = new Avatar
            {
                Data = ImageHelper.CreatePngWithSize(100, 100),
                Type = PngFormat.Instance.DefaultMimeType
            };
            _validator.Awaiting(v => v.Validate(avatar))
                    .Should().NotThrow();
        }
    }

    public class UserAvatarServiceTest : IDisposable, IClassFixture<MockDefaultUserAvatarProvider>, IClassFixture<MockUserAvatarValidator>
    {
        private static Avatar MockAvatar { get; } = new Avatar
        {
            Type = "image/testaaa",
            Data = Encoding.ASCII.GetBytes("amock")
        };

        private static Avatar MockAvatar2 { get; } = new Avatar
        {
            Type = "image/testbbb",
            Data = Encoding.ASCII.GetBytes("bmock")
        };

        private readonly MockDefaultUserAvatarProvider _mockDefaultUserAvatarProvider;

        private readonly LoggerFactory _loggerFactory;
        private readonly TestDatabase _database;

        private readonly UserAvatarService _service;

        public UserAvatarServiceTest(ITestOutputHelper outputHelper, MockDefaultUserAvatarProvider mockDefaultUserAvatarProvider, MockUserAvatarValidator mockUserAvatarValidator)
        {
            _mockDefaultUserAvatarProvider = mockDefaultUserAvatarProvider;

            _loggerFactory = MyTestLoggerFactory.Create(outputHelper);
            _database = new TestDatabase();

            _service = new UserAvatarService(_loggerFactory.CreateLogger<UserAvatarService>(), _database.DatabaseContext, _mockDefaultUserAvatarProvider, mockUserAvatarValidator);
        }

        public void Dispose()
        {
            _loggerFactory.Dispose();
            _database.Dispose();
        }

        [Fact]
        public void GetAvatar_ShouldThrow_ArgumentException()
        {
            // no need to await because arguments are checked syncronizedly.
            _service.Invoking(s => s.GetAvatar(null)).Should().Throw<ArgumentException>()
                .Where(e => e.ParamName == "username" && e.Message.Contains("null", StringComparison.OrdinalIgnoreCase));
            _service.Invoking(s => s.GetAvatar("")).Should().Throw<ArgumentException>()
                .Where(e => e.ParamName == "username" && e.Message.Contains("empty", StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public void GetAvatar_ShouldThrow_UserNotExistException()
        {
            const string username = "usernotexist";
            _service.Awaiting(s => s.GetAvatar(username)).Should().Throw<UserNotExistException>()
                .Where(e => e.Username == username);
        }

        [Fact]
        public async Task GetAvatar_ShouldReturn_Default()
        {
            const string username = MockUsers.UserUsername;
            (await _service.GetAvatar(username)).Avatar.Should().BeEquivalentTo((await _mockDefaultUserAvatarProvider.GetDefaultAvatar()).Avatar);
        }

        [Fact]
        public async Task GetAvatar_ShouldReturn_Data()
        {
            const string username = MockUsers.UserUsername;

            {
                // create mock data
                var context = _database.DatabaseContext;
                var user = await context.Users.Where(u => u.Name == username).Include(u => u.Avatar).SingleAsync();
                user.Avatar = new UserAvatar
                {
                    Type = MockAvatar.Type,
                    Data = MockAvatar.Data
                };
                await context.SaveChangesAsync();
            }

            (await _service.GetAvatar(username)).Avatar.Should().BeEquivalentTo(MockAvatar);
        }

        [Fact]
        public void SetAvatar_ShouldThrow_ArgumentException()
        {
            // no need to await because arguments are checked syncronizedly.
            _service.Invoking(s => s.SetAvatar(null, MockAvatar)).Should().Throw<ArgumentException>()
                .Where(e => e.ParamName == "username" && e.Message.Contains("null", StringComparison.OrdinalIgnoreCase));
            _service.Invoking(s => s.SetAvatar("", MockAvatar)).Should().Throw<ArgumentException>()
                .Where(e => e.ParamName == "username" && e.Message.Contains("empty", StringComparison.OrdinalIgnoreCase));

            _service.Invoking(s => s.SetAvatar("aaa", new Avatar { Type = null, Data = new[] { (byte)0x00 } })).Should().Throw<ArgumentException>()
                .Where(e => e.ParamName == "avatar" && e.Message.Contains("null", StringComparison.OrdinalIgnoreCase));
            _service.Invoking(s => s.SetAvatar("aaa", new Avatar { Type = "", Data = new[] { (byte)0x00 } })).Should().Throw<ArgumentException>()
                .Where(e => e.ParamName == "avatar" && e.Message.Contains("empty", StringComparison.OrdinalIgnoreCase));

            _service.Invoking(s => s.SetAvatar("aaa", new Avatar { Type = "aaa", Data = null })).Should().Throw<ArgumentException>()
                .Where(e => e.ParamName == "avatar" && e.Message.Contains("null", StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public void SetAvatar_ShouldThrow_UserNotExistException()
        {
            const string username = "usernotexist";
            _service.Awaiting(s => s.SetAvatar(username, MockAvatar)).Should().Throw<UserNotExistException>()
                .Where(e => e.Username == username);
        }

        [Fact]
        public async Task SetAvatar_Should_Work()
        {
            const string username = MockUsers.UserUsername;

            var user = await _database.DatabaseContext.Users.Where(u => u.Name == username).Include(u => u.Avatar).SingleAsync();

            // create
            await _service.SetAvatar(username, MockAvatar);
            user.Avatar.Should().NotBeNull();
            user.Avatar.Type.Should().Be(MockAvatar.Type);
            user.Avatar.Data.Should().Equal(MockAvatar.Data);

            // modify
            await _service.SetAvatar(username, MockAvatar2);
            user.Avatar.Should().NotBeNull();
            user.Avatar.Type.Should().Be(MockAvatar2.Type);
            user.Avatar.Data.Should().Equal(MockAvatar2.Data);

            // delete
            await _service.SetAvatar(username, null);
            user.Avatar.Type.Should().BeNull();
            user.Avatar.Data.Should().BeNull();
        }
    }
}
