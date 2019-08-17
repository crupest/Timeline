using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using Timeline.Entities;
using Timeline.Models;
using Timeline.Models.Validation;
using static Timeline.Helpers.MyLogHelper;
using static Timeline.Models.UserUtility;

namespace Timeline.Services
{
    public class CreateTokenResult
    {
        public string Token { get; set; }
        public UserInfo User { get; set; }
    }

    [Serializable]
    public class UserNotExistException : Exception
    {
        private const string message = "The user does not exist.";

        public UserNotExistException(string username)
            : base(FormatLogMessage(message, Pair("Username", username)))
        {
            Username = username;
        }

        public UserNotExistException(long id)
            : base(FormatLogMessage(message, Pair("Id", id)))
        {
            Id = id;
        }

        public UserNotExistException(string message, Exception inner) : base(message, inner) { }

        protected UserNotExistException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }

        /// <summary>
        /// The username that does not exist. May be null then <see cref="Id"/> is not null.
        /// </summary>
        public string Username { get; private set; }

        /// <summary>
        /// The id that does not exist. May be null then <see cref="Username"/> is not null.
        /// </summary>
        public long? Id { get; private set; }
    }

    [Serializable]
    public class BadPasswordException : Exception
    {
        public BadPasswordException(string badPassword)
            : base(FormatLogMessage("Password is wrong.", Pair("Bad Password", badPassword)))
        {
            Password = badPassword;
        }

        public BadPasswordException(string message, Exception inner) : base(message, inner) { }

        protected BadPasswordException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }

        /// <summary>
        /// The wrong password.
        /// </summary>
        public string Password { get; private set; }
    }


    [Serializable]
    public class BadTokenVersionException : Exception
    {
        public BadTokenVersionException(long tokenVersion, long requiredVersion)
            : base(FormatLogMessage("Token version is expired.",
                Pair("Token Version", tokenVersion),
                Pair("Required Version", requiredVersion)))
        {
            TokenVersion = tokenVersion;
            RequiredVersion = requiredVersion;
        }

        public BadTokenVersionException(string message, Exception inner) : base(message, inner) { }

        protected BadTokenVersionException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }

        /// <summary>
        /// The version in the token.
        /// </summary>
        public long TokenVersion { get; private set; }

        /// <summary>
        /// The version required.
        /// </summary>
        public long RequiredVersion { get; private set; }
    }

    /// <summary>
    /// Thrown when username is of bad format.
    /// </summary>
    [Serializable]
    public class UsernameBadFormatException : Exception
    {
        public UsernameBadFormatException(string username, string message) : base(message) { Username = username; }
        public UsernameBadFormatException(string username, string message, Exception inner) : base(message, inner) { Username = username; }
        protected UsernameBadFormatException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }

        /// <summary>
        /// Username of bad format.
        /// </summary>
        public string Username { get; private set; }
    }


    /// <summary>
    /// Thrown when the user already exists.
    /// </summary>
    [Serializable]
    public class UserAlreadyExistException : Exception
    {
        public UserAlreadyExistException(string username) : base($"User {username} already exists.") { Username = username; }
        public UserAlreadyExistException(string username, string message) : base(message) { Username = username; }
        public UserAlreadyExistException(string message, Exception inner) : base(message, inner) { }
        protected UserAlreadyExistException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }

        /// <summary>
        /// The username that already exists.
        /// </summary>
        public string Username { get; set; }
    }

    public interface IUserService
    {
        /// <summary>
        /// Try to anthenticate with the given username and password.
        /// If success, create a token and return the user info.
        /// </summary>
        /// <param name="username">The username of the user to anthenticate.</param>
        /// <param name="password">The password of the user to anthenticate.</param>
        /// <param name="expires">The expired time point. Null then use default. See <see cref="JwtService.GenerateJwtToken(TokenInfo, DateTime?)"/> for what is default.</param>
        /// <returns>An <see cref="CreateTokenResult"/> containing the created token and user info.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="username"/> or <paramref name="password"/> is null.</exception>
        /// <exception cref="UserNotExistException">Thrown when the user with given username does not exist.</exception>
        /// <exception cref="BadPasswordException">Thrown when password is wrong.</exception>
        Task<CreateTokenResult> CreateToken(string username, string password, DateTime? expires = null);

        /// <summary>
        /// Verify the given token.
        /// If success, return the user info.
        /// </summary>
        /// <param name="token">The token to verify.</param>
        /// <returns>The user info specified by the token.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="token"/> is null.</exception>
        /// <exception cref="JwtTokenVerifyException">Thrown when the token is of bad format. Thrown by <see cref="JwtService.VerifyJwtToken(string)"/>.</exception>
        /// <exception cref="UserNotExistException">Thrown when the user specified by the token does not exist. Usually it has been deleted after the token was issued.</exception>
        /// <exception cref="BadTokenVersionException">Thrown when the version in the token is expired. User needs to recreate the token.</exception>
        Task<UserInfo> VerifyToken(string token);

        /// <summary>
        /// Get the user info of given username.
        /// </summary>
        /// <param name="username">Username of the user.</param>
        /// <returns>The info of the user. Null if the user of given username does not exists.</returns>
        Task<UserInfo> GetUser(string username);

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
        /// <returns>Return <see cref="PutResult.Created"/> if a new user is created.
        /// Return <see cref="PutResult.Modified"/> if a existing user is modified.</returns>
        /// <exception cref="UsernameBadFormatException">Thrown when <paramref name="username"/> is of bad format.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="username"/> or <paramref name="password"/> is null.</exception>
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
        /// <exception cref="UserNotExistException">Thrown if the user with given username does not exist.</exception>
        Task PatchUser(string username, string password, bool? administrator);

        /// <summary>
        /// Delete a user of given username.
        /// </summary>
        /// <param name="username">Username of thet user to delete. Can't be null.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="username"/> is null.</exception>
        /// <exception cref="UserNotExistException">Thrown if the user with given username does not exist.</exception>
        Task DeleteUser(string username);

        /// <summary>
        /// Try to change a user's password with old password.
        /// </summary>
        /// <param name="username">The name of user to change password of.</param>
        /// <param name="oldPassword">The user's old password.</param>
        /// <param name="newPassword">The user's new password.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="username"/> or <paramref name="oldPassword"/> or <paramref name="newPassword"/> is null.</exception>
        /// <exception cref="UserNotExistException">Thrown if the user with given username does not exist.</exception>
        /// <exception cref="BadPasswordException">Thrown if the old password is wrong.</exception>
        Task ChangePassword(string username, string oldPassword, string newPassword);

        /// <summary>
        /// Change a user's username.
        /// </summary>
        /// <param name="oldUsername">The user's old username.</param>
        /// <param name="newUsername">The new username.</param>
        /// <exception cref="ArgumentException">Thrown if <paramref name="oldUsername"/> or <paramref name="newUsername"/> is null or empty.</exception>
        /// <exception cref="UserNotExistException">Thrown if the user with old username does not exist.</exception>
        /// <exception cref="UsernameBadFormatException">Thrown if the new username is not accepted because of bad format.</exception>
        /// <exception cref="UserAlreadyExistException">Thrown if user with the new username already exists.</exception>
        Task ChangeUsername(string oldUsername, string newUsername);
    }

    internal class UserCache
    {
        public string Username { get; set; }
        public bool Administrator { get; set; }
        public long Version { get; set; }

        public UserInfo ToUserInfo()
        {
            return new UserInfo(Username, Administrator);
        }
    }

    public class UserService : IUserService
    {
        private readonly ILogger<UserService> _logger;

        private readonly IMemoryCache _memoryCache;
        private readonly DatabaseContext _databaseContext;

        private readonly IJwtService _jwtService;
        private readonly IPasswordService _passwordService;

        private readonly UsernameValidator _usernameValidator;

        public UserService(ILogger<UserService> logger, IMemoryCache memoryCache, DatabaseContext databaseContext, IJwtService jwtService, IPasswordService passwordService)
        {
            _logger = logger;
            _memoryCache = memoryCache;
            _databaseContext = databaseContext;
            _jwtService = jwtService;
            _passwordService = passwordService;

            _usernameValidator = new UsernameValidator();
        }

        private string GenerateCacheKeyByUserId(long id) => $"user:{id}";

        private void RemoveCache(long id)
        {
            var key = GenerateCacheKeyByUserId(id);
            _memoryCache.Remove(key);
            _logger.LogInformation(FormatLogMessage("A cache entry is removed.", Pair("Key", key)));
        }

        public async Task<CreateTokenResult> CreateToken(string username, string password, DateTime? expires)
        {
            if (username == null)
                throw new ArgumentNullException(nameof(username));
            if (password == null)
                throw new ArgumentNullException(nameof(password));

            // We need password info, so always check the database.
            var user = await _databaseContext.Users.Where(u => u.Name == username).SingleOrDefaultAsync();

            if (user == null)
                throw new UserNotExistException(username);

            if (!_passwordService.VerifyPassword(user.EncryptedPassword, password))
                throw new BadPasswordException(password);

            var token = _jwtService.GenerateJwtToken(new TokenInfo
            {
                Id = user.Id,
                Version = user.Version
            }, expires);

            return new CreateTokenResult
            {
                Token = token,
                User = CreateUserInfo(user)
            };
        }

        public async Task<UserInfo> VerifyToken(string token)
        {
            if (token == null)
                throw new ArgumentNullException(nameof(token));

            TokenInfo tokenInfo;
            tokenInfo = _jwtService.VerifyJwtToken(token);

            var id = tokenInfo.Id;
            var key = GenerateCacheKeyByUserId(id);
            if (!_memoryCache.TryGetValue<UserCache>(key, out var cache))
            {
                // no cache, check the database
                var user = await _databaseContext.Users.Where(u => u.Id == id).SingleOrDefaultAsync();

                if (user == null)
                    throw new UserNotExistException(id);

                // create cache
                cache = CreateUserCache(user);
                _memoryCache.CreateEntry(key).SetValue(cache);
                _logger.LogInformation(FormatLogMessage("A cache entry is created.", Pair("Key", key)));
            }

            if (tokenInfo.Version != cache.Version)
                throw new BadTokenVersionException(tokenInfo.Version, cache.Version);

            return cache.ToUserInfo();
        }

        public async Task<UserInfo> GetUser(string username)
        {
            return await _databaseContext.Users
                .Where(user => user.Name == username)
                .Select(user => CreateUserInfo(user))
                .SingleOrDefaultAsync();
        }

        public async Task<UserInfo[]> ListUsers()
        {
            return await _databaseContext.Users
                .Select(user => CreateUserInfo(user))
                .ToArrayAsync();
        }

        public async Task<PutResult> PutUser(string username, string password, bool administrator)
        {
            if (username == null)
                throw new ArgumentNullException(nameof(username));
            if (password == null)
                throw new ArgumentNullException(nameof(password));

            if (!_usernameValidator.Validate(username, out var message))
            {
                throw new UsernameBadFormatException(username, message);
            }

            var user = await _databaseContext.Users.Where(u => u.Name == username).SingleOrDefaultAsync();

            if (user == null)
            {
                var newUser = new User
                {
                    Name = username,
                    EncryptedPassword = _passwordService.HashPassword(password),
                    RoleString = IsAdminToRoleString(administrator),
                    Version = 0
                };
                await _databaseContext.AddAsync(newUser);
                await _databaseContext.SaveChangesAsync();
                _logger.LogInformation(FormatLogMessage("A new user entry is added to the database.", Pair("Id", newUser.Id)));
                return PutResult.Created;
            }

            user.EncryptedPassword = _passwordService.HashPassword(password);
            user.RoleString = IsAdminToRoleString(administrator);
            user.Version += 1;
            await _databaseContext.SaveChangesAsync();
            _logger.LogInformation(FormatLogMessage("A user entry is updated to the database.", Pair("Id", user.Id)));

            //clear cache
            RemoveCache(user.Id);

            return PutResult.Modified;
        }

        public async Task PatchUser(string username, string password, bool? administrator)
        {
            if (username == null)
                throw new ArgumentNullException(nameof(username));

            var user = await _databaseContext.Users.Where(u => u.Name == username).SingleOrDefaultAsync();
            if (user == null)
                throw new UserNotExistException(username);

            if (password != null)
            {
                user.EncryptedPassword = _passwordService.HashPassword(password);
            }

            if (administrator != null)
            {
                user.RoleString = IsAdminToRoleString(administrator.Value);
            }

            user.Version += 1;
            await _databaseContext.SaveChangesAsync();
            _logger.LogInformation(FormatLogMessage("A user entry is updated to the database.", Pair("Id", user.Id)));

            //clear cache
            RemoveCache(user.Id);
        }

        public async Task DeleteUser(string username)
        {
            if (username == null)
                throw new ArgumentNullException(nameof(username));

            var user = await _databaseContext.Users.Where(u => u.Name == username).SingleOrDefaultAsync();
            if (user == null)
                throw new UserNotExistException(username);

            _databaseContext.Users.Remove(user);
            await _databaseContext.SaveChangesAsync();
            _logger.LogInformation(FormatLogMessage("A user entry is removed from the database.", Pair("Id", user.Id)));

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

            var user = await _databaseContext.Users.Where(u => u.Name == username).SingleOrDefaultAsync();
            if (user == null)
                throw new UserNotExistException(username);

            var verifyResult = _passwordService.VerifyPassword(user.EncryptedPassword, oldPassword);
            if (!verifyResult)
                throw new BadPasswordException(oldPassword);

            user.EncryptedPassword = _passwordService.HashPassword(newPassword);
            user.Version += 1;
            await _databaseContext.SaveChangesAsync();
            //clear cache
            RemoveCache(user.Id);
        }

        public async Task ChangeUsername(string oldUsername, string newUsername)
        {
            if (string.IsNullOrEmpty(oldUsername))
                throw new ArgumentException("Old username is null or empty", nameof(oldUsername));
            if (string.IsNullOrEmpty(newUsername))
                throw new ArgumentException("New username is null or empty", nameof(newUsername));

            if (!_usernameValidator.Validate(newUsername, out var message))
                throw new UsernameBadFormatException(newUsername, $"New username is of bad format. {message}");

            var user = await _databaseContext.Users.Where(u => u.Name == oldUsername).SingleOrDefaultAsync();
            if (user == null)
                throw new UserNotExistException(oldUsername);

            var conflictUser = await _databaseContext.Users.Where(u => u.Name == newUsername).SingleOrDefaultAsync();
            if (conflictUser != null)
                throw new UserAlreadyExistException(newUsername);

            user.Name = newUsername;
            user.Version += 1;
            await _databaseContext.SaveChangesAsync();
            _logger.LogInformation(FormatLogMessage("A user entry changed name field.",
                Pair("Id", user.Id), Pair("Old Username", oldUsername), Pair("New Username", newUsername)));
            RemoveCache(user.Id);
        }
    }
}
