using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Timeline.Entities;
using Timeline.Models.Validation;

namespace Timeline.Services.User
{
    public class BasicUserService : IBasicUserService
    {
        private readonly DatabaseContext _database;

        private readonly UsernameValidator _usernameValidator = new UsernameValidator();

        public BasicUserService(DatabaseContext database)
        {
            _database = database;
        }

        protected static EntityNotExistException CreateUserNotExistException(string username)
        {
            return new EntityNotExistException(EntityTypes.User,
                new Dictionary<string, object> { ["username"] = username });
        }

        protected static EntityNotExistException CreateUserNotExistException(long id)
        {
            return new EntityNotExistException(EntityTypes.User,
                new Dictionary<string, object> { ["id"] = id });
        }

        public async Task<bool> CheckUserExistenceAsync(long id)
        {
            return await _database.Users.AnyAsync(u => u.Id == id);
        }

        public async Task<long> GetUserIdByUsernameAsync(string username)
        {
            if (username == null)
                throw new ArgumentNullException(nameof(username));

            if (!_usernameValidator.Validate(username, out var message))
                throw new ArgumentException(message);

            var entity = await _database.Users.Where(user => user.Username == username).Select(u => new { u.Id }).SingleOrDefaultAsync();

            if (entity == null)
                throw CreateUserNotExistException(username);

            return entity.Id;
        }

        public async Task<DateTime> GetUsernameLastModifiedTimeAsync(long userId)
        {
            var entity = await _database.Users.Where(u => u.Id == userId).Select(u => new { u.UsernameChangeTime }).SingleOrDefaultAsync();

            if (entity is null)
                throw CreateUserNotExistException(userId);

            return entity.UsernameChangeTime;
        }
    }
}
