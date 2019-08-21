using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using Timeline.Entities;
using Timeline.Models;

namespace Timeline.Services
{
    public interface IUserDetailService
    {
        /// <summary>
        /// Get the detail of user.
        /// </summary>
        /// <param name="username">The username to get user detail of.</param>
        /// <returns>The user detail.</returns>
        /// <exception cref="ArgumentException">Thrown if <paramref name="username"/> is null or empty.</exception>
        /// <exception cref="UserNotExistException">Thrown if user doesn't exist.</exception>
        Task<UserDetail> GetUserDetail(string username);

        /// <summary>
        /// Update the detail of user. This function does not do data check.
        /// </summary>
        /// <param name="username">The username to get user detail of.</param>
        /// <param name="detail">The detail to update. Can't be null. Any null member means not set.</param>
        /// <exception cref="ArgumentException">Thrown if <paramref name="username"/> is null or empty or <paramref name="detail"/> is null.</exception>
        /// <exception cref="UserNotExistException">Thrown if user doesn't exist.</exception>
        Task UpdateUserDetail(string username, UserDetail detail);
    }

    public class UserDetailService : IUserDetailService
    {
        private readonly ILogger<UserDetailService> _logger;

        private readonly DatabaseContext _databaseContext;

        public UserDetailService(ILogger<UserDetailService> logger, DatabaseContext databaseContext)
        {
            _logger = logger;
            _databaseContext = databaseContext;
        }

        // Check the existence of user detail entry
        private async Task<UserDetailEntity> CheckAndInit(long userId)
        {
            var detail = await _databaseContext.UserDetails.Where(e => e.UserId == userId).SingleOrDefaultAsync();
            if (detail == null)
            {
                detail = new UserDetailEntity()
                {
                    UserId = userId
                };
                _databaseContext.UserDetails.Add(detail);
                await _databaseContext.SaveChangesAsync();
            }
            return detail;
        }

        public async Task<UserDetail> GetUserDetail(string username)
        {
            var userId = await DatabaseExtensions.CheckAndGetUser(_databaseContext.Users, username);
            var detailEntity = await CheckAndInit(userId);
            return UserDetail.From(detailEntity);
        }

        public async Task UpdateUserDetail(string username, UserDetail detail)
        {
            if (detail == null)
                throw new ArgumentNullException(nameof(detail));

            var userId = await DatabaseExtensions.CheckAndGetUser(_databaseContext.Users, username);
            var detailEntity = await CheckAndInit(userId);

            if (detail.QQ != null)
                detailEntity.QQ = detail.QQ;

            if (detail.EMail != null)
                detailEntity.EMail = detail.EMail;

            if (detail.PhoneNumber != null)
                detailEntity.PhoneNumber = detail.PhoneNumber;

            if (detail.Description != null)
                detailEntity.Description = detail.Description;

            await _databaseContext.SaveChangesAsync();
        }
    }
}
