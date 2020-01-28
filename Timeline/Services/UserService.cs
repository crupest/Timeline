using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Timeline.Entities;
using Timeline.Helpers;
using Timeline.Models;
using Timeline.Models.Validation;
using static Timeline.Resources.Services.UserService;

namespace Timeline.Services
{
    public interface IUserService
    {
        /// <summary>
        /// Try to verify the given username and password.
        /// </summary>
        /// <param name="username">The username of the user to verify.</param>
        /// <param name="password">The password of the user to verify.</param>
        /// <returns>The user info and auth info.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="username"/> or <paramref name="password"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when username is of bad format.</exception>
        /// <exception cref="UserNotExistException">Thrown when the user with given username does not exist.</exception>
        /// <exception cref="BadPasswordException">Thrown when password is wrong.</exception>
        Task<User> VerifyCredential(string username, string password);

        /// <summary>
        /// Try to get a user by id.
        /// </summary>
        /// <param name="id">The id of the user.</param>
        /// <returns>The user info.</returns>
        /// <exception cref="UserNotExistException">Thrown when the user with given id does not exist.</exception>
        Task<User> GetUserById(long id);

        /// <summary>
        /// Get the user info of given username.
        /// </summary>
        /// <param name="username">Username of the user.</param>
        /// <returns>The info of the user.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="username"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="username"/> is of bad format.</exception>
        /// <exception cref="UserNotExistException">Thrown when the user with given username does not exist.</exception>
        Task<User> GetUserByUsername(string username);

        /// <summary>
        /// List all users.
        /// </summary>
        /// <returns>The user info of users.</returns>
        Task<User[]> ListUsers();

        /// <summary>
        /// Create a user with given info.
        /// </summary>
        /// <param name="info">The info of new user.</param>
        /// <param name="password">The password, can't be null or empty.</param>
        /// <returns>The id of the new user.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="info"/>is null.</exception>
        /// <exception cref="ArgumentException">Thrown when some fields in <paramref name="info"/> is bad.</exception>
        /// <exception cref="UsernameConfictException">Thrown when a user with given username already exists.</exception>
        /// <remarks>
        /// <see cref="User.Username"/> must not be null and must be a valid username.
        /// <see cref="User.Password"/> must not be null or empty.
        /// <see cref="User.Administrator"/> is false by default (null).
        /// Other fields are ignored.
        /// </remarks>
        Task<long> CreateUser(User info);

        /// <summary>
        /// Modify a user's info.
        /// </summary>
        /// <param name="id">The id of the user.</param>
        /// <param name="info">The new info. May be null.</param>
        /// <exception cref="ArgumentException">Thrown when some fields in <paramref name="info"/> is bad.</exception>
        /// <exception cref="UserNotExistException">Thrown when user with given id does not exist.</exception>
        /// <remarks>
        /// Only <see cref="User.Administrator"/>, <see cref="User.Password"/> and <see cref="User.Nickname"/> will be used.
        /// If null, then not change.
        /// Other fields are ignored.
        /// After modified, even if nothing is changed, version will increase.
        /// 
        /// <see cref="User.Password"/> can't be empty.
        /// 
        /// Note: Whether <see cref="User.Version"/> is set or not, version will increase and not set to the specified value if there is one.
        /// </remarks>
        Task ModifyUser(long id, User? info);

        /// <summary>
        /// Partially modify a user of given username.
        /// 
        /// Note that whether actually modified or not, Version of the user will always increase.
        /// </summary>
        /// <param name="username">Username of the user to modify. Can't be null.</param>
        /// <param name="password">New password. Null if not modify.</param>
        /// <param name="administrator">Whether the user is administrator. Null if not modify.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="username"/> is null.</exception>
        /// <exception cref="UsernameBadFormatException">Thrown when <paramref name="username"/> is of bad format.</exception>
        /// <exception cref="UserNotExistException">Thrown if the user with given username does not exist.</exception>
        Task PatchUser(string username, string? password, bool? administrator);

        /// <summary>
        /// Delete a user of given username.
        /// </summary>
        /// <param name="username">Username of thet user to delete. Can't be null.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="username"/> is null.</exception>
        /// <exception cref="UsernameBadFormatException">Thrown when <paramref name="username"/> is of bad format.</exception>
        /// <exception cref="UserNotExistException">Thrown if the user with given username does not exist.</exception>
        Task DeleteUser(string username);

