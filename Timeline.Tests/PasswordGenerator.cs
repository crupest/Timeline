using System;
using TimelineApp.Services;
using Xunit;
using Xunit.Abstractions;

namespace TimelineApp.Tests
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
