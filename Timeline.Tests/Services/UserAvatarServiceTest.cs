using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using SixLabors.ImageSharp.Formats.Png;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Timeline.Entities;
using Timeline.Services;
using Timeline.Tests.Helpers;
using Timeline.Tests.Mock.Data;
using Xunit;

namespace Timeline.Tests.Services
{
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
                    .Should().Throw<AvatarFormatException>()
                    .Where(e => e.Avatar == avatar && e.Error == AvatarFormatException.ErrorReason.CantDecode);
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
                    .Should().Throw<AvatarFormatException>()
                    .Where(e => e.Avatar == avatar && e.Error == AvatarFormatException.ErrorReason.UnmatchedFormat);
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
                    .Should().Throw<AvatarFormatException>()
                    .Where(e => e.Avatar == avatar && e.Error == AvatarFormatException.ErrorReason.BadSize);
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

    public class UserAvatarServiceTest : IDisposable
    {
        private UserAvatar CreateMockAvatarEntity(string key) => new UserAvatar
        {
            Type = $"image/test{key}",
            Data = Encoding.ASCII.GetBytes($"mock{key}"),
            ETag = $"etag{key}",
            LastModified = DateTime.Now
        };

        private AvatarInfo CreateMockAvatarInfo(string key) => new AvatarInfo
        {
            Avatar = new Avatar
            {
                Type = $"image/test{key}",
                Data = Encoding.ASCII.GetBytes($"mock{key}")
            },
            LastModified = DateTime.Now
        };

        private Avatar CreateMockAvatar(string key) => new Avatar
        {
            Type = $"image/test{key}",
            Data = Encoding.ASCII.GetBytes($"mock{key}")
        };

        private static Avatar ToAvatar(UserAvatar entity)
        {
            return new Avatar
            {
                Data = entity.Data,
                Type = entity.Type
            };
        }

        private static AvatarInfo ToAvatarInfo(UserAvatar entity)
        {
            return new AvatarInfo
            {
                Avatar = ToAvatar(entity),
                LastModified = entity.LastModified
            };
        }

        private readonly Mock<IDefaultUserAvatarProvider> _mockDefaultAvatarProvider;
        private readonly Mock<IUserAvatarValidator> _mockValidator;
        private readonly Mock<IETagGenerator> _mockETagGenerator;
        private readonly Mock<IClock> _mockClock;

        private readonly TestDatabase _database;

        private readonly UserAvatarService _service;

        public UserAvatarServiceTest()
        {
            _mockDefaultAvatarProvider = new Mock<IDefaultUserAvatarProvider>();
            _mockValidator = new Mock<IUserAvatarValidator>();
            _mockETagGenerator = new Mock<IETagGenerator>();
            _mockClock = new Mock<IClock>();

            _database = new TestDatabase();

            _service = new UserAvatarService(NullLogger<UserAvatarService>.Instance, _database.DatabaseContext, _mockDefaultAvatarProvider.Object, _mockValidator.Object, _mockETagGenerator.Object, _mockClock.Object);
        }

        public void Dispose()
        {
            _database.Dispose();
        }

        [Theory]
        [InlineData(null, typeof(ArgumentNullException))]
        [InlineData("", typeof(UsernameBadFormatException))]
        [InlineData("a!a", typeof(UsernameBadFormatException))]
        [InlineData("usernotexist", typeof(UserNotExistException))]
        public async Task GetAvatarETag_ShouldThrow(string username, Type exceptionType)
        {
            await _service.Awaiting(s => s.GetAvatarETag(username)).Should().ThrowAsync(exceptionType);
        }

        [Fact]
        public async Task GetAvatarETag_ShouldReturn_Default()
        {
            const string etag = "aaaaaa";
            _mockDefaultAvatarProvider.Setup(p => p.GetDefaultAvatarETag()).ReturnsAsync(etag);
            (await _service.GetAvatarETag(MockUser.User.Username)).Should().Be(etag);
        }

        [Fact]
        public async Task GetAvatarETag_ShouldReturn_Data()
        {
            string username = MockUser.User.Username;
            var mockAvatarEntity = CreateMockAvatarEntity("aaa");
            {
                var context = _database.DatabaseContext;
                var user = await context.Users.Where(u => u.Name == username).Include(u => u.Avatar).SingleAsync();
                user.Avatar = mockAvatarEntity;
                await context.SaveChangesAsync();
            }
            (await _service.GetAvatarETag(username)).Should().BeEquivalentTo(mockAvatarEntity.ETag);
        }

