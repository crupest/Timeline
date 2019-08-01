using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using Timeline.Entities;
using Timeline.Models;
using static Timeline.Entities.UserUtility;

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
        public UserNotExistException(): base("The user does not exist.") { }
        public UserNotExistException(string message) : base(message) { }
        public UserNotExistException(string message, Exception inner) : base(message, inner) { }
        protected UserNotExistException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

    [Serializable]
    public class BadPasswordException : Exception
    {
        public BadPasswordException(): base("Password is wrong.") { }
        public BadPasswordException(string message) : base(message) { }
        public BadPasswordException(string message, Exception inner) : base(message, inner) { }
        protected BadPasswordException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }


    [Serializable]
    public class BadTokenVersionException : Exception
    {
        public BadTokenVersionException(): base("Token version is expired.") { }
        public BadTokenVersionException(string message) : base(message) { }
        public BadTokenVersionException(string message, Exception inner) : base(message, inner) { }
        protected BadTokenVersionException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

    public interface IUserService
    {
        /// <summary>
        /// Try to anthenticate with the given username and password.
        /// If success, create a token and return the user info.
        /// </summary>
        /// <param name="username">The username of the user to anthenticate.</param>
        /// <param name="password">The password of the user to anthenticate.</param>
        /// <returns>An <see cref="CreateTokenResult"/> containing the created token and user info.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="username"/> or <paramref name="password"/> is null.</exception>
        /// <exception cref="UserNotExistException">Thrown when the user with given username does not exist.</exception>
        /// <exception cref="BadPasswordException">Thrown when password is wrong.</exception>
        Task<CreateTokenResult> CreateToken(string username, string password);

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
        /// Return <see cref="PutUserResult.Created"/> if a new user is created.
        /// Return <see cref="PutUserResult.Modified"/> if a existing user is modified.
        /// </summary>
        /// <param name="username">Username of user.</param>
        /// <param name="password">Password of user.</param>
        /// <param name="administrator">Whether the user is administrator.</param>
        /// <returns>Return <see cref="PutResult.Created"/> if a new user is created.
        /// Return <see cref="PutResult.Modified"/> if a existing user is modified.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="username"/> or <paramref name="password"/> is null.</exception>
        Task<PutResult> PutUser(string username, string password, bool administrator);

        /// <summary>
        /// Partially modify a user of given username.
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

        public UserService(ILogger<UserService> logger, IMemoryCache memoryCache, DatabaseContext databaseContext, IJwtService jwtService, IPasswordService passwordService)
        {
            _logger = logger;
            _memoryCache = memoryCache;
            _databaseContext = databaseContext;
            _jwtService = jwtService;
            _passwordService = passwordService;
        }

        private string GenerateCacheKeyByUserId(long id) => $"user:{id}";

        private void RemoveCache(long id)
        {
            _memoryCache.Remove(GenerateCacheKeyByUserId(id));
        }

        public async Task<CreateTokenResult> CreateToken(string username, string password)
        {
            if (username == null)
                throw new ArgumentNullException(nameof(username));
            if (password == null)
                throw new ArgumentNullException(nameof(password));

            // We need password info, so always check the database.
            var user = await _databaseContext.Users.Where(u => u.Name == username).SingleOrDefaultAsync();

            if (user == null)
            {
                var e = new UserNotExistException();
                _logger.LogInformation(e, $"Create token failed. Reason: invalid username. Username = {username} Password = {password} .");
                throw e;
            }

            if (!_passwordService.VerifyPassword(user.EncryptedPassword, password))
            {
                var e = new BadPasswordException();
                _logger.LogInformation(e, $"Create token failed. Reason: invalid password. Username = {username} Password = {password} .");
                throw e;
            }

            var token = _jwtService.GenerateJwtToken(new TokenInfo
            {
                Id = user.Id,
                Version = user.Version
            });
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
            try
            {
                tokenInfo = _jwtService.VerifyJwtToken(token);
            }
            catch (JwtTokenVerifyException e)
            {
                _logger.LogInformation(e, $"Verify token falied. Reason: invalid token. Token: {token} .");
                throw e;
            }

            var id = tokenInfo.Id;
            var key = GenerateCacheKeyByUserId(id);
            if (!_memoryCache.TryGetValue<UserCache>(key, out var cache))
            {
                // no cache, check the database
                var user = await _databaseContext.Users.Where(u => u.Id == id).SingleOrDefaultAsync();

                if (user == null)
                {
                    var e = new UserNotExistException();
                    _logger.LogInformation(e, $"Verify token falied. Reason: invalid id. Token: {token} Id: {id}.");
                    throw e;
                }

                // create cache
                cache = CreateUserCache(user);
                _memoryCache.CreateEntry(key).SetValue(cache);
            }

            if (tokenInfo.Version != cache.Version)
            {
                var e = new BadTokenVersionException();
                _logger.LogInformation(e, $"Verify token falied. Reason: invalid version. Token: {token} Id: {id} Username: {cache.Username} Version: {tokenInfo.Version} Version in cache: {cache.Version}.");
                throw e;
            }

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

            var user = await _databaseContext.Users.Where(u => u.Name == username).SingleOrDefaultAsync();

            if (user == null)
            {
                await _databaseContext.AddAsync(new User
                {
                    Name = username,
                    EncryptedPassword = _passwordService.HashPassword(password),
                    RoleString = IsAdminToRoleString(administrator),
                    Version = 0
                });
                await _databaseContext.SaveChangesAsync();
                return PutResult.Created;
            }

            user.EncryptedPassword = _passwordService.HashPassword(password);
            user.RoleString = IsAdminToRoleString(administrator);
            user.Version += 1;
            await _databaseContext.SaveChangesAsync();

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
                throw new UserNotExistException();

            bool modified = false;

            if (password != null)
            {
                modified = true;
                user.EncryptedPassword = _passwordService.HashPassword(password);
            }

            if (administrator != null)
            {
                modified = true;
                user.RoleString = IsAdminToRoleString(administrator.Value);
            }

            if (modified)
            {
                user.Version += 1;
                await _databaseContext.SaveChangesAsync();
                //clear cache
                RemoveCache(user.Id);
            }
        }

        public async Task DeleteUser(string username)
        {
            if (username == null)
                throw new ArgumentNullException(nameof(username));

            var user = await _databaseContext.Users.Where(u => u.Name == username).SingleOrDefaultAsync();
            if (user == null)
                throw new UserNotExistException();

            _databaseContext.Users.Remove(user);
            await _databaseContext.SaveChangesAsync();
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
                throw new UserNotExistException();

            var verifyResult = _passwordService.VerifyPassword(user.EncryptedPassword, oldPassword);
            if (!verifyResult)
                throw new BadPasswordException();

            user.EncryptedPassword = _passwordService.HashPassword(newPassword);
            user.Version += 1;
            await _databaseContext.SaveChangesAsync();
            //clear cache
            RemoveCache(user.Id);
        }
    }
}
