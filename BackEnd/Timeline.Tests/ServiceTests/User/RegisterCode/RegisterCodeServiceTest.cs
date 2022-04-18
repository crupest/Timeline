using System;
using System.Threading.Tasks;
using FluentAssertions;
using Timeline.Services.User;
using Timeline.Services.User.RegisterCode;
using Xunit;

namespace Timeline.Tests.ServiceTests.User.RegisterCode
{
    public class RegisterCodeServiceTest : ServiceTestBase
    {
        private RegisterCodeService _registerCodeService = default!;

        protected override void OnInitialize()
        {
            _registerCodeService = new RegisterCodeService(Database, UserService, Clock);
        }

        protected override void OnDispose()
        {
            _registerCodeService.Dispose();
        }

        [Fact]
        public async Task RegisterCodeTest()
        {
            var a = await _registerCodeService.GetCurrentCodeAsync(AdminId);
            a.Should().BeNull();
            var b = await _registerCodeService.CreateNewCodeAsync(AdminId);
            b.Should().NotBeNullOrEmpty();
            var c = await _registerCodeService.GetCurrentCodeAsync(AdminId);
            c.Should().Be(b);
            var d = await _registerCodeService.CreateNewCodeAsync(AdminId);
            d.Should().NotBe(b);
            var e = await _registerCodeService.GetCodeOwnerAsync(d);
            e.Should().Be(AdminId);
            var f = await _registerCodeService.GetCodeOwnerAsync(b);
            f.Should().BeNull();
            var g = await _registerCodeService.GetCodeOwnerAsync(b, false);
            g.Should().Be(AdminId);
        }

        [Fact]
        public async Task RegisterInfoTest()
        {
            var registerCode = await _registerCodeService.CreateNewCodeAsync(AdminId);

            var a = await _registerCodeService.GetUserRegisterInfoAsync(UserId);
            a.Should().BeNull();

            var dateTime = DateTime.UtcNow;

            var b = await _registerCodeService.CreateRegisterInfoAsync(UserId, registerCode, dateTime);
            b.UserId.Should().Be(UserId);
            b.RegisterCode.Should().Be(registerCode);
            b.IntroducerId.Should().Be(AdminId);
            b.RegisterTime.Should().Be(dateTime);

            var c = await _registerCodeService.GetUserRegisterInfoAsync(UserId);
            c.Should().NotBeNull();
            c!.UserId.Should().Be(UserId);
            c.RegisterCode.Should().Be(registerCode);
            c.IntroducerId.Should().Be(AdminId);
            c.RegisterTime.Should().Be(dateTime);
        }

        [Fact]
        public async Task RegisterInfoTestWithOldCode()
        {
            await _registerCodeService.CreateNewCodeAsync(AdminId);

            // Make the old code disabled.
            var registerCode = await _registerCodeService.CreateNewCodeAsync(AdminId);

            var a = await _registerCodeService.GetUserRegisterInfoAsync(UserId);
            a.Should().BeNull();

            var dateTime = DateTime.UtcNow;

            var b = await _registerCodeService.CreateRegisterInfoAsync(UserId, registerCode, dateTime);
            b.UserId.Should().Be(UserId);
            b.RegisterCode.Should().Be(registerCode);
            b.IntroducerId.Should().Be(AdminId);
            b.RegisterTime.Should().Be(dateTime);

            var c = await _registerCodeService.GetUserRegisterInfoAsync(UserId);
            c.Should().NotBeNull();
            c!.UserId.Should().Be(UserId);
            c.RegisterCode.Should().Be(registerCode);
            c.IntroducerId.Should().Be(AdminId);
            c.RegisterTime.Should().Be(dateTime);
        }

        [Fact]
        public async Task RegisterUserTest()
        {
            var registerCode = await _registerCodeService.CreateNewCodeAsync(AdminId);

            var a = await _registerCodeService.RegisterUserWithCode(new CreateUserParams("user2", "user2pw"), registerCode);
            a.Should().NotBeNull();

            var b = await _registerCodeService.GetUserRegisterInfoAsync(a.Id);
            b.Should().NotBeNull();
            b!.RegisterCode.Should().Be(registerCode);
            b.IntroducerId.Should().Be(AdminId);

            await _registerCodeService.CreateNewCodeAsync(AdminId);

            await _registerCodeService.Awaiting(s => s.RegisterUserWithCode(new CreateUserParams("user3", "user3pw"), registerCode))
                .Should().ThrowAsync<InvalidRegisterCodeException>();

        }
    }
}