        /// <summary>
        /// Try to change a user's password with old password.
        /// </summary>
        /// <param name="username">The name of user to change password of.</param>
        /// <param name="oldPassword">The user's old password.</param>
        /// <param name="newPassword">The user's new password.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="username"/> or <paramref name="oldPassword"/> or <paramref name="newPassword"/> is null.</exception>
        /// <exception cref="UsernameBadFormatException">Thrown when <paramref name="username"/> is of bad format.</exception>
        /// <exception cref="UserNotExistException">Thrown if the user with given username does not exist.</exception>
        /// <exception cref="BadPasswordException">Thrown if the old password is wrong.</exception>
        Task ChangePassword(string username, string oldPassword, string newPassword);

        /// <summary>
        /// Change a user's username.
        /// </summary>
        /// <param name="oldUsername">The user's old username.</param>
        /// <param name="newUsername">The new username.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="oldUsername"/> or <paramref name="newUsername"/> is null.</exception>
        /// <exception cref="UserNotExistException">Thrown if the user with old username does not exist.</exception>
        /// <exception cref="UsernameBadFormatException">Thrown if the <paramref name="oldUsername"/> or <paramref name="newUsername"/> is of bad format.</exception>
        /// <exception cref="UsernameConfictException">Thrown if user with the new username already exists.</exception>
        Task ChangeUsername(string oldUsername, string newUsername);
    }

    public class UserService : IUserService
    {
        private readonly ILogger<UserService> _logger;

        private readonly DatabaseContext _databaseContext;


        private readonly IPasswordService _passwordService;

        private readonly UsernameValidator _usernameValidator = new UsernameValidator();

        public UserService(ILogger<UserService> logger, DatabaseContext databaseContext, IPasswordService passwordService)
        {
            _logger = logger;
            _databaseContext = databaseContext;
            _passwordService = passwordService;
        }

