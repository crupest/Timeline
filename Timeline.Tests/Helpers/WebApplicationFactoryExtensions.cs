using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Timeline.Models;
using Timeline.Services;
using Xunit.Abstractions;

namespace Timeline.Tests.Helpers
{
    public static class WebApplicationFactoryExtensions
    {
        public static WebApplicationFactory<TEntry> WithTestConfig<TEntry>(this WebApplicationFactory<TEntry> factory, ITestOutputHelper outputHelper) where TEntry : class
        {
            return factory.WithWebHostBuilder(builder =>
            {
                builder
                    .ConfigureLogging(logging =>
                    {
                        logging.AddXunit(outputHelper);
                    })
                    .ConfigureServices(services =>
                    {
                        var serviceProvider = new ServiceCollection()
                            .AddEntityFrameworkSqlite()
                            .BuildServiceProvider();

                        services.AddDbContext<DatabaseContext>(options =>
                        {
                            options.UseSqlite("Data Source=:memory:;"); //TODO! This not work!
                            options.UseInternalServiceProvider(serviceProvider);
                        });

                        var sp = services.BuildServiceProvider();

                        // Create a scope to obtain a reference to the database
                        // context (ApplicationDbContext).
                        using (var scope = sp.CreateScope())
                        {
                            var scopedServices = scope.ServiceProvider;
                            var db = scopedServices.GetRequiredService<DatabaseContext>();

                            // Ensure the database is created.
                            db.Database.EnsureCreated();

                            db.Users.AddRange(TestMockUsers.MockUsers);
                            db.SaveChanges();
                        }
                    })
                    .ConfigureTestServices(services =>
                    {
                        services.AddSingleton<IClock, TestClock>();
                    });
            });
        }
    }
}
