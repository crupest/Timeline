using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using Timeline.Entities;
using Timeline.Helpers;
using Timeline.Models;
using Timeline.Models.Validation;

namespace Timeline.Services
{
    public interface IUserService
    {
        /// <summary>
        /// Try to verify the given username and password.
        /// </summary>
        /// <param name="username">The username of the user to verify.</param>
        /// <param name="password">The password of the user to verify.</param>
        /// <returns>The user info.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="username"/> or <paramref name="password"/> is null.</exception>
        /// <exception cref="UsernameBadFormatException">Thrown when username is of bad format.</exception>
        /// <exception cref="UserNotExistException">Thrown when the user with given username does not exist.</exception>
        /// <exception cref="BadPasswordException">Thrown when password is wrong.</exception>
        Task<UserInfo> VerifyCredential(string username, string password);

        /// <summary>
        /// Try to get a user by id.
        /// </summary>
        /// <param name="id">The id of the user.</param>
        /// <returns>The user info.</returns>
        /// <exception cref="UserNotExistException">Thrown when the user with given id does not exist.</exception>
        Task<UserInfo> GetUserById(long id);

        /// <summary>
        /// Get the user info of given username.
        /// </summary>
        /// <param name="username">Username of the user.</param>
        /// <returns>The info of the user.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="username"/> is null.</exception>
        /// <exception cref="UsernameBadFormatException">Thrown when <paramref name="username"/> is of bad format.</exception>
        /// <exception cref="UserNotExistException">Thrown when the user with given username does not exist.</exception>
        Task<UserInfo> GetUserByUsername(string username);

        /// <summary>
        /// List all users.
        /// </summary>
        /// <returns>The user info of users.</returns>
        Task<UserInfo[]> ListUsers();

        /// <summary>
        /// Create or modify a user with given username.
        /// Username must be match with [a-zA-z0-9-_].
        /// </summary>
        /// <param name="username">Username of user.</param>
        /// <param name="password">Password of user.</param>
        /// <param name="administrator">Whether the user is administrator.</param>
        /// <returns>
        /// Return <see cref="PutResult.Create"/> if a new user is created.
        /// Return <see cref="PutResult.Modify"/> if a existing user is modified.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="username"/> or <paramref name="password"/> is null.</exception>
        /// <exception cref="UsernameBadFormatException">Thrown when <paramref name="username"/> is of bad format.</exception>
        Task<PutResult> PutUser(string username, string password, bool administrator);

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

        private readonly IMemoryCache _memoryCache;

        private readonly IPasswordService _passwordService;

        private readonly UsernameValidator _usernameValidator = new UsernameValidator();

        public UserService(ILogger<UserService> logger, IMemoryCache memoryCache, DatabaseContext databaseContext, IPasswordService passwordService)
        {
            _logger = logger;
            _memoryCache = memoryCache;
            _databaseContext = databaseContext;
            _passwordService = passwordService;
        }

        private static string GenerateCacheKeyByUserId(long id) => $"user:{id}";

        private void RemoveCache(long id)
        {
            var key = GenerateCacheKeyByUserId(id);
            _memoryCache.Remove(key);
            _logger.LogInformation(Log.Format(Resources.Services.UserService.LogCacheRemove, ("Key", key)));
        }

        private void CheckUsernameFormat(string username, string? message = null)
        {
            var (result, validationMessage) = _usernameValidator.Validate(username);
            if (!result)
            {
                if (message == null)
                    throw new UsernameBadFormatException(username, validationMessage);
                else
                    throw new UsernameBadFormatException(username, validationMessage, message);
            }
        }

        private static UserInfo CreateUserInfoFromEntity(UserEntity user)
        {
            return new UserInfo
            {
                Id = user.Id,
                Username = user.Name,
                Administrator = UserRoleConvert.ToBool(user.RoleString),
                Version = user.Version
            };
        }

        public async Task<UserInfo> VerifyCredential(string username, string password)
        {
            if (username == null)
                throw new ArgumentNullException(nameof(username));
            if (password == null)
                throw new ArgumentNullException(nameof(password));

            CheckUsernameFormat(username);

            // We need password info, so always check the database.
            var user = await _databaseContext.Users.Where(u => u.Name == username).SingleOrDefaultAsync();

            if (user == null)
                throw new UserNotExistException(username);

            if (!_passwordService.VerifyPassword(user.EncryptedPassword, password))
                throw new BadPasswordException(password);

            return CreateUserInfoFromEntity(user);
        }

        public async Task<UserInfo> GetUserById(long id)
        {
            var key = GenerateCacheKeyByUserId(id);
            if (!_memoryCache.TryGetValue<UserInfo>(key, out var cache))
            {
                // no cache, check the database
                var user = await _databaseContext.Users.Where(u => u.Id == id).SingleOrDefaultAsync();

                if (user == null)
                    throw new UserNotExistException(id);

                // create cache
                cache = CreateUserInfoFromEntity(user);
                _memoryCache.CreateEntry(key).SetValue(cache);
                _logger.LogInformation(Log.Format(Resources.Services.UserService.LogCacheCreate, ("Key", key)));
            }

            return cache;
        }