        [Theory]
        [InlineData(null, typeof(ArgumentNullException))]
        [InlineData("", typeof(UsernameBadFormatException))]
        [InlineData("a!a", typeof(UsernameBadFormatException))]
        [InlineData("usernotexist", typeof(UserNotExistException))]
        public async Task GetAvatar_ShouldThrow(string username, Type exceptionType)
        {
            await _service.Awaiting(s => s.GetAvatar(username)).Should().ThrowAsync(exceptionType);

        }

        [Fact]
        public async Task GetAvatar_ShouldReturn_Default()
        {
            var mockAvatar = CreateMockAvatarInfo("aaa");
            _mockDefaultAvatarProvider.Setup(p => p.GetDefaultAvatar()).ReturnsAsync(mockAvatar);
            string username = MockUser.User.Username;
            (await _service.GetAvatar(username)).Should().BeEquivalentTo(mockAvatar);
        }

        [Fact]
        public async Task GetAvatar_ShouldReturn_Data()
        {
            string username = MockUser.User.Username;
            var mockAvatarEntity = CreateMockAvatarEntity("aaa");
            {
                var context = _database.DatabaseContext;
                var user = await context.Users.Where(u => u.Name == username).Include(u => u.Avatar).SingleAsync();
                user.Avatar = mockAvatarEntity;
                await context.SaveChangesAsync();
            }

            (await _service.GetAvatar(username)).Should().BeEquivalentTo(ToAvatarInfo(mockAvatarEntity));
        }

        public static IEnumerable<object[]> SetAvatar_ShouldThrow_Data()
        {
            yield return new object[] { null, null, typeof(ArgumentNullException) };
            yield return new object[] { "", null, typeof(UsernameBadFormatException) };
            yield return new object[] { "u!u", null, typeof(UsernameBadFormatException) };
            yield return new object[] { null, new Avatar { Type = null, Data = new[] { (byte)0x00 } }, typeof(ArgumentException) };
            yield return new object[] { null, new Avatar { Type = "", Data = new[] { (byte)0x00 } }, typeof(ArgumentException) };
            yield return new object[] { null, new Avatar { Type = "aaa", Data = null }, typeof(ArgumentException) };
            yield return new object[] { "usernotexist", null, typeof(UserNotExistException) };
        }

        [Theory]
        [MemberData(nameof(SetAvatar_ShouldThrow_Data))]
        public async Task SetAvatar_ShouldThrow(string username, Avatar avatar, Type exceptionType)
        {
            await _service.Awaiting(s => s.SetAvatar(username, avatar)).Should().ThrowAsync(exceptionType);
        }

        [Fact]
        public async Task SetAvatar_Should_Work()
        {
            string username = MockUser.User.Username;

            var user = await _database.DatabaseContext.Users.Where(u => u.Name == username).Include(u => u.Avatar).SingleAsync();

            var avatar1 = CreateMockAvatar("aaa");
            var avatar2 = CreateMockAvatar("bbb");

            string etag1 = "etagaaa";
            string etag2 = "etagbbb";

            DateTime dateTime1 = DateTime.Now.AddSeconds(2);
            DateTime dateTime2 = DateTime.Now.AddSeconds(10);
            DateTime dateTime3 = DateTime.Now.AddSeconds(20);

            // create
            _mockETagGenerator.Setup(g => g.Generate(avatar1.Data)).ReturnsAsync(etag1);
            _mockClock.Setup(c => c.GetCurrentTime()).Returns(dateTime1);
            await _service.SetAvatar(username, avatar1);
            user.Avatar.Should().NotBeNull();
            user.Avatar.Type.Should().Be(avatar1.Type);
            user.Avatar.Data.Should().Equal(avatar1.Data);
            user.Avatar.ETag.Should().Be(etag1);
            user.Avatar.LastModified.Should().Be(dateTime1);

            // modify
            _mockETagGenerator.Setup(g => g.Generate(avatar2.Data)).ReturnsAsync(etag2);
            _mockClock.Setup(c => c.GetCurrentTime()).Returns(dateTime2);
            await _service.SetAvatar(username, avatar2);
            user.Avatar.Should().NotBeNull();
            user.Avatar.Type.Should().Be(avatar2.Type);
            user.Avatar.Data.Should().Equal(avatar2.Data);
            user.Avatar.ETag.Should().Be(etag2);
            user.Avatar.LastModified.Should().Be(dateTime2);

            // delete
            _mockClock.Setup(c => c.GetCurrentTime()).Returns(dateTime3);
            await _service.SetAvatar(username, null);
            user.Avatar.Type.Should().BeNull();
            user.Avatar.Data.Should().BeNull();
            user.Avatar.ETag.Should().BeNull();
            user.Avatar.LastModified.Should().Be(dateTime3);
        }
    }
}
