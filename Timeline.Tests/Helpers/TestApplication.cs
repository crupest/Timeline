using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using Timeline.Entities;
using Timeline.Tests.Mock.Data;

namespace Timeline.Tests.Helpers
{
    public class TestApplication : IDisposable
    {
        public TestDatabase Database { get; } = new TestDatabase();
        public WebApplicationFactory<Startup> Factory { get; }

        public TestApplication(WebApplicationFactory<Startup> factory)
        {
            Factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.AddEntityFrameworkSqlite();
                    services.AddDbContext<DatabaseContext>(options =>
                    {
                        options.UseSqlite(Database.Connection);
                    });
                });
            });
        }

        public void Dispose()
        {
            Database.Dispose();
        }
    }
}
