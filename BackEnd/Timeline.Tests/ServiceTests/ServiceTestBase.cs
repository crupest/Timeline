using Microsoft.Extensions.Logging.Abstractions;
using System.Threading.Tasks;
using Timeline.Entities;
using Timeline.Services.User;
using Timeline.Tests.Helpers;
using Xunit;
using Xunit.Abstractions;

namespace Timeline.Tests.ServiceTests
{
    public abstract class ServiceTestBase : IAsyncLifetime
    {
        private readonly ITestOutputHelper? _testOutputHelper;

        protected TestDatabase TestDatabase { get; }
        protected DatabaseContext Database { get; private set; } = default!;

        protected TestClock Clock { get; } = new TestClock();
        protected UserService UserService { get; private set; } = default!;

        protected long AdminId { get; private set; }
        protected long UserId { get; private set; }

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

            AdminId = await UserService.GetUserIdByUsernameAsync("admin");
            UserId = await UserService.GetUserIdByUsernameAsync("user");

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
