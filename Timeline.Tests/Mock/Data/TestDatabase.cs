using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System;
using Timeline.Entities;

namespace Timeline.Tests.Mock.Data
{
    public class TestDatabase : IDisposable
    {
        public static void InitDatabase(DatabaseContext context)
        {
            context.Database.EnsureCreated();
            context.Users.AddRange(MockUser.CreateMockEntities());
            context.SaveChanges();
        }

        public TestDatabase()
        {
            DatabaseConnection = new SqliteConnection("Data Source=:memory:;");
            DatabaseConnection.Open();

            var options = new DbContextOptionsBuilder<DatabaseContext>()
                .UseSqlite(DatabaseConnection)
                .Options;

            DatabaseContext = new DatabaseContext(options);

            InitDatabase(DatabaseContext);
        }

        public void Dispose()
        {
            DatabaseContext.Dispose();

            DatabaseConnection.Close();
            DatabaseConnection.Dispose();
        }

        public SqliteConnection DatabaseConnection { get; }
        public DatabaseContext DatabaseContext { get; }
    }
}
