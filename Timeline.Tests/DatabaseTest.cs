using FluentAssertions;
using System;
using System.Linq;
using Timeline.Entities;
using Timeline.Tests.Helpers;
using Xunit;

namespace Timeline.Tests
{
    public class DatabaseTest : IDisposable
    {
        private readonly TestDatabase _database;
        private readonly DatabaseContext _context;

        public DatabaseTest()
        {
            _database = new TestDatabase();
            _context = _database.Context;
        }

        public void Dispose()
        {
            _database.Dispose();
        }

        [Fact]
        public void DeleteUserShouldAlsoDeleteAvatar()
        {
            var user = _context.Users.First();
            _context.UserAvatars.Count().Should().Be(0);
            _context.UserAvatars.Add(new UserAvatar
            {
                Data = null,
                Type = null,
                ETag = null,
                LastModified = DateTime.Now,
                UserId = user.Id
            });
            _context.SaveChanges();
            _context.UserAvatars.Count().Should().Be(1);
            _context.Users.Remove(user);
            _context.SaveChanges();
            _context.UserAvatars.Count().Should().Be(0);
        }

        [Fact]
        public void DeleteUserShouldAlsoDeleteDetail()
        {
            var user = _context.Users.First();
            _context.UserDetails.Count().Should().Be(0);
            _context.UserDetails.Add(new UserDetail
            {
                Nickname = null,
                UserId = user.Id
            });
            _context.SaveChanges();
            _context.UserDetails.Count().Should().Be(1);
            _context.Users.Remove(user);
            _context.SaveChanges();
            _context.UserDetails.Count().Should().Be(0);
        }
    }
}
