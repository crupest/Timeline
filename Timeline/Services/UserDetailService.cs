using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using Timeline.Entities;
using static Timeline.Resources.Services.UserDetailService;

namespace Timeline.Services
{
    public interface IUserDetailService
    {
        /// <summary>
        /// Get the nickname of the user with given username.
        /// If the user does not set a nickname, the username is returned as the nickname.
        /// </summary>
        /// <param name="username">The username of the user to get nickname of.</param>
        /// <returns>The nickname of the user.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="username"/> is null.</exception>
        /// <exception cref="UsernameBadFormatException">Thrown when <paramref name="username"/> is of bad format.</exception>
        /// <exception cref="UserNotExistException">Thrown when the user does not exist.</exception>
        Task<string> GetNickname(string username);

        /// <summary>
        /// Set the nickname of the user with given username.
        /// </summary>
        /// <param name="username">The username of the user to set nickname of.</param>
        /// <param name="nickname">The nickname. Pass null to unset.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="username"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="nickname"/> is not null but its length is bigger than 10.</exception>
        /// <exception cref="UsernameBadFormatException">Thrown when <paramref name="username"/> is of bad format.</exception>
        /// <exception cref="UserNotExistException">Thrown when the user does not exist.</exception>
        Task SetNickname(string username, string? nickname);
    }

    public class UserDetailService : IUserDetailService
    {
        private readonly DatabaseContext _database;

        private readonly ILogger<UserDetailService> _logger;

        public UserDetailService(DatabaseContext database, ILogger<UserDetailService> logger)
        {
            _database = database;
            _logger = logger;
        }

        public async Task<string> GetNickname(string username)
        {
            var userId = await DatabaseExtensions.CheckAndGetUser(_database.Users, username);
            var nickname = _database.UserDetails.Where(d => d.UserId == userId).Select(d => new { d.Nickname }).SingleOrDefault()?.Nickname;
            return nickname ?? username;
        }

        public async Task SetNickname(string username, string? nickname)
        {
            if (nickname != null && nickname.Length > 10)
            {
                throw new ArgumentException(ExceptionNicknameTooLong, nameof(nickname));
            }
            var userId = await DatabaseExtensions.CheckAndGetUser(_database.Users, username);
            var userDetail = _database.UserDetails.Where(d => d.UserId == userId).SingleOrDefault();
            if (nickname == null)
            {
                if (userDetail == null || userDetail.Nickname == null)
                {
                    return;
                }
                else
                {
                    userDetail.Nickname = null;
                    await _database.SaveChangesAsync();
                    _logger.LogInformation(LogEntityNicknameSetToNull, userId);
                }
            }
            else
            {
                var create = userDetail == null;
                if (create)
                {
                    userDetail = new UserDetail
                    {
                        UserId = userId
                    };
                }
                userDetail!.Nickname = nickname;
                if (create)
                {
                    _database.UserDetails.Add(userDetail);
                }
                await _database.SaveChangesAsync();
                if (create)
                {
                    _logger.LogInformation(LogEntityNicknameCreate, userId, nickname);
                }
                else
                {
                    _logger.LogInformation(LogEntityNicknameSetNotNull, userId, nickname);
                }
            }
        }
    }
}
