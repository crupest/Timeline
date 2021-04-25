using Microsoft.Extensions.Logging.Abstractions;
using System.Threading.Tasks;
using Timeline.Entities;
using Timeline.Services.Timeline;
using Timeline.Services.User;
using Timeline.Tests.Helpers;
using Xunit;
using Xunit.Abstractions;

namespace Timeline.Tests.Services
{
    public abstract class ServiceTestBase : IAsyncLifetime
    {
        protected TestDatabase TestDatabase { get; }
        protected DatabaseContext Database { get; private set; } = default!;

        private readonly ITestOutputHelper? _testOutputHelper;

        protected TestClock Clock { get; } = new TestClock();
        protected UserService UserService { get; private set; } = default!;
        protected TimelineService TimelineService { get; private set; } = default!;

        protected long UserId { get; private set; }
        protected long AdminId { get; private set; }

        protected ServiceTestBase(ITestOutputHelper? testOutputHelper = null)
        {
            _testOutputHelper = testOutputHelper;
            TestDatabase = new TestDatabase();
        }

        public async Task InitializeAsync()
        {
            await TestDatabase.InitializeAsync();
            Database = TestDatabase.CreateContext(_testOutputHelper);

            UserService = new UserService(NullLogger<UserService>.Instance, Database, new PasswordService(), Clock);
            TimelineService = new TimelineService(Database, UserService, Clock);

            UserId = await UserService.GetUserIdByUsername("user");
            AdminId = await UserService.GetUserIdByUsername("admin");

            await OnInitializeAsync();
            OnInitialize();
        }

        public async Task DisposeAsync()
        {
            OnDispose();
            await OnDisposeAsync();
            await Database.DisposeAsync();
            await TestDatabase.DisposeAsync();
        }


        protected virtual void OnInitialize() { }
        protected virtual void OnDispose() { }


        protected virtual Task OnInitializeAsync()
        {
            return Task.CompletedTask;
        }

        protected virtual Task OnDisposeAsync()
        {
            return Task.CompletedTask;
        }
    }
}
