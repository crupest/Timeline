using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Localization;
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
        public string Token { get; set; } = default!;
        public UserInfo User { get; set; } = default!;
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
        /// <exception cref="JwtVerifyException">Thrown when the token is of bad format. Thrown by <see cref="JwtService.VerifyJwtToken(string)"/>.</exception>
        /// <exception cref="UserNotExistException">Thrown when the user specified by the token does not exist. Usually it has been deleted after the token was issued.</exception>
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
        /// <exception cref="UserNotExistException">Thrown if the user with given username does not exist.</exception>
        Task PatchUser(string username, string? password, bool? administrator);

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
        /// <exception cref="UsernameConfictException">Thrown if user with the new username already exists.</exception>
        Task ChangeUsername(string oldUsername, string newUsername);
    }

    internal class UserCache
    {
        public string Username { get; set; } = default!;
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
                throw new JwtVerifyException(new JwtBadVersionException(tokenInfo.Version, cache.Version), JwtVerifyException.ErrorCodes.OldVersion);

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

            var (result, messageGenerator) = _usernameValidator.Validate(username);
            if (!result)
            {
                throw new UsernameBadFormatException(username, messageGenerator(null));
            }

            var user = await _databaseContext.Users.Where(u => u.Name == username).SingleOrDefaultAsync();

            if (user == null)
            {
                var newUser = new User
                {
                    Name = username,
                    EncryptedPassword = _passwordService.HashPassword(password),
                    RoleString = IsAdminToRoleString(administrator),
                    Avatar = UserAvatar.Create(DateTime.Now)
                };
                await _databaseContext.AddAsync(newUser);
                await _databaseContext.SaveChangesAsync();
                _logger.LogInformation(FormatLogMessage("A new user entry is added to the database.", Pair("Id", newUser.Id)));
                return PutResult.Create;
            }

            user.EncryptedPassword = _passwordService.HashPassword(password);
            user.RoleString = IsAdminToRoleString(administrator);
            user.Version += 1;
            await _databaseContext.SaveChangesAsync();
            _logger.LogInformation(FormatLogMessage("A user entry is updated to the database.", Pair("Id", user.Id)));

            //clear cache
            RemoveCache(user.Id);

            return PutResult.Modify;
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


            var (result, messageGenerator) = _usernameValidator.Validate(newUsername);
            if (!result)
            {
                throw new UsernameBadFormatException(newUsername, $"New username is of bad format. {messageGenerator(null)}");
            }

            var user = await _databaseContext.Users.Where(u => u.Name == oldUsername).SingleOrDefaultAsync();
            if (user == null)
                throw new UserNotExistException(oldUsername);

            var conflictUser = await _databaseContext.Users.Where(u => u.Name == newUsername).SingleOrDefaultAsync();
            if (conflictUser != null)
                throw new UsernameConfictException(newUsername);

            user.Name = newUsername;
            user.Version += 1;
            await _databaseContext.SaveChangesAsync();
            _logger.LogInformation(FormatLogMessage("A user entry changed name field.",
                Pair("Id", user.Id), Pair("Old Username", oldUsername), Pair("New Username", newUsername)));
            RemoveCache(user.Id);
        }
    }
}
