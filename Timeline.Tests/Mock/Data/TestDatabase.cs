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
            context.Users.AddRange(MockUsers.CreateMockUsers());
            context.SaveChanges();
        }

        private readonly SqliteConnection _databaseConnection;
        private readonly DatabaseContext _databaseContext;

        public TestDatabase()
        {
            _databaseConnection = new SqliteConnection("Data Source=:memory:;");
            _databaseConnection.Open();

            var options = new DbContextOptionsBuilder<DatabaseContext>()
                .UseSqlite(_databaseConnection)
                .Options;

            _databaseContext = new DatabaseContext(options);

            InitDatabase(_databaseContext);
        }

        public void Dispose()
        {
            _databaseContext.Dispose();

            _databaseConnection.Close();
            _databaseConnection.Dispose();
        }

        public SqliteConnection DatabaseConnection => _databaseConnection;
        public DatabaseContext DatabaseContext => _databaseContext;
    }
}
