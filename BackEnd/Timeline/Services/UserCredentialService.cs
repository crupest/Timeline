using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using Timeline.Entities;
using Timeline.Helpers;
using Timeline.Models.Validation;
using Timeline.Services.Exceptions;

namespace Timeline.Services
{
    public interface IUserCredentialService
    {
        /// <summary>
        /// Try to verify the given username and password.
        /// </summary>
        /// <param name="username">The username of the user to verify.</param>
        /// <param name="password">The password of the user to verify.</param>
        /// <returns>User id.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="username"/> or <paramref name="password"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="username"/> is of bad format or <paramref name="password"/> is empty.</exception>
        /// <exception cref="UserNotExistException">Thrown when the user with given username does not exist.</exception>
        /// <exception cref="BadPasswordException">Thrown when password is wrong.</exception>
        Task<long> VerifyCredential(string username, string password);

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

    public class UserCredentialService : IUserCredentialService
    {
        private readonly ILogger<UserCredentialService> _logger;
        private readonly DatabaseContext _database;
        private readonly IPasswordService _passwordService;

        private readonly UsernameValidator _usernameValidator = new UsernameValidator();

        public UserCredentialService(ILogger<UserCredentialService> logger, DatabaseContext database, IPasswordService passwordService)
        {
            _logger = logger;
            _database = database;
            _passwordService = passwordService;
        }

        public async Task<long> VerifyCredential(string username, string password)
        {
            if (username == null)
                throw new ArgumentNullException(nameof(username));
            if (password == null)
                throw new ArgumentNullException(nameof(password));
            if (!_usernameValidator.Validate(username, out var message))
                throw new ArgumentException(message);
            if (password.Length == 0)
                throw new ArgumentException("Password can't be empty.");

            var entity = await _database.Users.Where(u => u.Username == username).Select(u => new { u.Id, u.Password }).SingleOrDefaultAsync();

            if (entity == null)
                throw new UserNotExistException(username);

            if (!_passwordService.VerifyPassword(entity.Password, password))
                throw new BadPasswordException(password);

            return entity.Id;
        }

        public async Task ChangePassword(long id, string oldPassword, string newPassword)
        {
            if (oldPassword == null)
                throw new ArgumentNullException(nameof(oldPassword));
            if (newPassword == null)
                throw new ArgumentNullException(nameof(newPassword));
            if (oldPassword.Length == 0)
                throw new ArgumentException("Old password can't be empty.");
            if (newPassword.Length == 0)
                throw new ArgumentException("New password can't be empty.");

            var entity = await _database.Users.Where(u => u.Id == id).SingleOrDefaultAsync();

            if (entity == null)
                throw new UserNotExistException(id);

            if (!_passwordService.VerifyPassword(entity.Password, oldPassword))
                throw new BadPasswordException(oldPassword);

            entity.Password = _passwordService.HashPassword(newPassword);
            entity.Version += 1;
            await _database.SaveChangesAsync();
            _logger.LogInformation(Log.Format(Resources.Services.UserService.LogDatabaseUpdate, ("Id", id), ("Operation", "Change password")));
        }
    }
}
