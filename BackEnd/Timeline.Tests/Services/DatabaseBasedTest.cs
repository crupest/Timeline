using System.Threading.Tasks;
using Timeline.Entities;
using Timeline.Tests.Helpers;
using Xunit;
using Xunit.Abstractions;

namespace Timeline.Tests.Services
{
    public abstract class DatabaseBasedTest : IAsyncLifetime
    {
        protected TestDatabase TestDatabase { get; }
        protected DatabaseContext Database { get; private set; } = default!;

        private readonly ITestOutputHelper? _testOutputHelper;

        protected DatabaseBasedTest(bool databaseCreateUsers = true, ITestOutputHelper? testOutputHelper = null)
        {
            _testOutputHelper = testOutputHelper;
            TestDatabase = new TestDatabase(databaseCreateUsers);
        }

        protected DatabaseBasedTest(ITestOutputHelper? testOutputHelper) : this(true, testOutputHelper) { }

        public async Task InitializeAsync()
        {
            await TestDatabase.InitializeAsync();
            Database = TestDatabase.CreateContext(_testOutputHelper);
            await OnDatabaseCreatedAsync();
            OnDatabaseCreated();
        }

        public async Task DisposeAsync()
        {
            BeforeDatabaseDestroy();
            await BeforeDatabaseDestroyAsync();
            await Database.DisposeAsync();
            await TestDatabase.DisposeAsync();
        }


        protected virtual void OnDatabaseCreated() { }
        protected virtual void BeforeDatabaseDestroy() { }


        protected virtual Task OnDatabaseCreatedAsync()
        {
            return Task.CompletedTask;
        }

        protected virtual Task BeforeDatabaseDestroyAsync()
        {
            return Task.CompletedTask;
        }
    }
}
