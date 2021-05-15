using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using Timeline.Services;
using Xunit;
using Xunit.Abstractions;

namespace Timeline.Tests.IntegratedTests
{
    public class DatabaseManagementTest : IntegratedTestBase
    {
        public DatabaseManagementTest(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {

        }

        [Fact]
        public void Backup_Should_Work()
        {
            var pathProvider = TestApp.Host.Services.GetRequiredService<IPathProvider>();
            var backupDir = pathProvider.GetDatabaseBackupDirectory();
            Directory.GetFiles(backupDir).Should().NotBeEmpty();
        }
    }
}
