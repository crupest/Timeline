using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Timeline.Entities;
using Timeline.Helpers;
using Timeline.Models.Validation;
using Timeline.Services.Exceptions;
using static Timeline.Resources.Services.UserService;

namespace Timeline.Services
{
    public interface IUserDeleteService
    {
        /// <summary>
        /// Delete a user of given username.
        /// </summary>
        /// <param name="username">Username of the user to delete. Can't be null.</param>
        /// <returns>True if user is deleted, false if user not exist.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="username"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="username"/> is of bad format.</exception>
        /// <exception cref="InvalidOperationOnRootUserException">Thrown when deleting root user.</exception>
        Task<bool> DeleteUser(string username);
    }

    public class UserDeleteService : IUserDeleteService
    {
        private readonly ILogger<UserDeleteService> _logger;

        private readonly DatabaseContext _databaseContext;

        private readonly ITimelineService _timelineService;

        private readonly UsernameValidator _usernameValidator = new UsernameValidator();

        public UserDeleteService(ILogger<UserDeleteService> logger, DatabaseContext databaseContext, ITimelineService timelineService)
        {
            _logger = logger;
            _databaseContext = databaseContext;
            _timelineService = timelineService;
        }

        public async Task<bool> DeleteUser(string username)
        {
            if (username == null)
                throw new ArgumentNullException(nameof(username));

            if (!_usernameValidator.Validate(username, out var message))
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, ExceptionUsernameBadFormat, message), nameof(username));
            }

            var user = await _databaseContext.Users.Where(u => u.Username == username).SingleOrDefaultAsync();
            if (user == null)
                return false;

            if (user.Id == 1)
                throw new InvalidOperationOnRootUserException("Can't delete root user.");

            await _timelineService.DeleteAllPostsOfUser(user.Id);

            _databaseContext.Users.Remove(user);

            await _databaseContext.SaveChangesAsync();
            _logger.LogInformation(Log.Format(LogDatabaseRemove, ("Id", user.Id), ("Username", user.Username)));

            return true;
        }

    }
}
