using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using Timeline.Entities;
using Timeline.Models;
using Timeline.Services;

namespace Timeline.Tests.Helpers
{
    public class TestDatabase : IDisposable
    {
        // currently password service is thread safe, so we share a static one.
        private static PasswordService PasswordService { get; } = new PasswordService();

        private static User CreateEntityFromMock(MockUser user)
        {
            return new User
            {
                Name = user.Username,
                EncryptedPassword = PasswordService.HashPassword(user.Password),
                RoleString = UserRoleConvert.ToString(user.Administrator),
                Avatar = null
            };
        }

        private static IEnumerable<User> CreateDefaultMockEntities()
        {
            // emmmmmmm. Never reuse the user instances because EF Core uses them, which will cause strange things.
            yield return CreateEntityFromMock(MockUser.User);
            yield return CreateEntityFromMock(MockUser.Admin);
        }

        private static void InitDatabase(DatabaseContext context)
        {
            context.Database.EnsureCreated();
            context.Users.AddRange(CreateDefaultMockEntities());
            context.SaveChanges();
        }

        public SqliteConnection Connection { get; }
        public DatabaseContext Context { get; }

        public TestDatabase()
        {
            Connection = new SqliteConnection("Data Source=:memory:;");
            Connection.Open();

            var options = new DbContextOptionsBuilder<DevelopmentDatabaseContext>()
                .UseSqlite(Connection)
                .Options;

            Context = new DevelopmentDatabaseContext(options);

            InitDatabase(Context);
        }

        private List<MockUser> _extraMockUsers;

        public IReadOnlyList<MockUser> ExtraMockUsers => _extraMockUsers;

        public void CreateExtraMockUsers(int count)
        {
            if (count <= 0)
                throw new ArgumentOutOfRangeException(nameof(count), count, "Additional user count must be bigger than 0.");
            if (_extraMockUsers != null)
                throw new InvalidOperationException("Already create mock users.");

            _extraMockUsers = new List<MockUser>();
            for (int i = 0; i < count; i++)
            {
                _extraMockUsers.Add(new MockUser($"user{i}", $"password", false));
            }

            Context.AddRange(_extraMockUsers.Select(u => CreateEntityFromMock(u)));
            Context.SaveChanges();
        }

        public void Dispose()
        {
            Context.Dispose();

            Connection.Close();
            Connection.Dispose();
        }

    }
}
