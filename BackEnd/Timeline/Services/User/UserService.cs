using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Timeline.Entities;
using Timeline.Models.Validation;

namespace Timeline.Services.User
{
    public class UserService : BasicUserService, IUserService
    {
        private readonly ILogger<UserService> _logger;
        private readonly IClock _clock;

        private readonly DatabaseContext _databaseContext;

        private readonly IPasswordService _passwordService;

        private readonly UsernameValidator _usernameValidator = new UsernameValidator();
        private readonly NicknameValidator _nicknameValidator = new NicknameValidator();

        public UserService(ILogger<UserService> logger, DatabaseContext databaseContext, IPasswordService passwordService, IClock clock) : base(databaseContext)
        {
            _logger = logger;
            _databaseContext = databaseContext;
            _passwordService = passwordService;
            _clock = clock;
        }

        private void CheckUsernameFormat(string username, string? paramName)
        {
            if (!_usernameValidator.Validate(username, out var message))
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Resource.ExceptionUsernameBadFormat, message), paramName);
            }
        }

        private static void CheckPasswordFormat(string password, string? paramName)
        {
            if (password.Length == 0)
            {
                throw new ArgumentException(Resource.ExceptionPasswordEmpty, paramName);
            }
        }

        private void CheckNicknameFormat(string nickname, string? paramName)
        {
            if (!_nicknameValidator.Validate(nickname, out var message))
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Resource.ExceptionNicknameBadFormat, message), paramName);
            }
        }

        private static void ThrowUsernameConflict(object? user)
        {
            throw new UserAlreadyExistException(user);
        }

        public async Task<UserEntity> GetUserAsync(long id)
        {
            var user = await _databaseContext.Users.Where(u => u.Id == id).SingleOrDefaultAsync();

            if (user is null)
                throw new UserNotExistException(id);

            return user;
        }

        public async Task<List<UserEntity>> GetUsersAsync()
        {
            return await _databaseContext.Users.ToListAsync();
        }

        public async Task<UserEntity> CreateUserAsync(CreateUserParams param)
        {
            if (param is null)
                throw new ArgumentNullException(nameof(param));
            if (param.Username is null)
                throw new ArgumentException(Resource.ExceptionUsernameNull, nameof(param));
            if (param.Password is null)
                throw new ArgumentException(Resource.ExceptionPasswordNull, nameof(param));
            CheckUsernameFormat(param.Username, nameof(param));
            CheckPasswordFormat(param.Password, nameof(param));
            if (param.Nickname is not null)
                CheckNicknameFormat(param.Nickname, nameof(param));

            var conflict = await _databaseContext.Users.AnyAsync(u => u.Username == param.Username);
            if (conflict)
                ThrowUsernameConflict(null);

            var newEntity = new UserEntity
            {
                Username = param.Username,
                Password = _passwordService.HashPassword(param.Password),
                Nickname = param.Nickname,
                Version = 1
            };
            _databaseContext.Users.Add(newEntity);
            await _databaseContext.SaveChangesAsync();
            _logger.LogInformation(Resource.LogUserCreated, param.Username, newEntity.Id);

            return newEntity;
        }

        public async Task<UserEntity> ModifyUserAsync(long id, ModifyUserParams? param)
        {
            if (param is not null)
            {
                if (param.Username is not null)
                    CheckUsernameFormat(param.Username, nameof(param));

                if (param.Password is not null)
                    CheckPasswordFormat(param.Password, nameof(param));

                if (param.Nickname is not null)
                    CheckNicknameFormat(param.Nickname, nameof(param));
            }

            var entity = await _databaseContext.Users.Where(u => u.Id == id).SingleOrDefaultAsync();
            if (entity == null)
                throw new UserNotExistException(id);

            if (param is not null)
            {
                var now = _clock.GetCurrentTime();
                bool updateLastModified = false;

                var username = param.Username;
                if (username is not null && username != entity.Username)
                {
                    var conflict = await _databaseContext.Users.AnyAsync(u => u.Username == username);
                    if (conflict)
                        ThrowUsernameConflict(null);

                    entity.Username = username;
                    entity.UsernameChangeTime = now;
                    updateLastModified = true;
                }

                var password = param.Password;
                if (password is not null)
                {
                    entity.Password = _passwordService.HashPassword(password);
                    entity.Version += 1;
                }

                var nickname = param.Nickname;
                if (nickname is not null && nickname != entity.Nickname)
                {
                    entity.Nickname = nickname;
                    updateLastModified = true;
                }

                if (updateLastModified)
                {
                    entity.LastModified = now;
                }

                await _databaseContext.SaveChangesAsync();
                _logger.LogInformation(Resource.LogUserModified, entity.Username, id);
            }

            return entity;
        }

        public async Task<long> VerifyCredential(string username, string password)
        {
            if (username is null)
                throw new ArgumentNullException(nameof(username));
            if (password is null)
                throw new ArgumentNullException(nameof(password));
            CheckUsernameFormat(username, nameof(username));
            CheckPasswordFormat(password, nameof(password));

            var entity = await _databaseContext.Users.Where(u => u.Username == username).Select(u => new { u.Id, u.Password }).SingleOrDefaultAsync();

            if (entity is null)
            {
                _logger.LogInformation(Resource.LogVerifyCredentialsUsernameBad, username);
                throw new UserNotExistException(username);
            }

            if (!_passwordService.VerifyPassword(entity.Password, password))
            {
                _logger.LogInformation(Resource.LogVerifyCredentialsPasswordBad, username);
                throw new BadPasswordException(password);
            }

            return entity.Id;
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

            if (entity is null)
                throw new UserNotExistException(id);

            if (!_passwordService.VerifyPassword(entity.Password, oldPassword))
                throw new BadPasswordException(oldPassword);

            entity.Password = _passwordService.HashPassword(newPassword);
            entity.Version += 1;
            await _databaseContext.SaveChangesAsync();
            _logger.LogInformation(Resource.LogChangePassowrd, entity.Username, id);
        }
    }
}
