using Microsoft.AspNetCore.Mvc.Testing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Timeline.Tests.Helpers;
using Timeline.Tests.Mock.Data;
using Xunit;

namespace Timeline.Tests.IntegratedTests
{
    public abstract class IntegratedTestBase : IClassFixture<WebApplicationFactory<Startup>>, IDisposable
    {
        protected TestApplication TestApp { get; }

        protected WebApplicationFactory<Startup> Factory => TestApp.Factory;

        public IntegratedTestBase(WebApplicationFactory<Startup> factory)
        {
            TestApp = new TestApplication(factory);
        }

        public virtual void Dispose()
        {
            TestApp.Dispose();
        }

        protected void CreateExtraMockUsers(int count)
        {
            TestApp.Database.CreateExtraMockUsers(count);
        }

        protected IReadOnlyList<MockUser> ExtraMockUsers => TestApp.Database.ExtraMockUsers;
    }
}
