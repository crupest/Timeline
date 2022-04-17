using System.Threading.Tasks;
using FluentAssertions;
using Timeline.Services.User.RegisterCode;
using Xunit;

namespace Timeline.Tests.ServiceTests.User.RegisterCode
{
    public class RegisterCodeServiceTest : ServiceTestBase
    {
        private RegisterCodeService _registerCodeService = default!;

        protected override void OnInitialize()
        {
            _registerCodeService = new RegisterCodeService(Database);
        }

        protected override void OnDispose()
        {
            _registerCodeService.Dispose();
        }

        [Fact]
        public async Task Test()
        {
            var a = await _registerCodeService.GetCurrentCode(AdminId);
            a.Should().BeNull();
            var b = await _registerCodeService.CreateNewCode(AdminId);
            b.Should().NotBeNullOrEmpty();
            var c = await _registerCodeService.GetCurrentCode(AdminId);
            c.Should().Be(b);
            var d = await _registerCodeService.CreateNewCode(AdminId);
            d.Should().NotBe(b);
            var e = await _registerCodeService.GetCodeOwner(d);
            e.Should().Be(AdminId);
            var f = await _registerCodeService.GetCodeOwner(b);
            f.Should().BeNull();
            var g = await _registerCodeService.GetCodeOwner(b, false);
            g.Should().Be(AdminId);
        }
    }
}

