using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Timeline.Entities;
using Timeline.Helpers;
using Timeline.Models;
using Timeline.Models.Validation;
using Timeline.Services.Exceptions;
using static Timeline.Resources.Services.UserService;

namespace Timeline.Services
{
    /// <summary>
    /// Null means not change.
    /// </summary>
    public record ModifyUserParams
    {
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string? Nickname { get; set; }
    }

    public interface IUserService : IBasicUserService
    {
        /// <summary>
        /// Try to get a user by id.
        /// </summary>
        /// <param name="id">The id of the user.</param>
        /// <returns>The user info.</returns>
        /// <exception cref="UserNotExistException">Thrown when the user with given id does not exist.</exception>
        Task<User> GetUser(long id);

        /// <summary>
        /// List all users.
        /// </summary>
        /// <returns>The user info of users.</returns>
        Task<List<User>> GetUsers();

        /// <summary>
        /// Create a user with given info.
        /// </summary>
        /// <param name="username">The username of new user.</param>
        /// <param name="password">The password of new user.</param>
        /// <returns>The the new user.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="username"/> or <paramref name="password"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="username"/> or <paramref name="password"/> is of bad format.</exception>
        /// <exception cref="EntityAlreadyExistException">Thrown when a user with given username already exists.</exception>
        Task<User> CreateUser(string username, string password);

        /// <summary>
        /// Modify a user.
        /// </summary>
        /// <param name="id">The id of the user.</param>
        /// <param name="param">The new information.</param>
        /// <returns>The new user info.</returns>
        /// <exception cref="ArgumentException">Thrown when some fields in <paramref name="param"/> is bad.</exception>
        /// <exception cref="UserNotExistException">Thrown when user with given id does not exist.</exception>
        /// <remarks>
        /// Version will increase if password is changed.
        /// </remarks>
        Task<User> ModifyUser(long id, ModifyUserParams? param);
    }

    public class UserService : BasicUserService, IUserService
    {
        private readonly ILogger<UserService> _logger;
        private readonly IClock _clock;

        private readonly DatabaseContext _databaseContext;

        private readonly IPasswordService _passwordService;
        private readonly IUserPermissionService _userPermissionService;

        private readonly UsernameValidator _usernameValidator = new UsernameValidator();
        private readonly NicknameValidator _nicknameValidator = new NicknameValidator();

        public UserService(ILogger<UserService> logger, DatabaseContext databaseContext, IPasswordService passwordService, IClock clock, IUserPermissionService userPermissionService) : base(databaseContext)
        {
            _logger = logger;
            _clock = clock;
            _databaseContext = databaseContext;
            _passwordService = passwordService;
            _userPermissionService = userPermissionService;
        }

        private void CheckUsernameFormat(string username, string? paramName)
        {
            if (!_usernameValidator.Validate(username, out var message))
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, ExceptionUsernameBadFormat, message), paramName);
            }
        }

        private static void CheckPasswordFormat(string password, string? paramName)
        {
            if (password.Length == 0)
            {
                throw new ArgumentException(ExceptionPasswordEmpty, paramName);
            }
        }

        private void CheckNicknameFormat(string nickname, string? paramName)
        {
            if (!_nicknameValidator.Validate(nickname, out var message))
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, ExceptionNicknameBadFormat, message), paramName);
            }
        }

        private static void ThrowUsernameConflict()
        {
            throw new EntityAlreadyExistException(EntityNames.User, ExceptionUsernameConflict);
        }

        private async Task<User> CreateUserFromEntity(UserEntity entity)
        {
            var permission = await _userPermissionService.GetPermissionsOfUserAsync(entity.Id);
            return new User
            {
                UniqueId = entity.UniqueId,
                Username = entity.Username,
                Permissions = permission,
                Nickname = string.IsNullOrEmpty(entity.Nickname) ? entity.Username : entity.Nickname,
                Id = entity.Id,
                Version = entity.Version,
                CreateTime = entity.CreateTime,
                UsernameChangeTime = entity.UsernameChangeTime,
                LastModified = entity.LastModified
            };
        }



        public async Task<User> GetUser(long id)
        {
            var user = await _databaseContext.Users.Where(u => u.Id == id).SingleOrDefaultAsync();

            if (user == null)
                throw new UserNotExistException(id);

            return await CreateUserFromEntity(user);
        }

        public async Task<List<User>> GetUsers()
        {
            List<User> result = new();
            foreach (var entity in await _databaseContext.Users.ToArrayAsync())
            {
                result.Add(await CreateUserFromEntity(entity));
            }
            return result;
        }

        public async Task<User> CreateUser(string username, string password)
        {
            if (username == null)
                throw new ArgumentNullException(nameof(username));
            if (password == null)
                throw new ArgumentNullException(nameof(password));

            CheckUsernameFormat(username, nameof(username));
            CheckPasswordFormat(password, nameof(password));

            var conflict = await _databaseContext.Users.AnyAsync(u => u.Username == username);
            if (conflict)
                ThrowUsernameConflict();

            var newEntity = new UserEntity
            {
                Username = username,
                Password = _passwordService.HashPassword(password),
                Version = 1
            };
            _databaseContext.Users.Add(newEntity);
            await _databaseContext.SaveChangesAsync();

            _logger.LogInformation(Log.Format(LogDatabaseCreate, ("Id", newEntity.Id), ("Username", username)));

            return await CreateUserFromEntity(newEntity);
        }

        public async Task<User> ModifyUser(long id, ModifyUserParams? param)
        {
            if (param != null)
            {
                if (param.Username != null)
                    CheckUsernameFormat(param.Username, nameof(param));

                if (param.Password != null)
                    CheckPasswordFormat(param.Password, nameof(param));

                if (param.Nickname != null)
                    CheckNicknameFormat(param.Nickname, nameof(param));
            }

            var entity = await _databaseContext.Users.Where(u => u.Id == id).SingleOrDefaultAsync();
            if (entity == null)
                throw new UserNotExistException(id);

            if (param != null)
            {
                var now = _clock.GetCurrentTime();
                bool updateLastModified = false;

                var username = param.Username;
                if (username != null && username != entity.Username)
                {
                    var conflict = await _databaseContext.Users.AnyAsync(u => u.Username == username);
                    if (conflict)
                        ThrowUsernameConflict();

                    entity.Username = username;
                    entity.UsernameChangeTime = now;
                    updateLastModified = true;
                }

                var password = param.Password;
                if (password != null)
                {
                    entity.Password = _passwordService.HashPassword(password);
                    entity.Version += 1;
                }

                var nickname = param.Nickname;
                if (nickname != null && nickname != entity.Nickname)
                {
                    entity.Nickname = nickname;
                    updateLastModified = true;
                }

                if (updateLastModified)
                {
                    entity.LastModified = now;
                }

                await _databaseContext.SaveChangesAsync();
                _logger.LogInformation(LogDatabaseUpdate, ("Id", id));
            }

            return await CreateUserFromEntity(entity);
        }
    }
}
