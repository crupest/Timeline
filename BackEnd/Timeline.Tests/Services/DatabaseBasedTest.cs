using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Timeline.Entities;
using Timeline.Tests.Helpers;
using Xunit;

namespace Timeline.Tests.Services
{
    public abstract class DatabaseBasedTest : IAsyncLifetime
    {
        protected TestDatabase TestDatabase { get; } = new TestDatabase();
        protected DatabaseContext Database { get; private set; }

        public async Task InitializeAsync()
        {
            await TestDatabase.InitializeAsync();
            Database = TestDatabase.CreateContext();
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
