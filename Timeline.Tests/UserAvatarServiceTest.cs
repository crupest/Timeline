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
        public static string ETag { get; } = "Hahaha";

        public static AvatarInfo AvatarInfo { get; } = new AvatarInfo
        {
            Avatar = new Avatar { Type = "image/test", Data = Encoding.ASCII.GetBytes("test") },
            LastModified = DateTime.Now
        };

        public Task<string> GetDefaultAvatarETag()
        {
            return Task.FromResult(ETag);
        }

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
        private UserAvatar MockAvatarEntity1 { get; } = new UserAvatar
        {
            Type = "image/testaaa",
            Data = Encoding.ASCII.GetBytes("amock"),
            ETag = "aaaa",
            LastModified = DateTime.Now
        };

        private UserAvatar MockAvatarEntity2 { get; } = new UserAvatar
        {
            Type = "image/testbbb",
            Data = Encoding.ASCII.GetBytes("bmock"),
            ETag = "bbbb",
            LastModified = DateTime.Now + TimeSpan.FromMinutes(1)
        };

        private Avatar ToAvatar(UserAvatar entity)
        {
            return new Avatar
            {
                Data = entity.Data,
                Type = entity.Type
            };
        }

        private AvatarInfo ToAvatarInfo(UserAvatar entity)
        {
            return new AvatarInfo
            {
                Avatar = ToAvatar(entity),
                LastModified = entity.LastModified
            };
        }

        private void Set(UserAvatar to, UserAvatar from)
        {
            to.Type = from.Type;
            to.Data = from.Data;
            to.ETag = from.ETag;
            to.LastModified = from.LastModified;
        }

        private readonly MockDefaultUserAvatarProvider _mockDefaultUserAvatarProvider;

        private readonly ILoggerFactory _loggerFactory;
        private readonly TestDatabase _database;

        private readonly IETagGenerator _eTagGenerator;

        private readonly UserAvatarService _service;

        public UserAvatarServiceTest(ITestOutputHelper outputHelper, MockDefaultUserAvatarProvider mockDefaultUserAvatarProvider, MockUserAvatarValidator mockUserAvatarValidator)
        {
            _mockDefaultUserAvatarProvider = mockDefaultUserAvatarProvider;

            _loggerFactory = Logging.Create(outputHelper);
            _database = new TestDatabase();

            _eTagGenerator = new ETagGenerator();

            _service = new UserAvatarService(_loggerFactory.CreateLogger<UserAvatarService>(), _database.DatabaseContext, _mockDefaultUserAvatarProvider, mockUserAvatarValidator, _eTagGenerator);
        }

        public void Dispose()
        {
            _loggerFactory.Dispose();
            _database.Dispose();
        }

        [Fact]
        public void GetAvatarETag_ShouldThrow_ArgumentException()
        {
            // no need to await because arguments are checked syncronizedly.
            _service.Invoking(s => s.GetAvatarETag(null)).Should().Throw<ArgumentException>()
                .Where(e => e.ParamName == "username" && e.Message.Contains("null", StringComparison.OrdinalIgnoreCase));
            _service.Invoking(s => s.GetAvatarETag("")).Should().Throw<ArgumentException>()
                .Where(e => e.ParamName == "username" && e.Message.Contains("empty", StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public void GetAvatarETag_ShouldThrow_UserNotExistException()
        {
            const string username = "usernotexist";
            _service.Awaiting(s => s.GetAvatarETag(username)).Should().Throw<UserNotExistException>()
                .Where(e => e.Username == username);
        }

        [Fact]
        public async Task GetAvatarETag_ShouldReturn_Default()
        {
            const string username = MockUsers.UserUsername;
            (await _service.GetAvatarETag(username)).Should().BeEquivalentTo((await _mockDefaultUserAvatarProvider.GetDefaultAvatarETag()));
        }

        [Fact]
        public async Task GetAvatarETag_ShouldReturn_Data()
        {
            const string username = MockUsers.UserUsername;
            {
                // create mock data
                var context = _database.DatabaseContext;
                var user = await context.Users.Where(u => u.Name == username).Include(u => u.Avatar).SingleAsync();
                Set(user.Avatar, MockAvatarEntity1);
                await context.SaveChangesAsync();
            }

            (await _service.GetAvatarETag(username)).Should().BeEquivalentTo(MockAvatarEntity1.ETag);
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
                Set(user.Avatar, MockAvatarEntity1);
                await context.SaveChangesAsync();
            }

            (await _service.GetAvatar(username)).Should().BeEquivalentTo(ToAvatarInfo(MockAvatarEntity1));
        }

        [Fact]
        public void SetAvatar_ShouldThrow_ArgumentException()
        {
            var avatar = ToAvatar(MockAvatarEntity1);
            // no need to await because arguments are checked syncronizedly.
            _service.Invoking(s => s.SetAvatar(null, avatar)).Should().Throw<ArgumentException>()
                .Where(e => e.ParamName == "username" && e.Message.Contains("null", StringComparison.OrdinalIgnoreCase));
            _service.Invoking(s => s.SetAvatar("", avatar)).Should().Throw<ArgumentException>()
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
            _service.Awaiting(s => s.SetAvatar(username, ToAvatar(MockAvatarEntity1))).Should().Throw<UserNotExistException>()
                .Where(e => e.Username == username);
        }

        [Fact]
        public async Task SetAvatar_Should_Work()
        {
            const string username = MockUsers.UserUsername;

            var user = await _database.DatabaseContext.Users.Where(u => u.Name == username).Include(u => u.Avatar).SingleAsync();

            // create
            var avatar1 = ToAvatar(MockAvatarEntity1);
            await _service.SetAvatar(username, avatar1);
            user.Avatar.Should().NotBeNull();
            user.Avatar.Type.Should().Be(avatar1.Type);
            user.Avatar.Data.Should().Equal(avatar1.Data);
            user.Avatar.ETag.Should().NotBeNull();

            // modify
            var avatar2 = ToAvatar(MockAvatarEntity2);
            await _service.SetAvatar(username, avatar2);
            user.Avatar.Should().NotBeNull();
            user.Avatar.Type.Should().Be(MockAvatarEntity2.Type);
            user.Avatar.Data.Should().Equal(MockAvatarEntity2.Data);
            user.Avatar.ETag.Should().NotBeNull();

            // delete
            await _service.SetAvatar(username, null);
            user.Avatar.Type.Should().BeNull();
            user.Avatar.Data.Should().BeNull();
            user.Avatar.ETag.Should().BeNull();
        }
    }
}
