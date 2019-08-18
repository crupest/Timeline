using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System;
using Timeline.Entities;

namespace Timeline.Tests.Mock.Data
{
    public class TestDatabase : IDisposable
    {
        private readonly SqliteConnection _databaseConnection;
        private readonly DatabaseContext _databaseContext;

        public TestDatabase()
        {
            _databaseConnection = new SqliteConnection("Data Source=:memory:;");
            _databaseConnection.Open();

            var options = new DbContextOptionsBuilder<DatabaseContext>()
                .UseSqlite(_databaseConnection)
                .ConfigureWarnings(builder =>
                {
                    builder.Throw(RelationalEventId.QueryClientEvaluationWarning);
                })
                .Options;

            _databaseContext = new DatabaseContext(options);

            // init with mock data
            _databaseContext.Database.EnsureCreated();
            _databaseContext.Users.AddRange(MockUsers.Users);
            _databaseContext.SaveChanges();
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
