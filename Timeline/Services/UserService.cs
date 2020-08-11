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
using Timeline.Services.Exceptions;
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
        /// <exception cref="ArgumentException">Thrown when <paramref name="username"/> is of bad format or <paramref name="password"/> is empty.</exception>
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
        /// Get the user id of given username.
        /// </summary>
        /// <param name="username">Username of the user.</param>
        /// <returns>The id of the user.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="username"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="username"/> is of bad format.</exception>
        /// <exception cref="UserNotExistException">Thrown when the user with given username does not exist.</exception>
        Task<long> GetUserIdByUsername(string username);

        /// <summary>
        /// List all users.
        /// </summary>
        /// <returns>The user info of users.</returns>
        Task<User[]> GetUsers();

        /// <summary>
        /// Create a user with given info.
        /// </summary>
        /// <param name="info">The info of new user.</param>
        /// <param name="password">The password, can't be null or empty.</param>
        /// <returns>The the new user.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="info"/>is null.</exception>
        /// <exception cref="ArgumentException">Thrown when some fields in <paramref name="info"/> is bad.</exception>
        /// <exception cref="ConflictException">Thrown when a user with given username already exists.</exception>
        /// <remarks>
        /// <see cref="User.Username"/> must not be null and must be a valid username.
        /// <see cref="User.Password"/> must not be null or empty.
        /// <see cref="User.Administrator"/> is false by default (null).
        /// <see cref="User.Nickname"/> must be a valid nickname if set. It is empty by default.
        /// Other fields are ignored.
        /// </remarks>
        Task<User> CreateUser(User info);

        /// <summary>
        /// Modify a user's info.
        /// </summary>
        /// <param name="id">The id of the user.</param>
        /// <param name="info">The new info. May be null.</param>
        /// <returns>The new user info.</returns>
        /// <exception cref="ArgumentException">Thrown when some fields in <paramref name="info"/> is bad.</exception>
        /// <exception cref="UserNotExistException">Thrown when user with given id does not exist.</exception>
        /// <remarks>
        /// Only <see cref="User.Username"/>, <see cref="User.Administrator"/>, <see cref="User.Password"/> and <see cref="User.Nickname"/> will be used.
        /// If null, then not change.
        /// Other fields are ignored.
        /// Version will increase if password is changed.
        /// 
        /// <see cref="User.Username"/> must be a valid username if set.
        /// <see cref="User.Password"/> can't be empty if set.
        /// <see cref="User.Nickname"/> must be a valid nickname if set.
        /// 
        /// </remarks>
        /// <seealso cref="ModifyUser(string, User)"/>
        Task<User> ModifyUser(long id, User? info);

        /// <summary>
        /// Modify a user's info.
        /// </summary>
        /// <param name="username">The username of the user.</param>
        /// <param name="info">The new info. May be null.</param>
        /// <returns>The new user info.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="username"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="username"/> is of bad format or some fields in <paramref name="info"/> is bad.</exception>
        /// <exception cref="UserNotExistException">Thrown when user with given id does not exist.</exception>
        /// <exception cref="ConflictException">Thrown when user with the newusername already exist.</exception>
        /// <remarks>
        /// Only <see cref="User.Administrator"/>, <see cref="User.Password"/> and <see cref="User.Nickname"/> will be used.
        /// If null, then not change.
        /// Other fields are ignored.
        /// After modified, even if nothing is changed, version will increase.
        /// 
        /// <see cref="User.Username"/> must be a valid username if set.
        /// <see cref="User.Password"/> can't be empty if set.
        /// <see cref="User.Nickname"/> must be a valid nickname if set.
        /// 
        /// Note: Whether <see cref="User.Version"/> is set or not, version will increase and not set to the specified value if there is one.
        /// </remarks>
        /// <seealso cref="ModifyUser(long, User)"/>
        Task<User> ModifyUser(string username, User? info);

        /// <summary>
        /// Try to change a user's password with old password.
        /// </summary>
        /// <param name="id">The id of user to change password of.</param>
        /// <param name="oldPassword">Old password.</param>
        /// <param name="newPassword">New password.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="oldPassword"/> or <paramref name="newPassword"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="oldPassword"/> or <paramref name="newPassword"/> is empty.</exception>
        /// <exception cref="UserNotExistException">Thrown if the user with given username does not exist.</exception>
        /// <exception cref="BadPasswordException">Thrown if the old password is wrong.</exception>
        Task ChangePassword(long id, string oldPassword, string newPassword);
    }

    public class UserService : IUserService
    {
        private readonly ILogger<UserService> _logger;
        private readonly IClock _clock;

        private readonly DatabaseContext _databaseContext;

        private readonly IPasswordService _passwordService;

        private readonly UsernameValidator _usernameValidator = new UsernameValidator();
        private readonly NicknameValidator _nicknameValidator = new NicknameValidator();
        public UserService(ILogger<UserService> logger, DatabaseContext databaseContext, IPasswordService passwordService, IClock clock)
        {
            _logger = logger;
            _clock = clock;
            _databaseContext = databaseContext;
            _passwordService = passwordService;
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

        private static User CreateUserFromEntity(UserEntity entity)
        {
            return new User
            {
                UniqueId = entity.UniqueId,
                Username = entity.Username,
                Administrator = UserRoleConvert.ToBool(entity.Roles),
                Nickname = string.IsNullOrEmpty(entity.Nickname) ? entity.Username : entity.Nickname,
                Id = entity.Id,
                Version = entity.Version,
                CreateTime = entity.CreateTime,
                UsernameChangeTime = entity.UsernameChangeTime,
                LastModified = entity.LastModified
            };
        }

        public async Task<User> VerifyCredential(string username, string password)
        {
            if (username == null)
                throw new ArgumentNullException(nameof(username));
            if (password == null)
                throw new ArgumentNullException(nameof(password));

            CheckUsernameFormat(username, nameof(username));
            CheckPasswordFormat(password, nameof(password));

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

        public async Task<long> GetUserIdByUsername(string username)
        {
            if (username == null)
                throw new ArgumentNullException(nameof(username));

            CheckUsernameFormat(username, nameof(username));

            var entity = await _databaseContext.Users.Where(user => user.Username == username).Select(u => new { u.Id }).SingleOrDefaultAsync();

            if (entity == null)
                throw new UserNotExistException(username);

            return entity.Id;
        }

        public async Task<User[]> GetUsers()
        {
            var entities = await _databaseContext.Users.ToArrayAsync();
            return entities.Select(user => CreateUserFromEntity(user)).ToArray();
        }

        public async Task<User> CreateUser(User info)
        {
            if (info == null)
                throw new ArgumentNullException(nameof(info));

            if (info.Username == null)
                throw new ArgumentException(ExceptionUsernameNull, nameof(info));
            CheckUsernameFormat(info.Username, nameof(info));

            if (info.Password == null)
                throw new ArgumentException(ExceptionPasswordNull, nameof(info));
            CheckPasswordFormat(info.Password, nameof(info));

            if (info.Nickname != null)
                CheckNicknameFormat(info.Nickname, nameof(info));

            var username = info.Username;

            var conflict = await _databaseContext.Users.AnyAsync(u => u.Username == username);
            if (conflict)
                ThrowUsernameConflict();

            var administrator = info.Administrator ?? false;
            var password = info.Password;
            var nickname = info.Nickname;

            var newEntity = new UserEntity
            {
                Username = username,
                Password = _passwordService.HashPassword(password),
                Roles = UserRoleConvert.ToString(administrator),
                Nickname = nickname,
                Version = 1
            };
            _databaseContext.Users.Add(newEntity);
            await _databaseContext.SaveChangesAsync();

            _logger.LogInformation(Log.Format(LogDatabaseCreate,
                ("Id", newEntity.Id), ("Username", username), ("Administrator", administrator)));

            return CreateUserFromEntity(newEntity);
        }

        private void ValidateModifyUserInfo(User? info)
        {
            if (info != null)
            {
                if (info.Username != null)
                    CheckUsernameFormat(info.Username, nameof(info));

                if (info.Password != null)
                    CheckPasswordFormat(info.Password, nameof(info));

                if (info.Nickname != null)
                    CheckNicknameFormat(info.Nickname, nameof(info));
            }
        }

        private async Task UpdateUserEntity(UserEntity entity, User? info)
        {
            if (info != null)
            {
                var now = _clock.GetCurrentTime();
                bool updateLastModified = false;

                var username = info.Username;
                if (username != null && username != entity.Username)
                {
                    var conflict = await _databaseContext.Users.AnyAsync(u => u.Username == username);
                    if (conflict)
                        ThrowUsernameConflict();

                    entity.Username = username;
                    entity.UsernameChangeTime = now;
                    updateLastModified = true;
                }

                var password = info.Password;
                if (password != null)
                {
                    entity.Password = _passwordService.HashPassword(password);
                    entity.Version += 1;
                }

                var administrator = info.Administrator;
                if (administrator.HasValue && UserRoleConvert.ToBool(entity.Roles) != administrator)
                {
                    entity.Roles = UserRoleConvert.ToString(administrator.Value);
                    updateLastModified = true;
                }

                var nickname = info.Nickname;
                if (nickname != null && nickname != entity.Nickname)
                {
                    entity.Nickname = nickname;
                    updateLastModified = true;
                }

                if (updateLastModified)
                {
                    entity.LastModified = now;
                }
            }
        }


        public async Task<User> ModifyUser(long id, User? info)
        {
            ValidateModifyUserInfo(info);

            var entity = await _databaseContext.Users.Where(u => u.Id == id).SingleOrDefaultAsync();
            if (entity == null)
                throw new UserNotExistException(id);

            await UpdateUserEntity(entity, info);

            await _databaseContext.SaveChangesAsync();
            _logger.LogInformation(LogDatabaseUpdate, ("Id", id));

            return CreateUserFromEntity(entity);
        }

        public async Task<User> ModifyUser(string username, User? info)
        {
            if (username == null)
                throw new ArgumentNullException(nameof(username));
            CheckUsernameFormat(username, nameof(username));

            ValidateModifyUserInfo(info);

            var entity = await _databaseContext.Users.Where(u => u.Username == username).SingleOrDefaultAsync();
            if (entity == null)
                throw new UserNotExistException(username);

            await UpdateUserEntity(entity, info);

            await _databaseContext.SaveChangesAsync();
            _logger.LogInformation(LogDatabaseUpdate, ("Username", username));

            return CreateUserFromEntity(entity);
        }

        public async Task ChangePassword(long id, string oldPassword, string newPassword)
        {
            if (oldPassword == null)
                throw new ArgumentNullException(nameof(oldPassword));
            if (newPassword == null)
                throw new ArgumentNullException(nameof(newPassword));
            CheckPasswordFormat(oldPassword, nameof(oldPassword));
            CheckPasswordFormat(newPassword, nameof(newPassword));

            var entity = await _databaseContext.Users.Where(u => u.Id == id).SingleOrDefaultAsync();

            if (entity == null)
                throw new UserNotExistException(id);

            if (!_passwordService.VerifyPassword(entity.Password, oldPassword))
                throw new BadPasswordException(oldPassword);

            entity.Password = _passwordService.HashPassword(newPassword);
            entity.Version += 1;
            await _databaseContext.SaveChangesAsync();
            _logger.LogInformation(Log.Format(LogDatabaseUpdate, ("Id", id), ("Operation", "Change password")));
        }
    }
}
