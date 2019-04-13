using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
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
                    .UseEnvironment(EnvironmentConstants.TestEnvironmentName)
                    .ConfigureLogging(logging =>
                    {
                        logging.AddXunit(outputHelper);
                    })
                    .ConfigureServices(services =>
                    {
                        var serviceProvider = new ServiceCollection()
                            .AddEntityFrameworkInMemoryDatabase()
                            .BuildServiceProvider();

                        services.AddDbContext<DatabaseContext>(options =>
                        {
                            options.UseInMemoryDatabase("timeline");
                            options.UseInternalServiceProvider(serviceProvider);
                        });

                        var sp = services.BuildServiceProvider();

                        // Create a scope to obtain a reference to the database
                        // context (ApplicationDbContext).
                        using (var scope = sp.CreateScope())
                        {
                            var scopedServices = scope.ServiceProvider;
                            var db = scopedServices.GetRequiredService<DatabaseContext>();

                            var passwordService = new PasswordService(null);

                            // Ensure the database is created.
                            db.Database.EnsureCreated();

                            db.Users.AddRange(new User[] {
                                new User
                                {
                                    Id = 0,
                                    Name = "user",
                                    EncryptedPassword = passwordService.HashPassword("user"),
                                    RoleString = "user"
                                },
                                new User
                                {
                                    Id = 0,
                                    Name = "admin",
                                    EncryptedPassword = passwordService.HashPassword("admin"),
                                    RoleString = "user,admin"
                                }
                            });

                            db.SaveChanges();
                        }
                    });
            });
        }
    }
}
