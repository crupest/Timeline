using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Timeline.Entities;
using Timeline.Models.Validation;
using Timeline.Services.Timeline;

namespace Timeline.Services.User
{
    public class UserDeleteService : IUserDeleteService
    {
        private readonly ILogger<UserDeleteService> _logger;

        private readonly DatabaseContext _databaseContext;

        private readonly ITimelinePostService _timelinePostService;

        private readonly UsernameValidator _usernameValidator = new UsernameValidator();

        public UserDeleteService(ILogger<UserDeleteService> logger, DatabaseContext databaseContext, ITimelinePostService timelinePostService)
        {
            _logger = logger;
            _databaseContext = databaseContext;
            _timelinePostService = timelinePostService;
        }

        public async Task<bool> DeleteUser(string username)
        {
            if (username == null)
                throw new ArgumentNullException(nameof(username));

            if (!_usernameValidator.Validate(username, out var message))
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Resource.ExceptionUsernameBadFormat, message), nameof(username));
            }

            var user = await _databaseContext.Users.Where(u => u.Username == username).SingleOrDefaultAsync();
            if (user == null)
                return false;

            if (user.Id == 1)
                throw new InvalidOperationOnRootUserException(Resource.ExceptionDeleteRootUser);

            await _timelinePostService.DeleteAllPostsOfUser(user.Id);

            _databaseContext.Users.Remove(user);

            await _databaseContext.SaveChangesAsync();
            _logger.LogWarning(Resource.LogDeleteUser, user.Username, user.Id);

            return true;
        }

    }
}
