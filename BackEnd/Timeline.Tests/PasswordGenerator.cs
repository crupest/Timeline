using Timeline.Services.User;
using Xunit;
using Xunit.Abstractions;

namespace Timeline.Tests
{
    public class PasswordGenerator
    {
        private readonly ITestOutputHelper _output;

        public PasswordGenerator(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void Generate()
        {
            var service = new PasswordService();
            _output.WriteLine(service.HashPassword("crupest"));
        }
    }
}