        public async Task<UserInfo> GetUserByUsername(string username)
        {
            if (username == null)
                throw new ArgumentNullException(nameof(username));

            CheckUsernameFormat(username);

            var entity = await _databaseContext.Users
                .Where(user => user.Name == username)
                .SingleOrDefaultAsync();

            if (entity == null)
                throw new UserNotExistException(username);

            return CreateUserInfoFromEntity(entity);
        }

        public async Task<UserInfo[]> ListUsers()
        {
            var entities = await _databaseContext.Users.ToArrayAsync();
            return entities.Select(user => CreateUserInfoFromEntity(user)).ToArray();
        }

        public async Task<PutResult> PutUser(string username, string password, bool administrator)
        {
            if (username == null)
                throw new ArgumentNullException(nameof(username));
            if (password == null)
                throw new ArgumentNullException(nameof(password));
            CheckUsernameFormat(username);

            var user = await _databaseContext.Users.Where(u => u.Name == username).SingleOrDefaultAsync();

            if (user == null)
            {
                var newUser = new UserEntity
                {
                    Name = username,
                    EncryptedPassword = _passwordService.HashPassword(password),
                    RoleString = UserRoleConvert.ToString(administrator),
                    Avatar = null
                };
                await _databaseContext.AddAsync(newUser);
                await _databaseContext.SaveChangesAsync();
                _logger.LogInformation(Log.Format(Resources.Services.UserService.LogDatabaseCreate,
                    ("Id", newUser.Id), ("Username", username), ("Administrator", administrator)));
                return PutResult.Create;
            }

            user.EncryptedPassword = _passwordService.HashPassword(password);
            user.RoleString = UserRoleConvert.ToString(administrator);
            user.Version += 1;
            await _databaseContext.SaveChangesAsync();
            _logger.LogInformation(Log.Format(Resources.Services.UserService.LogDatabaseUpdate,
                ("Id", user.Id), ("Username", username), ("Administrator", administrator)));

            //clear cache
            RemoveCache(user.Id);

            return PutResult.Modify;
        }

        public async Task PatchUser(string username, string? password, bool? administrator)
        {
            if (username == null)
                throw new ArgumentNullException(nameof(username));
            CheckUsernameFormat(username);

            var user = await _databaseContext.Users.Where(u => u.Name == username).SingleOrDefaultAsync();
            if (user == null)
                throw new UserNotExistException(username);

            if (password != null)
            {
                user.EncryptedPassword = _passwordService.HashPassword(password);
            }

            if (administrator != null)
            {
                user.RoleString = UserRoleConvert.ToString(administrator.Value);
            }

            user.Version += 1;
            await _databaseContext.SaveChangesAsync();
            _logger.LogInformation(Resources.Services.UserService.LogDatabaseUpdate, ("Id", user.Id));

            //clear cache
            RemoveCache(user.Id);
        }

        public async Task DeleteUser(string username)
        {
            if (username == null)
                throw new ArgumentNullException(nameof(username));
            CheckUsernameFormat(username);

            var user = await _databaseContext.Users.Where(u => u.Name == username).SingleOrDefaultAsync();
            if (user == null)
                throw new UserNotExistException(username);

            _databaseContext.Users.Remove(user);
            await _databaseContext.SaveChangesAsync();
            _logger.LogInformation(Log.Format(Resources.Services.UserService.LogDatabaseRemove,
                ("Id", user.Id)));

            //clear cache
            RemoveCache(user.Id);
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

            var user = await _databaseContext.Users.Where(u => u.Name == username).SingleOrDefaultAsync();
            if (user == null)
                throw new UserNotExistException(username);

            var verifyResult = _passwordService.VerifyPassword(user.EncryptedPassword, oldPassword);
            if (!verifyResult)
                throw new BadPasswordException(oldPassword);

            user.EncryptedPassword = _passwordService.HashPassword(newPassword);
            user.Version += 1;
            await _databaseContext.SaveChangesAsync();
            _logger.LogInformation(Log.Format(Resources.Services.UserService.LogDatabaseUpdate,
                ("Id", user.Id), ("Operation", "Change password")));
            //clear cache
            RemoveCache(user.Id);
        }

        public async Task ChangeUsername(string oldUsername, string newUsername)
        {
            if (oldUsername == null)
                throw new ArgumentNullException(nameof(oldUsername));
            if (newUsername == null)
                throw new ArgumentNullException(nameof(newUsername));
            CheckUsernameFormat(oldUsername, Resources.Services.UserService.ExceptionOldUsernameBadFormat);
            CheckUsernameFormat(newUsername, Resources.Services.UserService.ExceptionNewUsernameBadFormat);

            var user = await _databaseContext.Users.Where(u => u.Name == oldUsername).SingleOrDefaultAsync();
            if (user == null)
                throw new UserNotExistException(oldUsername);

            var conflictUser = await _databaseContext.Users.Where(u => u.Name == newUsername).SingleOrDefaultAsync();
            if (conflictUser != null)
                throw new UsernameConfictException(newUsername);

            user.Name = newUsername;
            user.Version += 1;
            await _databaseContext.SaveChangesAsync();
            _logger.LogInformation(Log.Format(Resources.Services.UserService.LogDatabaseUpdate,
                ("Id", user.Id), ("Old Username", oldUsername), ("New Username", newUsername)));
            RemoveCache(user.Id);
        }
    }
}