        private void CheckUsernameFormat(string username, string? paramName, Func<string, string>? messageBuilder = null)
        {
            if (!_usernameValidator.Validate(username, out var message))
            {
                if (messageBuilder == null)
                    throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, ExceptionUsernameBadFormat, message), paramName);
                else
                    throw new ArgumentException(messageBuilder(message), paramName);
            }
        }

        private static User CreateUserFromEntity(UserEntity entity)
        {
            return new User
            {
                Username = entity.Username,
                Administrator = UserRoleConvert.ToBool(entity.Roles),
                Nickname = string.IsNullOrEmpty(entity.Nickname) ? entity.Username : entity.Nickname,
                Version = entity.Version
            };
        }

        public async Task<User> VerifyCredential(string username, string password)
        {
            if (username == null)
                throw new ArgumentNullException(nameof(username));
            if (password == null)
                throw new ArgumentNullException(nameof(password));

            CheckUsernameFormat(username, nameof(username));

            var entity = await _databaseContext.Users.Where(u => u.Username == username).SingleOrDefaultAsync();

            if (entity == null)
                throw new UserNotExistException(username);

            if (!_passwordService.VerifyPassword(entity.Password, password))
                throw new BadPasswordException(password);

            return CreateUserFromEntity(entity);
        }

        public async Task<User> GetUserById(long id)
        {
            var user = await _databaseContext.Users.Where(u => u.Id == id).SingleOrDefaultAsync();

            if (user == null)
                throw new UserNotExistException(id);

            return CreateUserFromEntity(user);
        }

        public async Task<User> GetUserByUsername(string username)
        {
            if (username == null)
                throw new ArgumentNullException(nameof(username));

            CheckUsernameFormat(username, nameof(username));

            var entity = await _databaseContext.Users.Where(user => user.Username == username).SingleOrDefaultAsync();

            if (entity == null)
                throw new UserNotExistException(username);

            return CreateUserFromEntity(entity);
        }

        public async Task<User[]> ListUsers()
        {
            var entities = await _databaseContext.Users.ToArrayAsync();
            return entities.Select(user => CreateUserFromEntity(user)).ToArray();
        }

        public async Task<long> CreateUser(User info)
        {
            if (info == null)
                throw new ArgumentNullException(nameof(info));

            if (string.IsNullOrEmpty(info.Username))
                throw new ArgumentException(ExceptionUsernameNullOrEmpty, nameof(info));

            CheckUsernameFormat(info.Username, nameof(info));

            if (string.IsNullOrEmpty(info.Password))
                throw new ArgumentException(ExceptionPasswordNullOrEmpty);

            var username = info.Username;

            var conflict = await _databaseContext.Users.AnyAsync(u => u.Username == username);

            if (conflict)
                throw new UsernameConfictException(username);

            var administrator = info.Administrator ?? false;
            var password = info.Password;

            var newEntity = new UserEntity
            {
                Username = username,
                Password = _passwordService.HashPassword(password),
                Roles = UserRoleConvert.ToString(administrator),
                Version = 1
            };
            _databaseContext.Users.Add(newEntity);
            await _databaseContext.SaveChangesAsync();

            _logger.LogInformation(Log.Format(LogDatabaseCreate,
                ("Id", newEntity.Id), ("Username", username), ("Administrator", administrator)));

            return newEntity.Id;
        }

        public async Task ModifyUser(long id, User? info)
        {
            if (info != null && info.Password != null && info.Password.Length == 0)
                throw new ArgumentException(ExceptionPasswordEmpty, nameof(info));

            var entity = await _databaseContext.Users.Where(u => u.Id == id).SingleOrDefaultAsync();
            if (entity == null)
                throw new UserNotExistException(id);

            if (info != null)
            {
                var password = info.Password;
                if (password != null)
                {
                    entity.Password = _passwordService.HashPassword(password);
                }

                var administrator = info.Administrator;
                if (administrator.HasValue)
                {
                    entity.Roles = UserRoleConvert.ToString(administrator.Value);
                }

                var nickname = info.Nickname;
                if (nickname != null)
                {
                    entity.Nickname = nickname;
                }
            }

            entity.Version += 1;

            await _databaseContext.SaveChangesAsync();
            _logger.LogInformation(LogDatabaseUpdate, ("Id", id));
        }

        public async Task DeleteUser(string username)
        {
            if (username == null)
                throw new ArgumentNullException(nameof(username));
            CheckUsernameFormat(username);

            var user = await _databaseContext.Users.Where(u => u.Username == username).SingleOrDefaultAsync();
            if (user == null)
                throw new UserNotExistException(username);

            _databaseContext.Users.Remove(user);
            await _databaseContext.SaveChangesAsync();
            _logger.LogInformation(Log.Format(Resources.Services.UserService.LogDatabaseRemove,
                ("Id", user.Id)));

            //clear cache
            await _cache.RemoveCache(user.Id);
        }

        public async Task ChangePassword(string username, string oldPassword, string newPassword)
        {
            if (username == null)
                throw new ArgumentNullException(nameof(username));
            if (oldPassword == null)
                throw new ArgumentNullException(nameof(oldPassword));
            if (newPassword == null)
                throw new ArgumentNullException(nameof(newPassword));
            CheckUsernameFormat(username);

            var user = await _databaseContext.Users.Where(u => u.Username == username).SingleOrDefaultAsync();
            if (user == null)
                throw new UserNotExistException(username);

            var verifyResult = _passwordService.VerifyPassword(user.Password, oldPassword);
            if (!verifyResult)
                throw new BadPasswordException(oldPassword);

            user.Password = _passwordService.HashPassword(newPassword);
            user.Version += 1;
            await _databaseContext.SaveChangesAsync();
            _logger.LogInformation(Log.Format(Resources.Services.UserService.LogDatabaseUpdate,
                ("Id", user.Id), ("Operation", "Change password")));
            //clear cache
            await _cache.RemoveCache(user.Id);
        }

        public async Task ChangeUsername(string oldUsername, string newUsername)
        {
            if (oldUsername == null)
                throw new ArgumentNullException(nameof(oldUsername));
            if (newUsername == null)
                throw new ArgumentNullException(nameof(newUsername));
            CheckUsernameFormat(oldUsername, Resources.Services.UserService.ExceptionOldUsernameBadFormat);
            CheckUsernameFormat(newUsername, Resources.Services.UserService.ExceptionNewUsernameBadFormat);

            var user = await _databaseContext.Users.Where(u => u.Username == oldUsername).SingleOrDefaultAsync();
            if (user == null)
                throw new UserNotExistException(oldUsername);

            var conflictUser = await _databaseContext.Users.Where(u => u.Username == newUsername).SingleOrDefaultAsync();
            if (conflictUser != null)
                throw new UsernameConfictException(newUsername);

            user.Username = newUsername;
            user.Version += 1;
            await _databaseContext.SaveChangesAsync();
            _logger.LogInformation(Log.Format(Resources.Services.UserService.LogDatabaseUpdate,
                ("Id", user.Id), ("Old Username", oldUsername), ("New Username", newUsername)));
            await _cache.RemoveCache(user.Id);
        }


    }
}
